
namespace SimulatedTemperatureSensor
{
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Threading;
    using System.Threading.Tasks;

    public class ModuleClientWrapper : IModuleClient
    {
        readonly ModuleClient moduleClient;
        readonly ILogger logger;

        public ModuleClientWrapper(IConfiguration configuration,
            ILogger<ModuleClientWrapper> logger)
        {
            this.logger = logger;

            var transportType = 
                configuration.GetValue("ClientTransportType", TransportType.Mqtt_Tcp_Only);

            ITransportSettings[] settings = { new MqttTransportSettings(transportType) };

            moduleClient = ModuleClient.CreateFromEnvironmentAsync(settings).Result;
        }

        public async Task SendEventAsync(string outputName, Message message)
        {
            await moduleClient.SendEventAsync(outputName, message);
        }

        public async Task SetInputMessageHandlerAsync(string inputName,
            MessageHandler messageHandler,
            object userContext)
        {
            await moduleClient.SetInputMessageHandlerAsync(inputName,
                messageHandler,
                userContext);
        }

        public async Task SetMethodHandlerAsync(string methodName,
            MethodCallback methodHandler,
            object userContext)
        {
            await moduleClient.SetMethodHandlerAsync(methodName,
                methodHandler,
                userContext);
        }
        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            await moduleClient.OpenAsync(cancellationToken);
        }
        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await moduleClient.CloseAsync(cancellationToken);
        }
        public async Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
        {
            return await moduleClient.GetTwinAsync(cancellationToken);
        }
        public async Task<Twin> GetTwinAsync()
        {
            return await moduleClient.GetTwinAsync();
        }
    }
}
