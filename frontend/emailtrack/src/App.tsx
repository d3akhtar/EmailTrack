import React from 'react';
import logo from './logo.svg';
import './App.css';
import { Route, Routes } from 'react-router-dom';
import { HomePage,LoginPage,MainPage,NotFound,SettingsPage,TeamPage } from './pages/_index';
import Header from './components/common/Header';

function App() {
  return (
    <div className="App">
      <Routes>
        <Route path="/" element={<HomePage/>}/>
        <Route path="/login" element={<LoginPage/>}/>
        <Route path="/main" element={<MainPage/>}/>
        <Route path="/team" element={<TeamPage/>}/>
        <Route path="/settings" element={<SettingsPage/>}/>
        <Route path="*" element={<NotFound/>}/>
      </Routes>
    </div>
  );
}

export default App;
