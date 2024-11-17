import api from './api';

export const loginUser = async (name: string, password: string) => {
    try {
        const response = await api.post('Auth/login', { name, password });
        if (response.data?.token) {
            localStorage.setItem('token', response.data.token); 
            console.log('Token stored in localStorage:', response.data.token);
        }
        return response.data;
    } catch (error) {
        console.error('Login failed:', error);
        throw error;
    }
};

export const registerUser = async (name: string, email: string, password: string) => {
    try {
        const response = await api.post('Auth/register', { name, email, password });
        return response.data; 
    } catch (error) {
        console.error('Registration failed:', error);
        throw error; 
    }
};

export const logout = () => {
    localStorage.removeItem('token');
    console.log('Token removed from localStorage');
};