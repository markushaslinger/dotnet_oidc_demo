using KeyCloakDemo;
using KeyCloakDemo.Client;
using KeyCloakDemo.Client.Pages;
using KeyCloakDemo.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents()
       .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
       {
           options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
           options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
       })
       .AddCookie(options => { options.LoginPath = "/Login"; })
       .AddOpenIdConnect(options =>
       {
           options.Authority = "http://localhost:8080/realms/DemoApp";
           options.ClientId = "DemoAppID";
           options.ClientSecret = "client secret goes here";
           options.ResponseType = "code";
           options.CallbackPath = "/signin-oidc";
           options.SignedOutCallbackPath = "/signout-callback-oidc";
           options.SaveTokens = true;
           options.Scope.Add("openid");
           options.Scope.Add("profile");
           options.TokenValidationParameters = new TokenValidationParameters
           {
               NameClaimType = "preferred_username",
               RoleClaimType = "roles"
           };
           
           // remove for production!
           options.RequireHttpsMetadata = false;
       });

builder.Services.AddHttpContextAccessor();

builder.Services
       .AddTransient<CookieHandler>()
       .AddScoped(sp => sp
                        .GetRequiredService<IHttpClientFactory>()
                        .CreateClient("API"))
       .AddHttpClient("API", client => client.BaseAddress = new Uri("http://localhost:5173"))
       .AddHttpMessageHandler<CookieHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/Login", async context =>
{
    var authProperties = new AuthenticationProperties
    {
        RedirectUri = "/"
    };
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authProperties);
});

app.MapGet("/signin-oidc", context => context.Response.WriteAsync("Logged in!"));

app.MapGet("/Hello", context => Task.FromResult(Results.Ok("Auth ok!")))
   .RequireAuthorization();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddInteractiveWebAssemblyRenderMode()
   .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
