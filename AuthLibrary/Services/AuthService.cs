using AuthLibrary.DTOs;
using AuthLibrary.Interfaces;
using AuthLibrary.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace AuthLibrary.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AuthService(IDatabaseService databaseService, IPasswordService passwordService, ITokenService tokenService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            try
            {
                // Use stored proc to get user
                var userTable = await _databaseService.ExecuteStoredProcAsync("sp_GetUserByUsername", new { Username = request.Username });
                if (userTable.Rows.Count == 0)
                    return AuthResult.Failed("Invalid username or password");

                var user = MapToUser(userTable.Rows[0]);

                // Verify password (bcrypt/argon2 done in IPasswordService)
                if (!_passwordService.VerifyPassword(request.Password, user.Password))
                    return AuthResult.Failed("Invalid username or password");

                if (!user.IsActive)
                    return AuthResult.Failed("Account is disabled");

                // Generate token & expiry in app
                var token = _tokenService.GenerateToken(user); // raw token string
                var expiry = _tokenService.GetTokenExpiry(); // DateTime (UTC)

                // Store token in DB via SP (SingleSession optional: 0 = allow many)
                await _databaseService.ExecuteStoredProcAsync("sp_CreateAuthToken", new
                {
                    TokenKey = token,
                    IdUser = user.Id,
                    ExpireDate = expiry,
                    SingleSession = 0
                });

                // Update last login (async, best-effort)
                _ = _databaseService.ExecuteStoredProcAsync("sp_UpdateLastLogin", new { UserId = user.Id });

                var response = new LoginResponse
                {
                    Success = true,
                    Token = token,
                    ExpiresAt = expiry,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsStaff = user.IsStaff,
                        IsSuperuser = user.IsSuperuser
                    }
                };

                return AuthResult.Success(response);
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Authentication error: {ex.Message}");
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check username/email exist via SP or simple query
                var existsByName = await _databaseService.ExecuteStoredProcAsync("sp_GetUserByUsername", new { Username = request.Username });
                if (existsByName.Rows.Count > 0) return AuthResult.Failed("Username already exists");

                var existsByEmail = await _databaseService.ExecuteStoredProcAsync("sp_GetUserByEmail", new { Email = request.Email });
                if (existsByEmail.Rows.Count > 0) return AuthResult.Failed("Email already exists");

                // Hash password in app
                var hashedPassword = _passwordService.HashPassword(request.Password);

                // Create user via SP
                var dt = await _databaseService.ExecuteStoredProcAsync("sp_CreateUser", new
                {
                    Username = request.Username,
                    Password = hashedPassword,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsStaff = false,
                    IsSuperuser = false,
                    IsActive = true,
                    DateJoined = DateTime.UtcNow
                });

                if (dt.Rows.Count == 0) return AuthResult.Failed("Failed to create user");

                var userId = Convert.ToInt32(dt.Rows[0]["UserId"]);
                var newUser = await GetUserByIdAsync(userId);
                if (newUser == null) return AuthResult.Failed("Failed to fetch created user");

                var info = new UserInfo
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    IsStaff = newUser.IsStaff,
                    IsSuperuser = newUser.IsSuperuser
                };

                return AuthResult.Success(info);
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Registration error: {ex.Message}");
            }
        }

        public async Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null) return AuthResult.Failed("User not found");

                if (!_passwordService.VerifyPassword(request.CurrentPassword, user.Password))
                    return AuthResult.Failed("Current password is incorrect");

                var newHash = _passwordService.HashPassword(request.NewPassword);

                var res = await _databaseService.ExecuteStoredProcAsync("sp_ChangePassword", new
                {
                    UserId = userId,
                    NewPassword = newHash
                });

                if (res.Rows.Count > 0 && Convert.ToInt32(res.Rows[0]["RowsAffected"]) > 0)
                {
                    // invalidate all user tokens
                    await _databaseService.ExecuteStoredProcAsync("sp_InvalidateUserTokens", new { UserId = userId });
                    return AuthResult.Success("Password changed");
                }

                return AuthResult.Failed("Failed to change password");
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Change password error: {ex.Message}");
            }
        }

        public async Task<AuthResult> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return AuthResult.Failed("Token is required");

                // Validate signature + expiry and get userId from token claims
                if (!_tokenService.ValidateToken(token, out int userId))
                    return AuthResult.Failed("Invalid token");

                // Load user (ensure active)
                var user = await GetUserByIdAsync(userId);
                if (user == null || !user.IsActive)
                    return AuthResult.Failed("User not found or inactive");

                var info = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsStaff = user.IsStaff,
                    IsSuperuser = user.IsSuperuser
                };

                return AuthResult.Success(info);
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Token validation error: {ex.Message}");
            }
        }


        public async Task<AuthResult> LogoutAsync(string token)
        {
            try
            {
                await _databaseService.ExecuteStoredProcAsync("sp_InvalidateToken", new { TokenKey = token });
                return AuthResult.Success("Logged out");
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Logout error: {ex.Message}");
            }
        }

        // helpers
        private async Task<User?> GetUserByIdAsync(int id)
        {
            var dt = await _databaseService.QueryAsync("SELECT * FROM auth_user WHERE id = @Id", new { Id = id });
            if (dt.Rows.Count == 0) return null;
            return MapToUser(dt.Rows[0]);
        }

        private static User MapToUser(DataRow row)
        {
            return new User
            {
                Id = Convert.ToInt32(row["id"]),
                Username = row["username"].ToString() ?? string.Empty,
                Password = row["password"].ToString() ?? string.Empty,
                Email = row["email"].ToString() ?? string.Empty,
                FirstName = row["first_name"].ToString() ?? string.Empty,
                LastName = row["last_name"].ToString() ?? string.Empty,
                IsStaff = Convert.ToBoolean(row["is_staff"]),
                IsSuperuser = Convert.ToBoolean(row["is_superuser"]),
                IsActive = Convert.ToBoolean(row["is_active"]),
                DateJoined = Convert.ToDateTime(row["date_joined"]),
                LastLogin = row["last_login"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["last_login"])
            };
        }
    }
}
