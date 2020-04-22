namespace SimulatedTemperatureSensor
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    internal static class Utilities
    {
        internal static void InjectIoTEdgeVariables(string containerName)
        {
            var dockerCommand =
                $"docker exec dev_iot_edge bash -c \"docker exec {containerName} env | grep IOTEDGE_\"";
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo(dockerCommand)
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {dockerCommand}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
            };
            p.Start();
            p.WaitForExit();

            var output = p.StandardOutput.ReadToEnd();
            var lines = output.Split(new[] { "\n" }, StringSplitOptions.None)
                .Where(e => e != null && e.Contains("="));

            var variables = lines.ToDictionary(e => e.Split("=")[0], e => e.Split("=")[1]);

            // Overwrite these settigns
            variables["IOTEDGE_WORKLOADURI"] = "http://127.0.0.1:15581/";
            variables["IOTEDGE_GATEWAYHOSTNAME"] = Dns.GetHostName();
            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
                Console.WriteLine($"Injected {variable.Key}={variable.Value}");
            }
        }
    }
}