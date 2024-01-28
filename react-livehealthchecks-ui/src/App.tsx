import React from 'react';  
import './App.css';
import ApiWidget from './Components/ApiWidget';
import { MyServerService } from './Services/MyServerService';
import { MyHealthChecksRepository } from './Repository/MyHealthChecksRepository';
import dashboardSettings from './dashboardSettings.json';

const App: React.FC<{myServerService: MyServerService}> = ({myServerService}) => {
  let myHealthChecksRepository = new MyHealthChecksRepository();
  let apis = dashboardSettings.Apis;
  console.log(apis);
  var triple = (arr: string | any[]) => {
    var triples = []
    for (var i=0; i < arr.length; i=i+2) {
        triples.push([ arr[ i ]??null, arr[ i + 1 ]??null,arr[ i + 2 ]??null ])
    }
    return triples
  }
  return (
    <div className="App">
      <div className="row">
          <div className="col-sm col-md col" style={{textAlign: 'center'}}>
              <h2>Live Health Checks!</h2>
          </div>
          <div className="col-sm col-md col" style={{textAlign: 'center'}}>
              <h2><span style={{color:'green'}}>Healthy</span> vs <span style={{color:'red'}}>Unhealthy</span></h2>
          </div>
      </div>
      { 
        triple(apis).map((api, i) => {
          return <div className="row"><div className="col-sm col-md col"><ApiWidget ApiName={api[0].ApiName} ReceiveMethod={api[0].ReceiveMethod} MyServerService={myServerService} MyHealthChecksRepository={myHealthChecksRepository} /></div>
          {api[1] != null ? <div className="col-sm col-md col"><ApiWidget ApiName={api[1].ApiName} ReceiveMethod={api[1].ReceiveMethod} MyServerService={myServerService} MyHealthChecksRepository={myHealthChecksRepository} /></div> : <div className="col-sm col-md col"></div>}
          {api[2] != null ? <div className="col-sm col-md col"><ApiWidget ApiName={api[2].ApiName} ReceiveMethod={api[2].ReceiveMethod} MyServerService={myServerService} MyHealthChecksRepository={myHealthChecksRepository} /></div> : <div className="col-sm col-md col"></div>}
          </div>
        })
      }      
    </div>
  );
}

export default App;
