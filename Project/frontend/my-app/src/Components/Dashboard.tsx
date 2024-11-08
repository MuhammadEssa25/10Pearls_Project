import React, { useEffect, useState } from 'react';
import { getTaskCounts } from '../Services/taskService';
import { useAuth } from '../Hooks/useAuth';
import Sidebar from './Sidebar';
import { useNavigate } from 'react-router-dom';

interface TaskCounts {
    completed: number;
    inProgress: number;
    pending: number;
}

const Dashboard: React.FC = () => {
    const { isAuthenticated, userId, username, role, logout } = useAuth();
    const navigate = useNavigate();
    const [taskCounts, setTaskCounts] = useState<TaskCounts>({
        completed: 0,
        inProgress: 0,
        pending: 0,
    });

    useEffect(() => {
        const fetchTaskCounts = async () => {
            if (!userId) {
                console.log('No userId available, skipping task count fetch');
                return;
            }
            console.log('Fetching task counts for userId:', userId);
            try {
                const counts = await getTaskCounts(userId);
                console.log('Received task counts:', counts);
                setTaskCounts(counts);
            } catch (error) {
                console.error('Error fetching task counts:', error);
            }
        };

        if (isAuthenticated && userId) {
            fetchTaskCounts();
        } else if (!isAuthenticated) {
            console.log('Not authenticated, redirecting to login');
            navigate('/login');
        }
    }, [isAuthenticated, userId, navigate]);

    useEffect(() => {
        console.log('Current auth state:', { isAuthenticated, userId, username, role });
    }, [isAuthenticated, userId, username, role]);

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
                <main className="col-md-9 ms-sm-auto col-lg-10 px-4">
                    <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                        <h2>Dashboard</h2>
                        <div className="d-flex align-items-center">
                            <span className="me-3">Welcome, {username || 'User'} ({role || 'N/A'})</span>
                            <button className="btn btn-outline-danger" onClick={handleLogout}>Logout</button>
                        </div>
                    </div>
                    <div className="row">
                        <div className="col-md-4">
                            <div className="card text-white bg-success mb-3">
                                <div className="card-header">Completed Tasks</div>
                                <div className="card-body">
                                    <h5 className="card-title">{taskCounts.completed}</h5>
                                    <p className="card-text">Tasks you have successfully completed.</p>
                                </div>
                            </div>
                        </div>
                        <div className="col-md-4">
                            <div className="card text-white bg-warning mb-3">
                                <div className="card-header">In Progress Tasks</div>
                                <div className="card-body">
                                    <h5 className="card-title">{taskCounts.inProgress}</h5>
                                    <p className="card-text">Tasks you are currently working on.</p>
                                </div>
                            </div>
                        </div>
                        <div className="col-md-4">
                            <div className="card text-white bg-danger mb-3">
                                <div className="card-header">Pending Tasks</div>
                                <div className="card-body">
                                    <h5 className="card-title">{taskCounts.pending}</h5>
                                    <p className="card-text">Tasks you have yet to start.</p>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="row mt-4">
                        <div className="col-12">
                            <h3>Your Task Summary</h3>
                            <p>You have a total of {taskCounts.completed + taskCounts.inProgress + taskCounts.pending} tasks.</p>
                            <ul>
                                <li>{taskCounts.completed} tasks completed</li>
                                <li>{taskCounts.inProgress} tasks in progress</li>
                                <li>{taskCounts.pending} tasks pending</li>
                            </ul>
                        </div>
                    </div>
                </main>
            </div>
        </div>
    );
};

export default Dashboard;