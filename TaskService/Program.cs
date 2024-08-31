using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskService.DataStorage;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    app.Use(async (context, next) =>
{
    logger.LogInformation("Request: {method} {url}", context.Request.Method, context.Request.Path);
    await next.Invoke();
});
}
app.MapControllers();

app.Run();

static void ConfigureServices(IServiceCollection services)
{
    var secretKey = Environment.GetEnvironmentVariable("SecretKey") ?? throw new ArgumentException("SecretKey");
    //var redisConfiguration = Environment.GetEnvironmentVariable("Redis:Configuration");

    services.AddDbContext<TaskDbContext>(options =>
        options.UseNpgsql(Environment.GetEnvironmentVariable("DefaultConnection")));

    //services.AddStackExchangeRedisCache(options =>
    //{
    //    options.Configuration = redisConfiguration;
    //});

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            };
        });
    services.AddAuthorization();
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(opt => {
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        }
        );
        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
            }
        });
    });
}