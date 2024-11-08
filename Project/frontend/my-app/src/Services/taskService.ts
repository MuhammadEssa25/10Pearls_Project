import api from './api';
import { Task, User } from './types';

// Fetch users: retrieves a list of users (possibly for assigning tasks)
export const getUsers = async (): Promise<User[]> => {
    try {
        const response = await api.get('Auth');
        return response.data;
    } catch (error) {
        console.error('Error fetching users:', error);
        throw error;
    }
};

// Fetch all tasks: retrieves a list of tasks
export const getTasks = async (): Promise<Task[]> => {
    try {
        const response = await api.get('Task');
        return response.data;
    } catch (error) {
        console.error('Error fetching tasks:', error);
        throw error;
    }
};

// Fetch task details: retrieves specific task by ID
export const getTaskDetail = async (taskId: number): Promise<Task> => {
    try {
        const response = await api.get(`Task/${taskId}`);
        return response.data;
    } catch (error) {
        console.error('Error fetching task details:', error);
        throw error;
    }
};

// Create task: sends a request to create a new task
export const createTask = async (taskData: Omit<Task, 'id' | 'assignedToUser' | 'assignedToUserName'>): Promise<Task> => {
    try {
        const response = await api.post('Task', taskData);
        return response.data;
    } catch (error) {
        console.error('Error creating task:', error);
        throw error;
    }
};

// Update task: sends a request to update a task by ID
export const updateTask = async (taskId: number, taskData: Task): Promise<Task> => {
    try {
        const response = await api.put(`Task/${taskId}`, taskData);
        return response.data;
    } catch (error) {
        console.error('Error updating task:', error);
        throw error;
    }
};

// Get task counts: retrieves task counts based on user
export const getTaskCounts = async (userId: number): Promise<{ completed: number; inProgress: number; pending: number }> => {
    try {
        const response = await api.get(`Task/count`);
        return response.data;
    } catch (error) {
        console.error('Error getting task counts:', error);
        throw error;
    }
};

// Delete task: sends a request to delete a task by ID
export const deleteTask = async (taskId: number): Promise<void> => {
    try {
        await api.delete(`Task/${taskId}`);
    } catch (error) {
        console.error('Error deleting task:', error);
        throw error;
    }
};