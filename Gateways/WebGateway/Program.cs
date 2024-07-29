using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Services.AddControllers();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //// Устанавливаем схему по умолчанию как Cookie
        //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //// Указываем JWT-схему для входа и выхода
        //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    //{
    //    options.LoginPath = "/login";
    //    options.LogoutPath = "/logout";
    //    options.AccessDeniedPath = "/accessdenied";

    //    options.Cookie.Name = "__Host-bff";
    //    options.Cookie.SameSite = SameSiteMode.Strict;
    //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //    options.Cookie.SameSite = SameSiteMode.Strict;
    //    options.Cookie.HttpOnly = true;
    //})
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        c.SaveToken = true;
        c.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        c.RequireHttpsMetadata = false;
        c.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        c.Audience = "account";// $"{builder.Configuration["Keycloak:resource"]}";
        //c.TokenValidationParameters = new TokenValidationParameters
        //{
        //    ValidateAudience = false,
        //    ValidateIssuer = false,
        //};
    });
//.AddOpenIdConnect("Keycloak", options =>
//{
//    options.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
//    options.ClientId = builder.Configuration["Keycloak:resource"];
//    options.ResponseType = OpenIdConnectResponseType.Code;
//    options.SaveTokens = true;
//    options.Scope.Clear();
//    options.Scope.Add("openid");
//    options.Scope.Add("profile");
//    options.Scope.Add("offline_access");
//    options.Scope.Add("roles");// Обязательно добавьте "roles"
//    options.GetClaimsFromUserInfoEndpoint = true;
//    options.RequireHttpsMetadata = false;
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        NameClaimType = "preferred_username",
//        RoleClaimType = "roles"
//    };
//    options.UsePkce = true;
//    options.MapInboundClaims = true;
//    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//});
builder.Services.AddAuthorization();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration)
.AddConsul();
//.AddDelegatingHandler<AuthDelegatingHandler>();



var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

await app.UseOcelot();
app.MapControllers();
await app.RunAsync();
