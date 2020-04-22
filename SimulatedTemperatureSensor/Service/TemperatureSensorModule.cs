namespace SimulatedTemperatureSensor
{
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class TemperatureSensorModule : IHostedService
    {
        const string SAMPLING_PERIOD_CONFIG_NAME = "TelemetryPeriodSeconds";
        const int SAMPLING_PERIOD_DEFAULT = 1;
        
        readonly IModuleClient moduleClient;
        readonly ILogger logger;
        readonly IHostApplicationLifetime application;
        
        readonly Random random = new Random();
        readonly TaskTimer telemetryPump;

        public TemperatureSensorModule(IModuleClient moduleClient,
            IConfiguration config,
            IHostApplicationLifetime application, 
            ILogger<TemperatureSensorModule> logger)
        {
            this.moduleClient = moduleClient;
            this.logger = logger;
            this.application = application;

            var period = TimeSpan.FromSeconds(
                config.GetValue(SAMPLING_PERIOD_CONFIG_NAME, SAMPLING_PERIOD_DEFAULT));

            telemetryPump = new TaskTimer(OnTimer, period, logger, application.StopApplication);
            
            application.ApplicationStopping.Register(() =>
            {
                logger.LogWarning("Stop-draining application for 3 seconds...");
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            });
        }

        private async void OnTimer()
        {
            await moduleClient.SendEventAsync("telemetry",
                new Message(Encoding.UTF8.GetBytes($"Current temperature: {random.Next(0, 100)}")));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await moduleClient.OpenAsync(cancellationToken);
            telemetryPump.Start(application.ApplicationStopping);

            logger.LogInformation("Started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await moduleClient.CloseAsync(cancellationToken);
            logger.LogInformation("Stopped.");
        }
    }
}