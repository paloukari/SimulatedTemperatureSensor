namespace SimulatedTemperatureSensor
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    class Program
    {
        static async Task Main(string[] args)
        {
            using (var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((hostContext, services) =>
                 {
                     if (hostContext.HostingEnvironment.EnvironmentName == "Emulated")
                         Utilities.InjectIoTEdgeVariables("SimulatedTemperatureSensor");

                     if (hostContext.HostingEnvironment.EnvironmentName == "Standalone")
                         services.AddSingleton<IModuleClient, MockModuleClientWrapper>();
                     else
                         services.AddSingleton<IModuleClient, ModuleClientWrapper>();


                     services.AddHostedService<TemperatureSensorModule>();
                 })
                 .UseSerilog((hostingContext, log) =>
                 {
                     log.ReadFrom.Configuration(hostingContext.Configuration);
                 })
                 .UseConsoleLifetime()
                 .Build())
            {
                await host.RunAsync();
            }
        }
    }
}
