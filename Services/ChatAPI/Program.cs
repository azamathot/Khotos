using ChatAPI.Data;
using ChatAPI.Hubs;
using ChatAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedModels;
using SharedModels.Chats;

var builder = WebApplication.CreateBuilder(args);
Config.ConfigAppConfiguration(builder.Configuration);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddSignalR(x =>
{
    x.EnableDetailedErrors = true;
    x.MaximumReceiveMessageSize = HubNames.MaxFileSize;
});

#region Keycloak Работающая версия 2 с использованием JwtBearer...
string? token = string.Empty;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        c.RequireHttpsMetadata = false;
        c.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        c.Audience = "account";
        c.SaveToken = true;

        //c.TokenValidationParameters = new TokenValidationParameters
        //{
        //    ValidateIssuerSigningKey = true,
        //    ValidateIssuer = true,
        //    ValidateAudience = true,
        //    ValidIssuer = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}", // Замените на адрес вашего сервера Keycloak и realm
        //    ValidAudience = builder.Configuration["Keycloak:resource"], // Замените на имя вашего приложения
        //    ClockSkew = TimeSpan.Zero // Устанавливаем ClockSkew равным нулю для предотвращения проблем с несинхронизированным временем
        //};

        c.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                token = context.SecurityToken.ToString();
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                //var accessToken = context.Request.Query["access_token"];
                var accessToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                // если запрос направлен хабу
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(HubNames.HubUrl))
                {
                    // получаем токен из строки запроса
                    context.Token = accessToken;
                    //if (!context.Request.Headers.Any(x => x.Key == "Authorization"))
                    //{
                    //    context.HttpContext.Request.Headers.Add("Authorization", "Bearer " + token);
                    //}
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
#endregion

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

var connectionString = Config.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ChatDbContext>(opt =>
    opt.UseSqlServer(connectionString));

builder.Services.AddScoped<ChatService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Keycloak",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri($"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration"),
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {securityScheme, Array.Empty<string>()}
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

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(a => a.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapControllers();
app.MapHub<ChatHub>(HubNames.HubUrl);

app.Run();
