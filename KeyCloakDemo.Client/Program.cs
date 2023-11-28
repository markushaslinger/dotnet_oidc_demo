using KeyCloakDemo.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddScoped(sp => sp
                                 .GetRequiredService<IHttpClientFactory>()
                                 .CreateClient("API"))
       .AddHttpClient("API", client => client.BaseAddress = new Uri("http://localhost:5173"))
       .AddHttpMessageHandler<CookieHandler>();

await builder.Build().RunAsync();
