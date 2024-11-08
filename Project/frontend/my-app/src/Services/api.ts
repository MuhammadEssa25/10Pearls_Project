import axios from 'axios';

// Create an Axios instance with a base URL for the API
const api = axios.create({
    baseURL: 'http://localhost:5205/api/',
});

// Request interceptor to attach the token to each request
api.interceptors.request.use(
    (config) => {
        console.log("API Request:", config.method?.toUpperCase(), config.url);
        const token = localStorage.getItem('token');
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`; // Attach token in headers
        }
        return config;
    },
    (error) => {
        console.error("API Request Error:", error);
        return Promise.reject(error); // Reject request if an error occurs
    }
);

// Response interceptor to log response and handle errors
api.interceptors.response.use(
    (response) => {
        console.log("API Response:", response.status, response.data);
        return response; // Return response if successful
    },
    (error) => {
        console.error("API Response Error:", error.response?.status, error.response?.data);
        return Promise.reject(error); // Reject response if an error occurs
    }
);

export default api;