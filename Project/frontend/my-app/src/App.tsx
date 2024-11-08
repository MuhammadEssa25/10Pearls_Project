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
        {/* Public Routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        {/* Protected Routes */}
        <Route
          path="/dashboard"
          element={isAuthenticated ? <Dashboard /> : <Navigate to="/login" />} // Redirect to login if not authenticated
        />
        <Route
          path="/tasklist"
          element={isAuthenticated ? <TaskList /> : <Navigate to="/login" />} // Redirect to login if not authenticated
        />
        <Route
          path="/tasklist/tasks/new"
          element={isAuthenticated ? <NewTask /> : <Navigate to="/login" />} // Redirect to login if not authenticated
        />
        <Route
          path="/task/:id"
          element={isAuthenticated ? <TaskDetail /> : <Navigate to="/login" />} // Redirect to login if not authenticated
        />
        <Route
          path="/user-profile"
          element={isAuthenticated ? <UserProfile /> : <Navigate to="/login" />} // Redirect to login if not authenticated
        />
        
        {/* Default route redirects */}
        <Route
          path="/"
          element={isAuthenticated ? <Navigate to="/dashboard" /> : <Navigate to="/login" />}
        />
      </Routes>
    </Router>
  );
};

export default App;