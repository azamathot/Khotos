using Microsoft.AspNetCore.Authentication.JwtBearer;
using Robokassa.NET;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region Keycloak Работающая версия 2 с использованием AddOpenIdConnect...
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        c.RequireHttpsMetadata = false;
        c.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        c.Audience = "account";// $"{builder.Configuration["Keycloak:resource"]}"; ;
    });
builder.Services.AddAuthorization();
#endregion

//Заполните appsettings.Development.Json
builder.Services.AddCors(c => c.AddPolicy("cors", opt =>
{
    opt.AllowAnyOrigin();
    //opt.AllowAnyHeader();
    //opt.AllowCredentials();
    //opt.AllowAnyMethod();
    //opt.WithOrigins(builder.Configuration.GetSection("Cors:Urls").Get<string[]>()!);
}));

builder.Services.AddRobokassa(
                builder.Configuration["RobokassaOptions:ShopName"],
                builder.Configuration["RobokassaOptionsTest:Password1"],
                builder.Configuration["RobokassaOptionsTest:Password2"],
                !builder.Environment.IsProduction());

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
