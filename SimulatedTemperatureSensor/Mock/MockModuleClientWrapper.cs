namespace SimulatedTemperatureSensor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class MockModuleClientWrapper : IModuleClient
    {
        readonly ILogger logger;
        readonly IHostApplicationLifetime application;

        readonly TaskTimer taskTimer;
        readonly Dictionary<string, List<Message>> messageQueues;
        readonly Dictionary<string, ValueTuple<MessageHandler, object>> inputMessageHandlers;
        readonly Dictionary<string, MethodCallback> methodMessageHandlers;

        public MockModuleClientWrapper(IHostApplicationLifetime application,
            ILogger<MockModuleClientWrapper> logger)
        {
            this.logger = logger;
            this.application = application;

            messageQueues = new Dictionary<string, List<Message>>();
            inputMessageHandlers = new Dictionary<string, (MessageHandler, object)>();
            methodMessageHandlers = new Dictionary<string, MethodCallback>();

            taskTimer = new TaskTimer(OnTimer, TimeSpan.FromSeconds(1), logger);
        }

        private void OnTimer()
        {
            lock (messageQueues)
                foreach (var queue in messageQueues)
                {
                    if (inputMessageHandlers.ContainsKey(queue.Key))
                        foreach (var message in queue.Value)
                            inputMessageHandlers[queue.Key].Item1(message, inputMessageHandlers[queue.Key].Item2);
                    messageQueues[queue.Key].Clear();
                }
            // TODO: Process method messsages too
        }

        public Task SendEventAsync(string outputName, Message message)
        {
            lock (messageQueues)
            {
                if (!messageQueues.ContainsKey(outputName))
                    messageQueues[outputName] = new List<Message>();
                messageQueues[outputName].Add(message);
            }
            logger.LogInformation($"Message Sent to {outputName}");
            return Task.CompletedTask;
        }

        public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
        {
            inputMessageHandlers[inputName] = (messageHandler, userContext);
            logger.LogInformation($"Message Handler Set for {inputName}");
            return Task.CompletedTask;
        }

        public Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext)
        {
            methodMessageHandlers[methodName] = methodHandler;
            logger.LogInformation($"Method Handler Set for {methodName}");
            return Task.CompletedTask;
        }

        public Task OpenAsync(CancellationToken token)
        {
            logger.LogInformation("Opened ModuleClient");
            taskTimer.Start(application.ApplicationStopping);
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken token)
        {
            logger.LogInformation("Closed ModuleClient");
            return Task.CompletedTask;
        }

        public Task<Twin> GetTwinAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("GetTwinAsync");
            return Task.FromResult<Twin>(null);
        }

        public Task<Twin> GetTwinAsync()
        {
            logger.LogInformation("GetTwinAsync");
            return Task.FromResult<Twin>(null);
        }
    }
}