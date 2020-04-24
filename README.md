# SimulatedTemperatureSensor

This is a loosely coupled version of the IoT Edge SimulatedTemperatureSensor example based on dependency injection. This solution includes an IoT Edge emulator that greatly simplifies the IoT Edge development experience.

To run this application, you need .NET Core SDK (>= 3.1), Docker and an IoT Hub.

> This repo has been presented and analysed in this blog post : http://havedatawilltrain.com/lysis/
## Running the application

1. Clone this repo:

``` bash
   git clone https://github.com/paloukari/SimulatedTemperatureSensor
```

2. Start the IoT Edge Simulator:

``` bash
   cd DevelopmentTools
   dotnet run ../manifest.json dev_device "YOUR_IOT_HUB_OWNER_CONNECTION_STRING"
```

3. Run the application

``` bash
cd ..
cd SimulatedTemperatureSensor
dotnet run
``` 
> The `DOTNET_ENVIRONMENT` environment variable is set [here](https://github.com/paloukari/SimulatedTemperatureSensor/blob/master/SimulatedTemperatureSensor/Properties/launchSettings.json).
