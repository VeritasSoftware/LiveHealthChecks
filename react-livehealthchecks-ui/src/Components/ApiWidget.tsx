import React, { useEffect, useState } from 'react';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import { Pie } from 'react-chartjs-2';
import { ApiWidgetProperties, HealthCheck } from '../Models/Models';

const ApiWidget: React.FC<ApiWidgetProperties> = (props) => {
  ChartJS.register(ArcElement, Tooltip, Legend); 

  let myHealthChecksRepository = props.MyHealthChecksRepository!;
  let myServerService = props.MyServerService;

  let [timestamp, setTimestamp] = useState(""); 
  let [status, setStatus] = useState('green');

  let dbResult = myHealthChecksRepository?.getDbResult(props.ReceiveMethod);
  let [result, setResult] = useState(dbResult);  

  let [lastHealthChecks, setLastHealthChecks] = useState(new Array<HealthCheck>());

  let [total, setTotal] = useState(0);
  let [totalHealthy, setTotalHealthy] = useState(0);
  let [totalUnhealthy, setTotalUnhealthy] = useState(0);
  let [healthyPercent, setHealthyPercent] = useState(0);
  let [unhealthyPercent, setUnhealthyPercent] = useState(0);
  
  let dbHealthChecks = myHealthChecksRepository.getDbHealthChecks(props.ReceiveMethod);

  let [healthChecks, setHealthChecks] = useState(dbHealthChecks);  

  useEffect(()=>{
    let last = healthChecks.reverse().take(5);

    setLastHealthChecks(last.toArray());

    total = healthChecks.count();
    totalHealthy = healthChecks.count(hc => hc.Status == 2);
    totalUnhealthy = healthChecks.count(hc => hc.Status == 1);
    
    healthyPercent = Math.floor(((totalHealthy * 100) / total)); 
    unhealthyPercent = Math.floor((totalUnhealthy * 100) / total);
    
    setTotal(total);
    setTotalHealthy(totalHealthy);
    setTotalUnhealthy(totalUnhealthy);
    setHealthyPercent(healthyPercent);
    setUnhealthyPercent(unhealthyPercent);
  },[healthChecks])   

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
      healthCheck.Status = r["Status"] == 2 ? 2 : 1;

      myHealthChecksRepository.saveHealthChecks(props.ReceiveMethod, healthCheck);

      let dbHealthChecks = myHealthChecksRepository.getDbHealthChecks(props.ReceiveMethod);
      setHealthChecks(dbHealthChecks);
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
                    <div style={{width: '70%', margin: '0 auto'}}>
                        <Pie data={data} />
                    </div>                    
                </div>
            </div>
            <br /> <br />
            <div className="row">
              <div className="col-6" style={{textAlign: 'left'}}>
                  <b>Total Health Checks</b>
              </div>
              <div className="col-3">
                  {total}
              </div>
              <div className="col-3">
                  <br />
              </div>
            </div>
            <div className="row">
              <div className="col-6" style={{textAlign: 'left'}}>
                  <b>Total <span style={{color:'green'}}>Healthy</span> Checks</b>
              </div>
              <div className="col-3">
                  {totalHealthy}
              </div>
              <div className="col-3">
                  {healthyPercent} %
              </div>
            </div> 
            <div className="row">
              <div className="col-6" style={{textAlign: 'left'}}>
                  <b>Total <span style={{color:'red'}}>Unhealthy</span> Checks</b>
              </div>
              <div className="col-3">
                  {totalUnhealthy}
              </div>
              <div className="col-3">
                  {unhealthyPercent} %
              </div>
            </div>           
            <hr />
            <div style={{textAlign: 'left'}}>
              <b>Last 5 Health Checks</b><br />
            </div>                                    
            {
              lastHealthChecks.map((hc, i) => {
                return <div><br /><div className="row"><div className="col-10" style={{textAlign: 'left'}}>{hc.ReceiveTimeStamp?.toLocaleLowerCase()}</div><div style={{width: '50px', height: '50px', float: 'right', backgroundColor: hc.Status == 2 ? 'green' : 'red'}}></div></div></div>
              })
            }
        </div>
    </div>
  );
};

export default ApiWidget;