import { DashboardSettings } from "../Models/Models";

const signalr = require('@microsoft/signalr') 

export class MyServerService {
    connection: any;

    connect = (dashboardSettings: DashboardSettings) => {
        this.connection = new signalr.HubConnectionBuilder()
          .withUrl(dashboardSettings.ServerUrl)
          .build();
      
        if (this.connection.state === signalr.HubConnectionState.Disconnected)
        {
          this.connection.start()
          .then(() => this.connection.invoke("AuthenticateAsync",
          {
              ReceiveMethod : dashboardSettings.ServerReceiveMethod,
              SecretKey : dashboardSettings.ServerSecretKey,
              ClientId : dashboardSettings.ServerClientId
          }));
        }
    };
    
    subscribe = (receiveMethod: string, onHealthReportReceivedHandler: (healthReport: any) => void) => {
        this.connection.off(receiveMethod);
        this.connection.on(receiveMethod, onHealthReportReceivedHandler);
    }

    disconnect = (receiveMethod: string) => {
      if (this.connection != null) {
        this.connection.invoke("DisconnectAsync");
        this.connection.off(receiveMethod);
        this.connection = null;
      }      
    }
}