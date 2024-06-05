import { useEffect, useState } from 'react';
import "bootstrap/dist/css/bootstrap.min.css"
import './App.css';
import Auth from './Auth';
import AuthProvider from "./AuthProvider";
import Routes from "./Routes";

function App() {
    const [user, setUser] = useState({name: '', isAuthenticated: false});

    return (
        <AuthProvider>
            <Routes />
        </AuthProvider>
    );
}

export default App;