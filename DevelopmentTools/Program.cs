namespace DevelopmentTools
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: dotnet run {MANIFEST_FILE} {DEVICE_ID} " +
                "{IOT_HUB_OWNER_CONNECTION_STRING}");
                return;
            }
            var developmentManifest = Utilities.CreateDevelopmentManifest(
                File.ReadAllText(args[0]));

            var deviceConnectionString =
                await Utilities.ProvisionDeviceAsync(args[1],
                developmentManifest,
                args[2]);

            await Utilities.StartEmulatorAsync(deviceConnectionString);
        }
    }
}
