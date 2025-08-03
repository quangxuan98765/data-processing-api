using ExcelDataAPI.Infrastructure.Data;
using ExcelDataAPI.Infrastructure.Configuration;
using ExcelDataAPI.Application.Interfaces.Financial;
using ExcelDataAPI.Application.Services.Financial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============ CLEAN ARCHITECTURE REGISTRATION ============

// Configuration
var databaseSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();
if (databaseSettings == null)
{
    // Fallback to ConnectionStrings if DatabaseSettings not found
    databaseSettings = new DatabaseSettings
    {
        ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found"),
        CommandTimeout = 120,
        BulkInsertBatchSize = 1000,
        BulkCopyTimeout = 300,
        MaxRetryAttempts = 3,
        EnableConnectionPooling = true
    };
}
builder.Services.AddSingleton(databaseSettings);

// Infrastructure Layer
builder.Services.AddScoped<IDataProvider, SqlDataProvider>();

// Application Layer - TÁCH BIỆT Revenue và Expense
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// ============ END CLEAN ARCHITECTURE ============

// Add CORS for Power Automate
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPowerAutomate", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowPowerAutomate");
app.UseAuthorization();
app.MapControllers();

app.Run();
