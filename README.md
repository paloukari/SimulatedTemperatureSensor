# SimulatedTemperatureSensor
This is a loosely couple version of the IoT Edge SimulatedTemperatureSensor example based on dependency injection. This solution includes an IoT Edge emulator that greatly simplifies the IoT Edge development experience.

To run run this application, you need .NET Core SDK (>= 3.1), Docker and an IoT Hub.

## Running the application

1. Clone this repo:

``` bash
   git clone https://github.com/paloukari/SimulatedTemperatureSensor
```

1. Start the IoT Edge Simulator:

``` bash
   cd DevelopmentTools
   dotnet run ../manifest.json dev_device "YOUR_IOT_HUB_OWNER_CONNECTION_STRING"
```

3. Set the `DOTNET_ENVIRONMENT` environment variable to `Emulated`

4. Run the application

``` bash
cd ..
cd SimulatedTemperatureSensor
dotnet run
``` 