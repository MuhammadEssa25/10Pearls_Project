import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../Hooks/useAuth';
import Sidebar from './Sidebar';

const UserProfile: React.FC = () => {
  const { username, role, email, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
    }
  }, [isAuthenticated, navigate]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!isAuthenticated) {
    return <div>Loading...</div>; 
  }

  return (
    <div className="container-fluid">
      <div className="row">
        <Sidebar />
        <main className="col-md-9 ms-sm-auto col-lg-10 px-md-4">
          <div className="container mt-5">
            <div className="row justify-content-center">
              <div className="col-md-8">
                <div className="card">
                  <div className="card-header bg-primary text-white">
                    <h3 className="mb-0">User Profile</h3>
                  </div>
                  <div className="card-body">
                    <div className="text-center mb-4">
                      <img
                        src={`https://api.dicebear.com/6.x/initials/svg?seed=${username}`}
                        alt="Profile"
                        className="rounded-circle"
                        width="100"
                        height="100"
                      />
                    </div>
                    <ul className="list-group list-group-flush">
                      <li className="list-group-item">
                        <strong>Username:</strong> {username || 'N/A'}
                      </li>
                      <li className="list-group-item">
                        <strong>Role:</strong> {role || 'N/A'}
                      </li>
                      <li className="list-group-item">
                        <strong>User Email:</strong> {email || 'N/A'}
                      </li>
                    </ul>
                  </div>
                  <div className="card-footer">
                    <button
                      className="btn btn-danger btn-block"
                      onClick={handleLogout}
                    >
                      Logout
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};

export default UserProfile;