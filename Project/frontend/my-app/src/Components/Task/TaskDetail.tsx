import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getTaskDetail, updateTask, deleteTask, getUsers } from '../../Services/taskService';
import { Task, User } from '../../Services/types';
import Sidebar from '../Sidebar';
import { useAuth } from '../../Hooks/useAuth';

const TaskDetail: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { userId, role } = useAuth();
    const [task, setTask] = useState<Task | null>(null);
    const [loading, setLoading] = useState(true);
    const [users, setUsers] = useState<User[]>([]);
    const isAdmin = role === 'Admin';

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [taskData, userList] = await Promise.all([
                    getTaskDetail(Number(id)),
                    isAdmin ? getUsers() : Promise.resolve([])
                ]);
                setTask(taskData);
                setUsers(userList);
            } catch (error) {
                console.error('Error fetching data:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id, isAdmin]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setTask(prevTask => prevTask ? { ...prevTask, [name]: value } : null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (task) {
            try {
                await updateTask(task.id!, task);
                alert('Task updated successfully');
                navigate('/tasklist');
            } catch (error) {
                console.error('Error updating task:', error);
                alert('Failed to update task. Please try again.');
            }
        }
    };

    if (loading) return <div className="loading-spinner">Loading...</div>;
    if (!task) return <div className="alert alert-danger">Task not found</div>;

    return (
        <div className="container-fluid">
            <div className="row">
                <Sidebar />
                <main className="col-md-9 ms-sm-auto col-lg-10 px-4">
                    <div className="d-flex justify-content-between align-items-center mt-4 mb-4">
                        <h2>{isAdmin ? 'Edit Task' : 'View Task'}</h2>
                        <button 
                            className="btn btn-secondary" 
                            onClick={() => navigate('/tasklist')}
                        >
                            Back to List
                        </button>
                    </div>
                    <form onSubmit={handleSubmit} className="bg-light p-4 rounded border shadow-sm">
                        <div className="mb-3">
                            <label htmlFor="title" className="form-label">Title</label>
                            <input
                                type="text"
                                className="form-control"
                                id="title"
                                name="title"
                                value={task.title}
                                onChange={handleInputChange}
                                disabled={!isAdmin}
                                required
                            />
                        </div>
                        <div className="mb-3">
                            <label htmlFor="description" className="form-label">Description</label>
                            <textarea
                                className="form-control"
                                id="description"
                                name="description"
                                value={task.description}
                                onChange={handleInputChange}
                                disabled={!isAdmin}
                                required
                            />
                        </div>
                        <div className="row">
                            <div className="col-md-6 mb-3">
                                <label htmlFor="dueDate" className="form-label">Due Date</label>
                                <input
                                    type="date"
                                    className="form-control"
                                    id="dueDate"
                                    name="dueDate"
                                    value={task.dueDate}
                                    onChange={handleInputChange}
                                    disabled={!isAdmin}
                                />
                            </div>
                            <div className="col-md-6 mb-3">
                                <label htmlFor="priority" className="form-label">Priority</label>
                                <select
                                    className="form-select"
                                    id="priority"
                                    name="priority"
                                    value={task.priority}
                                    onChange={handleInputChange}
                                    disabled={!isAdmin}
                                >
                                    <option value="Low">Low</option>
                                    <option value="Normal">Normal</option>
                                    <option value="High">High</option>
                                </select>
                            </div>
                        </div>
                        <div className="mb-3">
                            <label htmlFor="status" className="form-label">Status</label>
                            <select
                                className="form-select"
                                id="status"
                                name="status"
                                value={task.status}
                                onChange={handleInputChange}
                            >
                                <option value="Pending">Pending</option>
                                <option value="In Progress">In Progress</option>
                                <option value="Completed">Completed</option>
                            </select>
                        </div>
                        {isAdmin && (
                            <div className="mb-3">
                                <label htmlFor="assignedToUserId" className="form-label">Assigned To</label>
                                <select
                                    className="form-select"
                                    id="assignedToUserId"
                                    name="assignedToUserId"
                                    value={task.assignedToUserId}
                                    onChange={handleInputChange}
                                >
                                    <option value="">Select a user</option>
                                    {users.map((user) => (
                                        <option key={user.id} value={user.id}>{user.name}</option>
                                    ))}
                                </select>
                            </div>
                        )}
                        <div className="d-flex justify-content-between">
                            <button type="submit" className="btn btn-primary">
                                {isAdmin ? 'Update Task' : 'Save Status'}
                            </button>
                            {isAdmin && (
                                <button 
                                    type="button" 
                                    className="btn btn-danger"
                                    onClick={() => {
                                        if (window.confirm('Are you sure you want to delete this task?')) {
                                            deleteTask(task.id!).then(() => {
                                                navigate('/tasklist');
                                            });
                                        }
                                    }}
                                >
                                    Delete Task
                                </button>
                            )}
                        </div>
                    </form>
                </main>
            </div>
        </div>
    );
};

export default TaskDetail;