import React, { useEffect } from 'react';
import logo from './logo.svg';
import './App.css';
import ApiWidget from './Components/ApiWidget';
import { MyServerService } from './Services/MyServerService';
import { MyHealthChecksRepository } from './Repository/MyHealthChecksRepository';


const signalr = require('@microsoft/signalr') 

const App: React.FC<{myServerService: MyServerService}> = ({myServerService}) => {
  let myHealthChecksRepository = new MyHealthChecksRepository();
  return (
    <div className="App">
      <ApiWidget 
          ApiName='Sample Api' 
          ReceiveMethod='SampleApiHealth' 
          MyServerService={myServerService} 
          MyHealthChecksRepository={myHealthChecksRepository} 
      />
      <hr />
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.tsx</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
  );
}

export default App;
