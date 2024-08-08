using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using SharedModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Keycloak Работающая версия 2 с использованием AddOpenIdConnect...
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        c.RequireHttpsMetadata = false;
        c.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        c.Audience = "account";// $"{builder.Configuration["Keycloak:resource"]}"; ;

        c.TokenValidationParameters.ValidateIssuer = false;//отключаю временно, чтобы измежать конфликта (localhost:8080 и keycloak:8080) при работе с контейнерами докер 
    });
builder.Services.AddAuthorization();
#endregion

builder.Services.AddControllers();

var connectionString = Config.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<PortfolioDbContext>(opt =>
    opt.UseSqlServer(connectionString));
builder.Services.AddCors(c => c.AddPolicy("cors", opt =>
{
    opt.AllowAnyOrigin();
    //opt.AllowAnyHeader();
    //opt.AllowCredentials();
    //opt.AllowAnyMethod();
    //opt.WithOrigins(builder.Configuration.GetSection("Cors:Urls").Get<string[]>()!);
}));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("cors");

app.MapControllers();

app.Run();
