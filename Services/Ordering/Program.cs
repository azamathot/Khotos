using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Ordering.Data;
using SharedModels;

var builder = WebApplication.CreateBuilder(args);
Config.ConfigAppConfiguration(builder.Configuration);

// Add services to the container.
#region Keycloak Работающая версия 2 с использованием AddOpenIdConnect...
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        c.RequireHttpsMetadata = false;
        c.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        c.Audience = "account";// $"{builder.Configuration["Keycloak:resource"]}"; ;

        c.TokenValidationParameters.NameClaimType = "name";
        c.TokenValidationParameters.RoleClaimType = "role";
    });
builder.Services.AddAuthorization();
#endregion


builder.Services.AddControllers();

var connectionString = Config.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<OrdersDbContext>(opt =>
    opt.UseSqlServer(connectionString));
builder.Services.AddCors(c => c.AddPolicy("cors", opt =>
{
    opt.AllowAnyOrigin();
    //opt.AllowAnyHeader();
    //opt.AllowCredentials();
    //opt.AllowAnyMethod();
    //opt.WithOrigins(builder.Configuration.GetSection("Cors:Urls").Get<string[]>()!);
}));
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

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
