import React from 'react';
import { Link } from 'react-router-dom';

const Sidebar: React.FC = () => {
    return (
        <nav className="col-md-3 col-lg-2 d-md-block bg-light sidebar">
            <div className="position-sticky">
                <ul className="nav flex-column">
                    <li className="nav-item">
                        <Link className="nav-link" to="/dashboard">
                            Dashboard
                        </Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/tasklist">
                            Task List
                        </Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/user-profile">
                            User Profile
                        </Link>
                    </li>
                </ul>
            </div>
        </nav>
    );
};

export default Sidebar;