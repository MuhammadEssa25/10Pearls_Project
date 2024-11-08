import { useState, useEffect, useCallback } from 'react';
import { jwtDecode } from 'jwt-decode';

// Define types for the decoded JWT token and authentication state
interface DecodedToken {
  unique_name: string;
  nameid: string;
  email: string;
  role: string;
  exp: number;
}

interface AuthState {
  isAuthenticated: boolean;
  userId: number | null;
  email: string;
  username: string;
  role: string;
}

export const useAuth = () => {
  // Initialize state with a function to check token validity on first load
  const [authState, setAuthState] = useState<AuthState>(() => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode<DecodedToken>(token);
        const currentTime = Date.now() / 1000;
        if (decoded.exp > currentTime) {
          return {
            isAuthenticated: true,
            userId: parseInt(decoded.nameid, 10),
            username: decoded.unique_name,
            email: decoded.email,
            role: decoded.role,
          };
        }
      } catch (error) {
        console.error('Error decoding token:', error);
      }
    }
    return {
      isAuthenticated: false,
      userId: null,
      username: '',
      email: '',
      role: '',
    };
  });

  // Check authentication status from localStorage token
  const checkAuthStatus = useCallback(() => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode<DecodedToken>(token);
        const currentTime = Date.now() / 1000;
        
        if (decoded.exp > currentTime) {
          setAuthState(prevState => {
            const isTokenValid = (
              prevState.isAuthenticated &&
              prevState.userId === parseInt(decoded.nameid, 10) &&
              prevState.username === decoded.unique_name &&
              prevState.email === decoded.email &&
              prevState.role === decoded.role
            );
            if (isTokenValid) {
              return prevState; // Return previous state if token is still valid
            }
            return {
              isAuthenticated: true,
              userId: parseInt(decoded.nameid, 10),
              username: decoded.unique_name,
              email: decoded.email,
              role: decoded.role,
            };
          });
        } else {
          logout(); // Expired token, log out the user
        }
      } catch (error) {
        console.error('Error decoding token:', error);
        logout(); // Token decoding error, log out the user
      }
    } else {
      logout(); // No token found, log out
    }
  }, []);

  // Run checkAuthStatus on component mount
  useEffect(() => {
    checkAuthStatus();
  }, [checkAuthStatus]);

  // Login method: save the token and recheck authentication
  const login = useCallback((token: string) => {
    localStorage.setItem('token', token);
    checkAuthStatus();
  }, [checkAuthStatus]);

  // Logout method: remove token and reset authentication state
  const logout = useCallback(() => {
    localStorage.removeItem('token');
    setAuthState({
      isAuthenticated: false,
      userId: null,
      username: '',
      email: '',
      role: '',
    });
  }, []);

  // Return authentication state and methods
  return { ...authState, login, logout, checkAuthStatus };
};
