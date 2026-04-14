// Create Connection
let connectionUserCount = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5279/hubs/greenhouse")
    .withAutomaticReconnect()
    .build();

// receive notifications from hub
connectionUserCount.on("ReceiveSensorData", function (value) {
    const SoilMoisture = document.getElementById("SoilMoisture");
    const LightDependentResistor = document.getElementById("LightDependentResistor");
    const Temperature = document.getElementById("Temperature");
    const Humidity = document.getElementById("Humidity");

    SoilMoisture.innerText = value.soilMoisture;
    LightDependentResistor.innerText = value.lightDependentResistor;
    Temperature.innerText = value.temperature;
    Humidity.innerText = value.humidity;
});

// invoke hub method from client


// start connection and then call hub
function fulfilled() {
    console.log("Connection to GreenhouseHub successful");
}

function rejected(error) {
    console.error(error);
}

connectionUserCount
    .start()
    .then(fulfilled, rejected);