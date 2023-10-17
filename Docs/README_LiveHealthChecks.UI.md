# LiveHealthChecks.UI
# Real-Time Api Health Check Monitoring Dashboard

## Dashboard

### Desktop

![**Sample Monitoring web app - LiveHealthChecks.UI**](/Docs/LiveHealthChecks-UI.jpg)

### Mobile

![**Sample Monitoring web app - LiveHealthChecks.UI**](/Docs/LiveHealthChecks-UI-Mobile.jpg)


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

You can create a similar json file for your own system.

The web app takes over from there.

If you want to receive notifications for all **ReceiveMethods** in the system, on the same connection,

set the **ServerReceiveMethod** header to * & use the SecretKey set in the Server.

The web app renders the Widgets for each Api specified in the file.

Also, each widget is listening to the **ReceiveMethod** of that Api.

When a **Health Report** for an Api is received from the Server,

that Api's widget only, is refreshed automatically.

The web app also uses the **browser's local storage database** for storing the Health Reports,

received from the Server.

Also, you can search this data in the Search popup (click on Search button on App bar).

### Desktop

![**Search**](/Docs/LiveHealthChecks-UI-Search.jpg)

### Mobile

![**Search**](/Docs/LiveHealthChecks-UI-Search-Mobile.jpg)

You can click on a row in the table and a **View Health Check** popup comes up.

This has all the details of the Health Check, including the Health Report JSON.

### Desktop

![**View Health Check**](/Docs/LiveHealthChecks-UI-ViewHealthCheck.jpg)

### Mobile

![**View Health Check**](/Docs/LiveHealthChecks-UI-ViewHealthCheck-Mobile.jpg)

## Docker containerization

You can clone the solution and containerize the Monitoring web app.

It is all configured for containerization using **Alpine Linux container** & **nginx**.

Also, **SSL** supported **https**.

From the solution directory, run the build:

```
docker compose build --pull --no-cache
```

then, deploy container:

```
docker compose up
```

Then, open the app in browser, by url: https://localhost:8001