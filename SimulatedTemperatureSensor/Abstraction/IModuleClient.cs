namespace SimulatedTemperatureSensor
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.Devices.Client;

    public interface IModuleClient
    {
        Task OpenAsync(CancellationToken cancellationToken);
        Task CloseAsync(CancellationToken cancellationToken);
        Task SendEventAsync(string outputName,
            Message message);
        Task SetInputMessageHandlerAsync(string inputName,
            MessageHandler messageHandler,
            object userContext);
        Task SetMethodHandlerAsync(string methodName,
            MethodCallback methodHandler,
            object userContext);
        Task<Twin> GetTwinAsync(CancellationToken cancellationToken);
        Task<Twin> GetTwinAsync();
    }
}