using CatFactLogger;
using CatFactLogger.Options;
using CatFactLogger.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

// ContentRootPath is pinned to the exe's own folder so appsettings.json and
// profile.json are found next to it, regardless of the working directory
// the app was launched from (this differs between "dotnet run", a debugger
// in Visual Studio, and double-clicking the published exe).
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.Configure<CatFactOptions>(builder.Configuration.GetSection(CatFactOptions.SectionName));
builder.Services.Configure<ProfileOptions>(builder.Configuration.GetSection(ProfileOptions.SectionName));

builder.Services.AddHttpClient<ICatFactClient, CatFactClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<CatFactOptions>>().Value;
    client.BaseAddress = new Uri(options.ApiUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IFactStorage, FileFactStorage>();
builder.Services.AddSingleton<IProfileProvider, JsonProfileProvider>();
builder.Services.AddSingleton<IProfileRenderer, SpectreProfileRenderer>();
builder.Services.AddSingleton<ConsoleMenu>();

using var host = builder.Build();

var menu = host.Services.GetRequiredService<ConsoleMenu>();
await menu.RunAsync();
