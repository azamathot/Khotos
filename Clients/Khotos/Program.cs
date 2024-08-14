using Khotos.Components;
using Khotos.Helper;
using Khotos.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

//SharedModels.Config.ConfigAppConfiguration(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCors();

builder.Services.AddScoped<JwtAuthorizationMessageHandler>();
builder.Services.AddHttpClient("API",
    client => client.BaseAddress = new Uri(builder.Configuration["WebGatewayUrl"]))
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>()
.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
});

//builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
//    .CreateClient("API"));

#region Keycloak Работающая версия 1 с использованием AddKeycloakAuthentication...
//builder.Services
//    .AddKeycloakWebAppAuthentication(builder.Configuration);

//builder.Services.AddKeycloakAuthorization(options =>
//{
//    options.AuthServerUrl = builder.Configuration["Keycloak:auth-server-url"]!;
//    options.Realm = builder.Configuration["Keycloak:realm"]!;
//    options.Resource = builder.Configuration["Keycloak:resource"]!;
//    options.SslRequired = builder.Configuration["Keycloak:ssl-required"]!;
//    options.VerifyTokenAudience = false;
//    options.EnableRolesMapping = RolesClaimTransformationSource.ResourceAccess;
//});
#endregion

#region Keycloak Работающая версия 2 с использованием AddOpenIdConnect...
string? Id_Token = null;
builder.Services.AddCascadingAuthenticationState().AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        //options.Cookie.HttpOnly = true;
        //options.Cookie.IsEssential = true;
        //options.Cookie.SameSite = SameSiteMode.Strict;
        //options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    .AddOpenIdConnect(options =>
    {
        //Отключить проверку SSL сертификата
        HttpClientHandler handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        options.BackchannelHttpHandler = handler;
        //это нужно в Development в docker, чтобы шлюз не конфликтовал

        options.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}";
        options.Authority = $"{builder.Configuration["WebGatewayUrl"]}realms/{builder.Configuration["Keycloak:realm"]}";
        options.ClientId = builder.Configuration["Keycloak:resource"];
        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = true;
        options.RequireHttpsMetadata = false;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("roles");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role",
            //ValidateIssuerSigningKey = true,
            //ValidateIssuer = true,
            //ValidateAudience = true,
            //ValidIssuer = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}", // Замените на адрес вашего сервера Keycloak и realm
            //ValidAudience = builder.Configuration["Keycloak:resource"], // Замените на имя вашего приложения
            ClockSkew = TimeSpan.Zero // Устанавливаем ClockSkew равным нулю для предотвращения проблем с несинхронизированным временем
        };


        options.Events = new OpenIdConnectEvents
        {
            OnUserInformationReceived = context =>
            {
                MapRoles.MapKeyCloakRolesToRoleClaims(context);
                return Task.CompletedTask;
            }
            ,
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                Id_Token ??= context.ProtocolMessage.IdTokenHint;
                context.ProtocolMessage.IdTokenHint ??= context.HttpContext.User.FindFirstValue("id_token") ?? Id_Token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
#endregion


builder.Services.AddBlazorBootstrap(); // Add this line
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<ChatClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
// Add routes for callback handling

app.Run();
