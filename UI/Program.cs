using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;
using UI;
using UI.DataLoading;
using UI.Models;
using UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<RecipeDataLoader>();
builder.Services.AddScoped<MealPlanState>();
builder.Services.AddSingleton(_ =>
{
    JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
    options.Converters.Add(new MarkupStringJsonConverter());
    return options;
});

await builder.Build().RunAsync();