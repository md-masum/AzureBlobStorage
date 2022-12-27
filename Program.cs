using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureBlobStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = CreateDefaultBuilder().Build();

// Invoke Worker
using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;
var workerInstance = provider.GetRequiredService<Worker>();
workerInstance.DoWork();

host.Run();

static IHostBuilder CreateDefaultBuilder()
{
    return Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(app =>
        {
            app.AddJsonFile("appsettings.json");
        })
        .ConfigureServices(services =>
        {
            services.AddSingleton<Worker>();
        });
}