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

  // Load auth state from localStorage on app start
  useEffect(() => {
    const storedAuth = localStorage.getItem('isLoggedIn');
    const storedUser = localStorage.getItem('user');
    const storedGuest = localStorage.getItem('isGuest');
    
    if (storedAuth === 'true' && storedUser) {
      setIsLoggedIn(true);
      setUser(JSON.parse(storedUser));
    } else if (storedGuest === 'true') {
      setIsGuest(true);
    }
  }, []);

  const login = (userData) => {
    setIsLoggedIn(true);
    setIsGuest(false);
    setUser(userData);
    localStorage.setItem('isLoggedIn', 'true');
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.removeItem('isGuest');
  };

  const logout = () => {
    setIsLoggedIn(false);
    setIsGuest(false);
    setUser(null);
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('user');
    localStorage.removeItem('isGuest');
  };

  const enterAsGuest = () => {
    setIsGuest(true);
    setIsLoggedIn(false);
    setUser(null);
    localStorage.setItem('isGuest', 'true');
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('user');
  };

  const register = (userData) => {
    // Save user data for future login
    localStorage.setItem('registeredUser', JSON.stringify(userData));
    return true; // Registration successful
  };

  const value = {
    isLoggedIn,
    isGuest,
    user,
    login,
    logout,
    register,
    enterAsGuest
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};