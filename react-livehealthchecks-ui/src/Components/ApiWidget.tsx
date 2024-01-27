import React, { useState } from 'react';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import { Pie } from 'react-chartjs-2';
import { ApiWidgetProperties, HealthCheck } from '../Models/Models';

const ApiWidget: React.FC<ApiWidgetProperties> = (props) => {
  let myHealthChecksRepository = props.MyHealthChecksRepository;
  let myServerService = props.MyServerService;

  let [timestamp, setTimestamp] = useState(""); 
  let [status, setStatus] = useState('green');

  let dbResult = myHealthChecksRepository?.getDbResult(props.ReceiveMethod);
  let [result, setResult] = useState(dbResult);

  let last = myHealthChecksRepository?.getDbHealthChecks(props.ReceiveMethod)?.reverse().take(5);

  let [lastHealthChecks, setLastHealthChecks] = useState(last)
  
  ChartJS.register(ArcElement, Tooltip, Legend);  

  if (myServerService != undefined) {    
    myServerService.subscribe(props.ReceiveMethod, (report: any) => {
      console.log(report);
      setTimestamp(new Date().toLocaleString());

      var r = JSON.parse(report);

      let dbResult = myHealthChecksRepository?.getDbResult(props.ReceiveMethod);

      if (dbResult == null) return;

      let healthy = dbResult[0];
      let unHealthy = dbResult[1];
      
      if (r["Status"] == 2) {
        healthy = healthy + 1;
        setStatus('green');
      }
      else {
        unHealthy = unHealthy + 1;
        setStatus('red');
      }

      setResult([healthy, unHealthy]);

      var healthCheck = new HealthCheck();

      healthCheck.Api = props.ApiName;
      healthCheck.ReceiveMethod = props.ReceiveMethod;
      healthCheck.ReceiveTimeStamp = new Date().toLocaleString();
      healthCheck.Status = r["Status"];

      myHealthChecksRepository?.saveHealthChecks(props.ReceiveMethod, healthCheck);

      let last = myHealthChecksRepository?.getDbHealthChecks(props.ReceiveMethod)?.reverse().take(5);
      setLastHealthChecks(last);
    });
  }

  let data = {
    labels: ['Healthy', 'Unhealthy'],
    datasets: [
      {
        label: 'Api Health',
        data: result,
        backgroundColor: [
          'green',
          'red'
        ],
        borderColor: [
          'green',
          'red'
        ],
        borderWidth: 1,
      },
    ],
  };    

  return (
    <div className="card">
        <div className="card-header">
            <h1 className="text-center">{props.ApiName}</h1>
        </div>
        <div className="card-body">
            <div className="row">
                <div className="col-10" style={{textAlign: 'left'}}>
                    <b>Last Health Check</b><br />
                    {timestamp}                    
                </div>  
                <div className="col-2">
                    <div style={{width: '50px', height: '50px', float: 'right', backgroundColor: status}}></div>
                </div>
            </div>
            <hr />
            <div className="row">
                <div className="col-12" style={{textAlign:'center'}}>
                    <div style={{width: '80%', margin: '0 auto'}}>
                        <Pie data={data} />
                    </div>                    
                </div>
            </div>
            <hr />
            <div style={{textAlign: 'left'}}>
              <b>Last 5 Health Checks</b><br />
            </div>                        
            
            {
              lastHealthChecks?.toArray().map((hc, i) => {
                return <div><br /><div className="row"><div className="col-10" style={{textAlign: 'left'}}>{hc.ReceiveTimeStamp?.toLocaleLowerCase()}</div><div style={{width: '50px', height: '50px', float: 'right', backgroundColor: hc.Status == 2 ? 'green' : 'red'}}></div></div></div>
              })
            }
        </div>
    </div>
  );
};

export default ApiWidget;