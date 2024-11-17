import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5205/api/',
});

api.interceptors.request.use(
    (config) => {
        console.log("API Request:", config.method?.toUpperCase(), config.url);
        const token = localStorage.getItem('token');
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        console.error("API Request Error:", error);
        return Promise.reject(error); 
    }
);

api.interceptors.response.use(
    (response) => {
        console.log("API Response:", response.status, response.data);
        return response; 
    },
    (error) => {
        console.error("API Response Error:", error.response?.status, error.response?.data);
        return Promise.reject(error); 
    }
);

export default api;