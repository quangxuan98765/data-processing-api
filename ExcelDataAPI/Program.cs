using ExcelDataAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add custom services
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<DataService>();

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
