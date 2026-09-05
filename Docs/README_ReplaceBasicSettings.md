# Replace Basic Settings

The Client package provides a set of basic settings that can be used to configure the behavior of the client. 

These settings can be replaced dynamically at runtime, with the Api/app still running. 

This allows for greater flexibility and customization of the client behavior without the need to restart the application.

The package provides 2 endpoints for replacing the basic settings:

GET /livehealthchecks/settings

This endpoint allows for retrieving the current basic settings. 

The response will be a JSON object containing the current values of the basic settings.

POST /livehealthchecks/settings/replace

This endpoint allows for replacing the basic settings with new values. 

The new values can be provided in the request body as a JSON object.

To include the endpoints in your Api/App client, you can use the following code:

```csharp
app.UseLiveHealthChecksClient();
```

## Get basic settings

![Get basic settings](/Docs/GetBasicSettings.png)

## Replace basic settings

![Replace basic settings](/Docs/ReplaceBasicSettings.png)

### Note

When you replace the Server Hub Url to point to a new Server,

the SignalR connection is re-started with the new Url.