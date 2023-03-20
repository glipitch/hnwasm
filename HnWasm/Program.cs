using HnWasm;
using HnWasm.Navigation;
using HnWasm.Settings;

using HnWasm.TopStories;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/") });
builder.Services.AddSingleton<LengthProvider>();
builder.Services.AddSingleton<ThemeProvider>();
builder.Services.AddSingleton<TopStoriesCache>();
builder.Services.AddSingleton<RestClient>();
builder.Services.AddSingleton<NavigationService>();
builder.Services.AddSingleton<ErrorState>();
await builder.Build().RunAsync();