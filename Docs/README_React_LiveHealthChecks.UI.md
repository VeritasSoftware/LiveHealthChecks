# LiveHealthChecks.UI
# Real-Time Api Health Check Monitoring Dashboard

### Dashboard tech stack:

* React
* Javascript SignalR Client
* React ChartJS
* React Bootstrap

## Dashboard

The Monitoring dashboard opens a **SignalR Client connection** to the Server.

Then, each Api widget listens to that Api's ReceiveMethod for push notifications with the Health Report.

The widgets are rendered based this data.

### Desktop

![**Sample React Monitoring web app - LiveHealthChecks.UI**](/Docs/React-LiveHealthChecks-UI.jpg)

### Mobile

![**Sample React Monitoring web app - LiveHealthChecks.UI**](/Docs/React-LiveHealthChecks-UI-Mobile.jpg)


## Dashboard Settings

The web app has a **dashboardSettings.json** file in the **wwwroot** folder.

```JSON
{
  "ServerUrl": "https://localhost:5001/livehealthcheckshub",
  "ServerReceiveMethod": "*",
  "ServerSecretKey": "f22f3fd2-687d-48a1-aa2f-f2c9181364eb",
  "ServerClientId": "LiveHealthChecks.UI",
  "Apis": [
    {
      "ApiName": "Sample Api",
      "ReceiveMethod": "SampleApiHealth"
    },
    {
      "ApiName": "Sample Api 2",
      "ReceiveMethod": "SampleApi2Health"
    }
  ]
}
```

## Information

**Note:** You should create a similar json settings file for your own system.

The web app takes over from there.

If you want to receive notifications for all **ReceiveMethods** in the system, on the same connection,

set the **ServerReceiveMethod** to * & use the SecretKey set in the Server.

The web app renders the Widgets for each Api specified in the file.

Also, each widget is listening to the SignalR **ReceiveMethod** of that Api.

When a **Health Report** for an Api is received from the Server,

that Api's widget only, is refreshed automatically.

The web app also uses the **browser's local storage database** for storing the Health Reports,

received from the Server.