import { List } from "ts-generic-collections-linq";
import { HealthCheck } from "../Models/Models";

export class MyHealthChecksRepository {
    getDbHealthChecks = (receiveMethod: string) : List<HealthCheck> => {
        let items = localStorage.getItem(receiveMethod);
        let db = new List<HealthCheck>();
        if (items != null && items != "") {
            db = new List<HealthCheck>(JSON.parse(items));          
        }
        
        return db;
    }
    
    getDbResult = (receiveMethod: string) : number[] => {
        let db = this.getDbHealthChecks(receiveMethod);
        
        var healthy = db.count(x => x.Status == 2);
        var unHealthy = db.count(x => x.Status == 1);
        
        return [healthy, unHealthy];
    }
    
    saveHealthChecks = (receiveMethod: string, healthCheck: HealthCheck) => {
        var db = this.getDbHealthChecks(receiveMethod);
        db.add(healthCheck);
        localStorage.removeItem(receiveMethod);
        localStorage.setItem(receiveMethod, JSON.stringify(db.toArray()));
    }
}