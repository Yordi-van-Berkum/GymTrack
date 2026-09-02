using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Blazor;
using Microsoft.AspNetCore.Components.Authorization;
using Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:4000") });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ExercisesService>();
builder.Services.AddScoped<WorkoutsService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<SafeApiHelper>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    provider => provider.GetRequiredService<CustomAuthStateProvider>()
);

await builder.Build().RunAsync();
