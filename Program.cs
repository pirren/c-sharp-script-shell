using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using script_shell;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddHostedService<CoreService>();
    })
    .Build();

await host.RunAsync();