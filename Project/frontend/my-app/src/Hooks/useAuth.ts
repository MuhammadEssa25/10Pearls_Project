import { useState, useEffect, useCallback } from 'react';
import { jwtDecode } from 'jwt-decode';

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
              return prevState; 
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
          logout(); 
        }
      } catch (error) {
        console.error('Error decoding token:', error);
        logout(); 
      }
    } else {
      logout(); 
    }
  }, []);


  useEffect(() => {
    checkAuthStatus();
  }, [checkAuthStatus]);

  const login = useCallback((token: string) => {
    localStorage.setItem('token', token);
    checkAuthStatus();
  }, [checkAuthStatus]);

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

  return { ...authState, login, logout, checkAuthStatus };
};