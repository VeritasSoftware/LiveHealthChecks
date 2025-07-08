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

The web app has a **dashboardSettings.json** file.

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

Then, open the app in browser, by url: https://localhost:7001

To run the Sample in the Solution, make sure the below projects, are started up, in the specified order.

![**Solution Startup**](/Docs/LiveHealthChecks-Solution-Startup.jpg)

### Docker Hub

You can pull an image of the monitoring web app, from Docker Hub.

Browse [**my Docker Hub**](https://hub.docker.com/r/shantanun) for all available images of the Monitoring web app.

The command is:

```
docker pull shantanun/react-livehealthchecks.ui:latest
```

This will create the image in your local docker.

Then, you can start up the image in a container, using command:

```
docker run -t -d --name react-livehealthchecks.ui -p 7001:443 shantanun/react-livehealthchecks.ui 
```

#### Certificate files

You may need to replace the **certificate files** (in the `/etc/nginx` folder of the docker container) with your own, if they have expired.

You can use the `docker cp` command to copy the new certificate files into the container.

The new certificate should have the same name as the existing one.

The certificate files needed to be replaced are:

- `livehealthchecks.ui.crt`
- `livehealthchecks.ui.key`
- `livehealthchecks.ui.pem`