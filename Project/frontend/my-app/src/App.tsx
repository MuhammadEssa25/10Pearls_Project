import React from 'react';
import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import { useAuth } from './Hooks/useAuth';
import Login from './Components/Auth/Login';
import Register from './Components/Auth/Register';
import Dashboard from './Components/Dashboard';
import TaskList from './Components/Task/TaskList';
import NewTask from './Components/Task/NewTask';
import TaskDetail from './Components/Task/TaskDetail';
import UserProfile from './Components/User';

const App: React.FC = () => {
  const { isAuthenticated } = useAuth(); // Get authentication state from custom hook

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        <Route
          path="/dashboard"
          element={isAuthenticated ? <Dashboard /> : <Navigate to="/login" />} 
        />
        <Route
          path="/tasklist"
          element={isAuthenticated ? <TaskList /> : <Navigate to="/login" />} 
        />
        <Route
          path="/tasklist/tasks/new"
          element={isAuthenticated ? <NewTask /> : <Navigate to="/login" />} 
        />
        <Route
          path="/task/:id"
          element={isAuthenticated ? <TaskDetail /> : <Navigate to="/login" />} 
        />
        <Route
          path="/user-profile"
          element={isAuthenticated ? <UserProfile /> : <Navigate to="/login" />} 
        />
        
        <Route
          path="/"
          element={isAuthenticated ? <Navigate to="/dashboard" /> : <Navigate to="/login" />}
        />
      </Routes>
    </Router>
  );
};

export default App;