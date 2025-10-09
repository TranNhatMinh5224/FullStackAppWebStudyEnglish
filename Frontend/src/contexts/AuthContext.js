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

  const updateProfile = (updateData) => {
    try {
      clearError();
      
      // Validation
      if (!updateData || !updateData.firstName || !updateData.lastName) {
        throw new Error('Vui lòng điền đầy đủ thông tin');
      }

      // Get current user data
      const registeredUser = localStorage.getItem('registeredUser');
      if (!registeredUser) {
        throw new Error('Không tìm thấy thông tin tài khoản');
      }

      const currentUser = JSON.parse(registeredUser);

      // Validate current password if changing password
      if (updateData.password && updateData.currentPassword) {
        if (currentUser.password !== updateData.currentPassword) {
          throw new Error('Mật khẩu hiện tại không đúng');
        }
      }

      // Update user data (keep existing email)
      const updatedUser = {
        ...currentUser,
        firstName: updateData.firstName,
        lastName: updateData.lastName,
        name: updateData.name,
        email: currentUser.email, // Keep existing email unchanged
        phoneNumber: updateData.phoneNumber,
        avatar: updateData.avatar || currentUser.avatar,
        password: updateData.password || currentUser.password
      };

      // Save updated data
      localStorage.setItem('registeredUser', JSON.stringify(updatedUser));
      
      // Update current user state
      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
      
      return { success: true };
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi cập nhật thông tin';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const sendPasswordResetEmail = async (email) => {
    try {
      clearError();
      
      if (!email) {
        throw new Error('Vui lòng nhập email');
      }

      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        throw new Error('Email không hợp lệ');
      }

      // Check if user exists
      const registeredUser = localStorage.getItem('registeredUser');
      if (!registeredUser) {
        throw new Error('Email này chưa được đăng ký');
      }

      const user = JSON.parse(registeredUser);
      if (user.email !== email) {
        throw new Error('Email này chưa được đăng ký');
      }

      // Generate reset token (in real app, this would be done by backend)
      const resetToken = Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
      const resetExpiry = Date.now() + 15 * 60 * 1000; // 15 minutes

      // Save reset token
      localStorage.setItem('passwordResetToken', JSON.stringify({
        token: resetToken,
        email: email,
        expiry: resetExpiry
      }));

      // In real app, you would send SMTP email here
      console.log('SMTP Email would be sent with reset link:', 
        `${window.location.origin}/reset-password?token=${resetToken}&email=${encodeURIComponent(email)}`
      );

      return { success: true, resetToken };
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi gửi email';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const validateResetToken = async (token, email) => {
    try {
      clearError();
      
      if (!token || !email) {
        throw new Error('Token hoặc email không hợp lệ');
      }

      const resetData = localStorage.getItem('passwordResetToken');
      if (!resetData) {
        throw new Error('Token không tồn tại');
      }

      const { token: savedToken, email: savedEmail, expiry } = JSON.parse(resetData);

      if (token !== savedToken || email !== savedEmail) {
        throw new Error('Token không hợp lệ');
      }

      if (Date.now() > expiry) {
        // Clean up expired token
        localStorage.removeItem('passwordResetToken');
        throw new Error('Token đã hết hạn');
      }

      return { success: true };
    } catch (err) {
      const errorMessage = err.message || 'Token không hợp lệ';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const resetPassword = async ({ token, email, newPassword }) => {
    try {
      clearError();
      
      // Validate inputs
      if (!token || !email || !newPassword) {
        throw new Error('Thiếu thông tin cần thiết');
      }

      if (newPassword.length < 6) {
        throw new Error('Mật khẩu phải có ít nhất 6 ký tự');
      }

      // Validate token first
      const tokenValidation = await validateResetToken(token, email);
      if (!tokenValidation.success) {
        throw new Error(tokenValidation.error);
      }

      // Get and update user data
      const registeredUser = localStorage.getItem('registeredUser');
      if (!registeredUser) {
        throw new Error('Không tìm thấy thông tin tài khoản');
      }

      const user = JSON.parse(registeredUser);
      if (user.email !== email) {
        throw new Error('Email không khớp với tài khoản');
      }

      // Update password
      const updatedUser = {
        ...user,
        password: newPassword
      };

      // Save updated user
      localStorage.setItem('registeredUser', JSON.stringify(updatedUser));

      // Clean up reset token
      localStorage.removeItem('passwordResetToken');

      return { success: true };
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đặt lại mật khẩu';
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
    updateProfile,
    sendPasswordResetEmail,
    validateResetToken,
    resetPassword,
    clearError
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};