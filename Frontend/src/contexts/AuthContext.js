import React, { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isGuest, setIsGuest] = useState(false);
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Load auth state from localStorage on app start
  useEffect(() => {
    try {
      const storedAuth = localStorage.getItem('isLoggedIn');
      const storedUser = localStorage.getItem('user');
      const storedGuest = localStorage.getItem('isGuest');
      
      if (storedAuth === 'true' && storedUser) {
        const userData = JSON.parse(storedUser);
        setIsLoggedIn(true);
        setUser(userData);
      } else if (storedGuest === 'true') {
        setIsGuest(true);
      }
    } catch (err) {
      console.error('Error loading auth state:', err);
      setError('Có lỗi khi tải thông tin đăng nhập');
      // Clear potentially corrupted data
      localStorage.removeItem('isLoggedIn');
      localStorage.removeItem('user');
      localStorage.removeItem('isGuest');
    } finally {
      setLoading(false);
    }
  }, []);

  const clearError = () => setError(null);

  const login = (userData) => {
    try {
      clearError();
      
      if (!userData || !userData.email) {
        throw new Error('Thông tin đăng nhập không hợp lệ');
      }

      setIsLoggedIn(true);
      setIsGuest(false);
      setUser(userData);
      localStorage.setItem('isLoggedIn', 'true');
      localStorage.setItem('user', JSON.stringify(userData));
      localStorage.removeItem('isGuest');
      
      return { success: true };
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đăng nhập';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const logout = () => {
    try {
      clearError();
      setIsLoggedIn(false);
      setIsGuest(false);
      setUser(null);
      localStorage.removeItem('isLoggedIn');
      localStorage.removeItem('user');
      localStorage.removeItem('isGuest');
      
      return { success: true };
    } catch (err) {
      console.error('Error during logout:', err);
      setError('Có lỗi khi đăng xuất');
      return { success: false, error: 'Có lỗi khi đăng xuất' };
    }
  };

  const enterAsGuest = () => {
    try {
      clearError();
      setIsGuest(true);
      setIsLoggedIn(false);
      setUser(null);
      localStorage.setItem('isGuest', 'true');
      localStorage.removeItem('isLoggedIn');
      localStorage.removeItem('user');
      
      return { success: true };
    } catch (err) {
      console.error('Error entering as guest:', err);
      setError('Có lỗi khi vào chế độ khách');
      return { success: false, error: 'Có lỗi khi vào chế độ khách' };
    }
  };

  const register = (userData) => {
    try {
      clearError();
      
      // Validation
      if (!userData || !userData.email || !userData.password || !userData.name) {
        throw new Error('Vui lòng điền đầy đủ thông tin');
      }

      if (userData.password.length < 6) {
        throw new Error('Mật khẩu phải có ít nhất 6 ký tự');
      }

      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(userData.email)) {
        throw new Error('Email không hợp lệ');
      }

      // Check if user already exists
      const existingUser = localStorage.getItem('registeredUser');
      if (existingUser) {
        const existing = JSON.parse(existingUser);
        if (existing.email === userData.email) {
          throw new Error('Email này đã được đăng ký');
        }
      }

      // Save user data for future login
      localStorage.setItem('registeredUser', JSON.stringify(userData));
      
      return { success: true };
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đăng ký';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const validateLogin = (email, password) => {
    try {
      clearError();
      
      if (!email || !password) {
        throw new Error('Vui lòng điền đầy đủ email và mật khẩu');
      }

      // Check if user exists (from registration)
      const registeredUser = localStorage.getItem('registeredUser');
      
      if (!registeredUser) {
        throw new Error('Tài khoản không tồn tại. Vui lòng đăng ký trước!');
      }

      const user = JSON.parse(registeredUser);
      
      // Simple validation
      if (email !== user.email || password !== user.password) {
        throw new Error('Email hoặc mật khẩu không đúng!');
      }

      return { success: true, user };
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi xác thực';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const value = {
    isLoggedIn,
    isGuest,
    user,
    loading,
    error,
    login,
    logout,
    register,
    enterAsGuest,
    validateLogin,
    clearError
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};