using AuthService.DataStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

ConfigureServices(builder.Services);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();

static void ConfigureServices(IServiceCollection services)
{
    var secretKey = Environment.GetEnvironmentVariable("SecretKey") ?? throw new ArgumentException("SecretKey");

    services.AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(Environment.GetEnvironmentVariable("DefaultConnection")));

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            };
        });

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}