import React, { useState, useEffect } from 'react';
import { useAuth } from '../../Hooks/useAuth';
import Sidebar from '../Sidebar';
import { createTask, getUsers } from '../../Services/taskService';
import { User, Task } from '../../Services/types';
import { useNavigate } from 'react-router-dom';

const NewTask: React.FC = () => {
  const navigate = useNavigate();
  const { username } = useAuth();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [priority, setPriority] = useState('Normal');
  const [users, setUsers] = useState<User[]>([]);
  const [assignedToUserId, setAssignedToUserId] = useState<number | undefined>(undefined);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const userList = await getUsers();
        setUsers(userList);
      } catch (error) {
        console.error('Error fetching users:', error);
      }
    };
    fetchUsers();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!assignedToUserId) {
      alert('Please select a user to assign the task to.');
      return;
    }
    const newTask: Omit<Task, 'id' | 'assignedToUser' | 'assignedToUserName'> = {
      title,
      description,
      dueDate: dueDate || undefined,
      priority,
      status: 'Pending',
      assignedToUserId,
    };
    console.log("New Task Payload:", newTask);
    try {
      const createdTask = await createTask(newTask);
      console.log("Created Task:", createdTask);
      alert('Task created successfully');
      setTitle('');
      setDescription('');
      setDueDate('');
      setPriority('Normal');
      setAssignedToUserId(undefined);
    } catch (error: any) {
      console.error('Error creating task:', error.response?.data || error.message);
      alert(`Error creating task: ${error.response?.data?.error || error.message}`);
    }
  };

    return (
        <div className="container-fluid">
            <div className="row">
                <Sidebar />
                <main className="col-md-9 ms-sm-auto col-lg-10 px-4">
                <div className="d-flex justify-content-between align-items-center mt-4 mb-4">
                <h2>Add Task</h2>
                    <button 
                            className="btn btn-secondary" 
                            onClick={() => navigate('/tasklist')}
                        >
                            Back to List
                        </button>
                        </div>
                    <form onSubmit={handleSubmit} className="bg-light p-4 rounded border">
                        <div className="mb-3">
                            <label htmlFor="title" className="form-label">Title</label>
                            <input
                                type="text"
                                className="form-control"
                                id="title"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                required
                            />
                        </div>
                        <div className="mb-3">
                            <label htmlFor="description" className="form-label">Description</label>
                            <textarea
                                className="form-control"
                                id="description"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                required
                            />
                        </div>
                        <div className="mb-3">
                            <label htmlFor="dueDate" className="form-label">Due Date</label>
                            <input
                                type="date"
                                className="form-control"
                                id="dueDate"
                                value={dueDate}
                                onChange={(e) => setDueDate(e.target.value)}
                            />
                        </div>
                        <div className="mb-3">
                            <label htmlFor="priority" className="form-label">Priority</label>
                            <select
                                className="form-select"
                                id="priority"
                                value={priority}
                                onChange={(e) => setPriority(e.target.value)}
                            >
                                <option value="Low">Low</option>
                                <option value="Normal">Normal</option>
                                <option value="High">High</option>
                            </select>
                        </div>
                        <div className="mb-3">
                            <label htmlFor="assignedToUserId" className="form-label">Assigned To</label>
                            <select
                                className="form-select"
                                id="assignedToUserId"
                                value={assignedToUserId || ''}
                                onChange={(e) => setAssignedToUserId(Number(e.target.value))}
                                required
                            >
                                <option value="">Select a user</option>
                                {users.map(user => (
                                    <option key={user.id} value={user.id}>{user.name}</option>
                                ))}
                            </select>
                        </div>
                        <button type="submit" className="btn btn-primary">Create Task</button>
                    </form>
                </main>
            </div>
        </div>
    );
};

export default NewTask;