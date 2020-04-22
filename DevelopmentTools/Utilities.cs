namespace DevelopmentTools
{
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public static class Utilities
    {
        public static async Task StartEmulatorAsync(string deviceConnectionString)
        {
            string deviceContainerImage = "toolboc/azure-iot-edge-device-container";
            int[] exposedPorts = new[] { 15580, 15581, 443, 8883, 5671 };
            string imageName = "dev_iot_edge";

            var localDockerSocket = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                @"npipe://./pipe/docker_engine" :
                @"unix:/var/run/docker.sock";

            var dockerClient = new DockerClientConfiguration(new Uri(localDockerSocket))
                .CreateClient();

            Console.WriteLine($"Downloading the latest image:{deviceContainerImage}..");

            await dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = deviceContainerImage,
                    Tag = "latest"
                },
                new AuthConfig(),
                new Progress<JSONMessage>((e) =>
                {
                    if (!string.IsNullOrEmpty(e.Status))
                        Console.Write($"{e.Status}");
                    if (!string.IsNullOrEmpty(e.Status))
                        Console.Write($"{e.ProgressMessage}");
                    if (!string.IsNullOrEmpty(e.Status))
                        Console.Write($"{e.ErrorMessage}");
                    Console.WriteLine("");
                }));

            var containers = await dockerClient.Containers
                .ListContainersAsync(new ContainersListParameters() { All = true });

            foreach (var _container in containers)
            {
                if (_container.Names.Contains(imageName) || 
                    _container.Names.Contains($@"/{imageName}"))
                {
                    if (_container.State == "running")
                    {
                        Console.WriteLine($"Stopping container {_container.ID}..");
                        await dockerClient.Containers.StopContainerAsync(_container.ID,
                            new ContainerStopParameters());
                    }
                    Console.WriteLine($"Removing container {_container.ID}..");
                    await dockerClient.Containers.RemoveContainerAsync(_container.ID,
                        new ContainerRemoveParameters());
                    break;
                }
            }

            Console.WriteLine($"Creating {imageName} container..");
            var container = await dockerClient.Containers
                .CreateContainerAsync(new CreateContainerParameters
            {
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,
                Tty = true,
                Env = new List<string>() { $"connectionString={deviceConnectionString}" },

                Name = imageName,
                Image = deviceContainerImage,
                ExposedPorts = exposedPorts
                    .ToDictionary(x => x.ToString(), x => default(EmptyStruct)),
                HostConfig = new HostConfig
                {
                    Privileged = true,
                    PortBindings = exposedPorts.ToDictionary(
                        x => x.ToString(),
                        x => (IList<PortBinding>)new List<PortBinding> {
                            new PortBinding {
                                HostPort = x.ToString()
                            }
                        }),
                    PublishAllPorts = true
                }
            });
            Console.WriteLine($"Starting {imageName} container..");
            var startResult = await dockerClient.Containers.StartContainerAsync(
                container.ID, null);

            if (!startResult)
                throw new Exception($"Could not start the {imageName} container!");

            Console.WriteLine("Done.");
        }
        public static string CreateDevelopmentManifest(string template)
        {
            var templateContent = JsonConvert
                .DeserializeObject<ConfigurationContent>(template);
            
            var agentDesired = JObject.FromObject(
                   templateContent.ModulesContent["$edgeAgent"]["properties.desired"]);

            if (!agentDesired.TryGetValue("modules", out var modulesSection))
                throw new Exception("Cannot read modules config from $edgeAgent");

            foreach (var module in modulesSection as JObject)
            {
                var moduleSettings = JObject.FromObject(modulesSection[module.Key]["settings"]);
                moduleSettings["image"] = "wardsco/sleep:latest";
                modulesSection[module.Key]["settings"] = moduleSettings;
            }
            agentDesired["modules"] = modulesSection;
            templateContent.ModulesContent["$edgeAgent"]["properties.desired"]
                = agentDesired;


            return JsonConvert.SerializeObject(templateContent, Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static async Task<string> ProvisionDeviceAsync(
            string deviceId,
            string manifest,
            string ioTHubConnectionString)
        {
            var registryManager = RegistryManager
                .CreateFromConnectionString(ioTHubConnectionString);

            var hostName = ioTHubConnectionString.Split(";")
                .SingleOrDefault(e => e.Contains("HostName="));

            if (string.IsNullOrEmpty(hostName))
                throw new ArgumentException(
                    $"Invalid ioTHubConnectionString: {ioTHubConnectionString}");
            hostName = hostName.Replace("HostName=", "");

            var device = await registryManager.GetDeviceAsync(deviceId) ??
                await registryManager.AddDeviceAsync(
                    new Device(deviceId)
                    {
                        Capabilities = new DeviceCapabilities() { IotEdge = true }
                    });

            var sasKey = device.Authentication.SymmetricKey.PrimaryKey;

            var manifestContent = JsonConvert
                .DeserializeObject<ConfigurationContent>(manifest);

            // remove all old modules
            foreach (var oldModule in await registryManager.GetModulesOnDeviceAsync(deviceId))
                if (!oldModule.Id.StartsWith("$"))
                    await registryManager.RemoveModuleAsync(oldModule);
            // create new modules
            foreach (var module in manifestContent.ModulesContent.Keys)
                if (!module.StartsWith("$"))
                    await registryManager.AddModuleAsync(new Module(deviceId, module));

            await registryManager
                .ApplyConfigurationContentOnDeviceAsync(deviceId, manifestContent);

            return $"HostName={hostName};DeviceId={deviceId};SharedAccessKey={sasKey}";
        }
    }
}
