using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using WebUI_Wasm.Components;
using WebUI_Wasm.Components.Account;
using WebUI_Wasm.Data;
using WebUI_Wasm.Hubs;
using WebUI_Wasm.Services;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
          second: new[] { "application/octet-stream" });
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddSingleton<ChatManager>();
builder.Services.AddBlazorBootstrap(); // Add this line
builder.Services.AddScoped<OrderService>();
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = ChatService.MaxFileSize;  //Изменил размер принимаемых данных
});
builder.Services.AddScoped<ChatService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(c => c.AddPolicy("cors", opt =>
{
    opt.AllowAnyOrigin();
    //opt.AllowAnyHeader();
    //opt.AllowCredentials();
    //opt.AllowAnyMethod();
    //opt.WithOrigins(builder.Configuration.GetSection("Cors:Urls").Get<string[]>()!);
}));


builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = ConnectDb.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseResponseCompression();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


//app.MapBlazorHub();
//app.MapFallbackToPage("/_Host");
app.MapHub<ChatHub>(ChatHub.HubUrl);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
await DbInitializer.SeedAsync(app, builder.Configuration);

app.Run();
