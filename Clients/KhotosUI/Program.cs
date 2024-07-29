using KhotosUI;
using KhotosUI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<JwtAuthorizationMessageHandler>();
builder.Services.AddHttpClient("API",
    client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("WebGatewayUrl")))
  .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services
    .AddCascadingAuthenticationState()
    .AddOidcAuthentication(options =>
    {
        options.ProviderOptions.Authority = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"];
        options.ProviderOptions.ClientId = builder.Configuration["Keycloak:resource"];
        options.ProviderOptions.MetadataUrl = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"] + "/.well-known/openid-configuration";
        options.ProviderOptions.ResponseType = "id_token token";//"code";//
        options.ProviderOptions.DefaultScopes.Add("offline_access");
        options.UserOptions.RoleClaim = "roles";
        options.UserOptions.ScopeClaim = "scope";

    });
builder.Services.AddApiAuthorization().AddAccountClaimsPrincipalFactory<CustomUserFactory>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddBlazorBootstrap(); // Add this line
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DataService>();
//builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ChatClient>();

await builder.Build().RunAsync();