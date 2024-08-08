using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using WebGateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

//var url = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";

builder.Services.AddCors();
builder.Services.AddControllers();
//SharedModels.Config.ConfigAppConfiguration(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        c.RequireHttpsMetadata = false;
        c.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        c.Audience = "account";

        c.TokenValidationParameters.ValidateIssuer = false;
    });
builder.Services.AddAuthorization();

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

OcelotConfig.LoadConfig(builder.Configuration);
builder.Services.AddOcelot().AddConsul();
//builder.Services.AddSignalR();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
await app.UseOcelot();
app.MapControllers();
app.RunAsync().Wait();
