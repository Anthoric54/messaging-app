using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Ocelot
builder.Configuration.AddJsonFile("ocelot-configuration.json", optional: false, reloadOnChange: true);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.WithOrigins("http://localhost:3000") 
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Configuration des services
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5000",
            ValidAudience = "messaging_api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSecret")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"message\": \"Token invalide ou expir�.\"}");
            }
        };
    });


builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAll");

// Middleware Ocelot
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot();

app.Run();
