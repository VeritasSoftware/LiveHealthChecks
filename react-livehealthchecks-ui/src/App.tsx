import React, { useEffect } from 'react';
import logo from './logo.svg';
import './App.css';
import ApiWidget from './Components/ApiWidget';
import { MyServerService } from './Services/MyServerService';
import { MyHealthChecksRepository } from './Repository/MyHealthChecksRepository';
import dashboardSettings from './dashboardSettings.json';


const signalr = require('@microsoft/signalr') 

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
