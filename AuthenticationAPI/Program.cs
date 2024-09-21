var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var connectionString = builder.Configuration.GetConnectionString("DbConnection");
Log.Logger = new LoggerConfiguration()
            .WriteTo
            .MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Tbl_LogEvents",
                    AutoCreateSqlTable = true
                })
            .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]!)),
        };
    });

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(connectionString);
}, ServiceLifetime.Scoped, ServiceLifetime.Scoped);


builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 401)
    {
        context.HttpContext.Response.ContentType = "application/json";
        var errorResponse = new
        {
            message = "Unauthorized! Token is invalid or expired.",
        };

        context.HttpContext.Response.StatusCode = 401;
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await context.HttpContext.Response.WriteAsync(jsonResponse);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    Log.Information($"Request: {context.Request.Method} {context.Request.Path}");

    await next();

    Log.Information($"Response: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting the API app host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}