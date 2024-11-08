import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getTasks, deleteTask } from '../../Services/taskService';
import { Task } from '../../Services/types';
import Sidebar from '../Sidebar';
import { useAuth } from '../../Hooks/useAuth';

const TaskList: React.FC = () => {
    const [tasks, setTasks] = useState<Task[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();
    const { role, isAuthenticated } = useAuth();

    useEffect(() => {
        const fetchTasks = async () => {
            if (!isAuthenticated) {
                navigate('/login');
                return;
            }

            try {
                const taskData = await getTasks();
                setTasks(taskData);
            } catch (error) {
                console.error('Error fetching tasks:', error);
                setError('Failed to fetch tasks. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchTasks();
    }, [isAuthenticated, navigate]);

    const handleDelete = async (taskId: number) => {
        if (window.confirm('Are you sure you want to delete this task?')) {
            try {
                await deleteTask(taskId);
                setTasks(tasks.filter(task => task.id !== taskId)); 
                alert('Task deleted successfully');
            } catch (error) {
                console.error('Error deleting task:', error);
                alert('Failed to delete task. Please try again.');
            }
        }
    };

    const handleView = (taskId: number) => {
        navigate(`/task/${taskId}`);
    };

    if (!isAuthenticated) {
        return <div>Please log in to view tasks.</div>;
    }

    if (loading) return <div className="loading-spinner">Loading tasks...</div>;

    if (error) return <div className="alert alert-danger">{error}</div>;

    return (
        <div className="container-fluid">
            <div className="row">
                <Sidebar />
                <main className="col-md-9 ms-sm-auto col-lg-10 px-4">
                    <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                        <h2 className="h2">Task List</h2>
                        {role === 'Admin' && (
                            <div className="btn-toolbar mb-2 mb-md-0">
                                <Link to="/tasklist/tasks/new" className="btn btn-sm btn-outline-secondary">
                                    Add New Task
                                </Link>
                            </div>
                        )}
                    </div>
                    {tasks.length === 0 ? (
                        <p>No tasks available.</p>
                    ) : (
                        <div className="table-responsive">
                            <table className="table table-striped table-sm">
                                <thead>
                                    <tr>
                                        <th scope="col">Task Title</th>
                                        <th scope="col">Status</th>
                                        {role === 'Admin' && <th scope="col">Assigned To</th>}
                                        <th scope="col">Due Date</th>
                                        <th scope="col">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {tasks.map(task => (
                                        <tr key={task.id}>
                                            <td>{task.title}</td>
                                            <td>{task.status}</td>
                                            {role === 'Admin' && <td>{task.assignedToUserName || (task.assignedToUser ? task.assignedToUser.name : 'Unassigned')}</td>}
                                            <td>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No Due Date'}</td>
                                            <td>
                                                <button className="btn btn-primary btn-sm me-2" onClick={() => handleView(task.id!)}>
                                                    {role === 'Admin' ? 'Edit' : 'View'}
                                                </button>
                                                {role === 'Admin' && (
                                                    <button className="btn btn-danger btn-sm" onClick={() => handleDelete(task.id!)}>
                                                        Delete
                                                    </button>
                                                )}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </main>
            </div>
        </div>
    );
};

export default TaskList;