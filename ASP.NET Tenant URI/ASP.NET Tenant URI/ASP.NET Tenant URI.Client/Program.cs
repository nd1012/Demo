using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new($"http://localhost:5113/{"tenant"}") });// Base URI for http API request including tenant

await builder.Build().RunAsync();
