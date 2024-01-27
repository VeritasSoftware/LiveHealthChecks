import { MyServerService } from "../Services/MyServerService";
import { Dictionary } from 'ts-generic-collections-linq'
import { MyHealthChecksRepository } from "../Repository/MyHealthChecksRepository";

export class MyApiHealthCheckModel {
    ApiName: string = "";
    ReceiveMethod: string = "";
}

export class ApiWidgetProperties extends MyApiHealthCheckModel {
    MyServerService: MyServerService | undefined;
    MyHealthChecksRepository: MyHealthChecksRepository | undefined;
}

export class DashboardSettings {
    ServerUrl: string = "https://localhost:5001/livehealthcheckshub";
    ServerReceiveMethod: string = "*";
    ServerSecretKey: string = "f22f3fd2-687d-48a1-aa2f-f2c9181364eb";
    ServerClientId: string = "Monitoring App";
    Apis: Array<MyApiHealthCheckModel> = new Array<MyApiHealthCheckModel>();
}

export class HealthCheck {
    Api?: string;
    ReceiveMethod?: string;
    ReceiveTimeStamp?: string;
    Status?: number;
}

export class HealthReport {
    Status?: number;
    TotalDuration?: string;
    Entries?: Dictionary<string, number>;
}