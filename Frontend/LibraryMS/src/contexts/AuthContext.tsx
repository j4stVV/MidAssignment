
import React, { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from '@/components/ui/sonner';
import api from '@/services/api';

interface User {
  id: string;
  name: string;
  email: string;
  role: 'User' | 'SuperUser';
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isLoading: boolean;
  login: (loginId: string, password: string) => Promise<void>;
  register: (loginId: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  // Check if user is logged in on initial load
  useEffect(() => {
    const storedUser = localStorage.getItem('bookwise_user');
    if (storedUser) {
      try {
        const parsedUser = JSON.parse(storedUser);
        setUser(parsedUser);
      } catch (error) {
        localStorage.removeItem('bookwise_user');
      }
    }
    setIsLoading(false);
  }, []);

  const login = async (loginId: string, password: string) => {
    try {
      setIsLoading(true);
      const response = await api.post('Auth/login', {
        loginId, //mail username
        password,
      });
      const { accessToken, refreshToken } = response.data;

      localStorage.setItem('access_token', accessToken);
      localStorage.setItem('refresh_token', refreshToken);

      const userResponse = await api.get('/Auth/profile');
      const userData: User = userResponse.data;

      localStorage.setItem('bookwise_user', JSON.stringify(userData));

      setUser(userData);
      toast.success(`Welcome, ${userData.name}!`);
      navigate('/dashboard', { replace: true });
    } catch (error: any) {
      toast.error(error.response.data.error);
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (loginId: string, password: string) => {
    try {
      await api.post('/Auth/register', {
        loginId,
        password,
      });

      toast.success('Registered successfully! Please log in.');
      navigate('/login');
    } catch (error: any) {
      toast.error(error.response.data.details.join() || 'Registration failed');
    }
  };

  const logout = async () => {
    try {
      const refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken) {
        await api.post('/Auth/logout', { refreshToken });
      }
      setUser(null);
      localStorage.removeItem('bookwise_user');
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      toast.info('Logged out successfully');
      navigate('/login');
    } catch (error: any) {
      toast.error(error.response?.data?.error || 'Logout failed');
      // Still clear local storage and navigate to login even if API call fails
      setUser(null);
      localStorage.removeItem('bookwise_user');
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      navigate('/login');
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isAdmin: user?.role === 'SuperUser',
        isLoading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};