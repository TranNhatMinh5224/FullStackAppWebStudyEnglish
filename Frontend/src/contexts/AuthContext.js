import React, { createContext, useContext, useState, useEffect } from "react";
import { AuthAPI, TokenManager } from "../services/api";

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

  // Initialize auth state from tokens
  useEffect(() => {
    const initializeAuth = async () => {
      setLoading(true);
      
      try {
        const accessToken = TokenManager.getAccessToken();
        const storedUser = localStorage.getItem('user');
        
        if (accessToken && !TokenManager.isTokenExpired(accessToken) && storedUser) {
          // Token exists and is valid, get fresh profile data
          const profileResult = await AuthAPI.getProfile();
          
          if (profileResult.success) {
            setUser(profileResult.data);
            setIsLoggedIn(true);
            localStorage.setItem('user', JSON.stringify(profileResult.data));
          } else {
            // Token might be invalid, clear everything
            TokenManager.clearTokens();
            setUser(null);
            setIsLoggedIn(false);
          }
        } else {
          // No valid token, check for guest mode
          const storedGuest = localStorage.getItem('isGuest');
          if (storedGuest === 'true') {
            setIsGuest(true);
          }
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
        TokenManager.clearTokens();
        setUser(null);
        setIsLoggedIn(false);
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const clearError = () => setError(null);

  const login = async (credentials) => {
    try {
      clearError();
      
      if (!credentials || !credentials.email || !credentials.password) {
        throw new Error('Vui lòng điền đầy đủ email và mật khẩu');
      }

      const result = await AuthAPI.login(credentials);
      
      if (result.success) {
        setIsLoggedIn(true);
        setIsGuest(false);
        setUser(result.data.user);
        localStorage.removeItem('isGuest');
        return { success: true, user: result.data.user };
      } else {
        throw new Error(result.error || 'Đăng nhập thất bại');
      }
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đăng nhập';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const logout = () => {
    try {
      clearError();
      AuthAPI.logout(); // This clears tokens
      setIsLoggedIn(false);
      setIsGuest(false);
      setUser(null);
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

  const register = async (userData) => {
    try {
      clearError();
      
      // Validation
      if (!userData || !userData.email || !userData.password || !userData.firstName || !userData.lastName) {
        throw new Error('Vui lòng điền đầy đủ thông tin');
      }

      if (userData.password.length < 6) {
        throw new Error('Mật khẩu phải có ít nhất 6 ký tự');
      }

      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(userData.email)) {
        throw new Error('Email không hợp lệ');
      }

      const result = await AuthAPI.register(userData);
      
      if (result.success) {
        return { success: true, message: 'Đăng ký thành công! Vui lòng đăng nhập.' };
      } else {
        throw new Error(result.error || 'Đăng ký thất bại');
      }
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đăng ký';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const updateProfile = async (updateData) => {
    try {
      clearError();
      console.log('AuthContext updateProfile called with:', updateData);
      
      // Validation
      if (!updateData || !updateData.firstName || !updateData.lastName) {
        throw new Error('Vui lòng điền đầy đủ thông tin');
      }

      // Update profile information first
      console.log('Updating profile...');
      const result = await AuthAPI.updateProfile(updateData);
      console.log('Profile update result:', result);
      
      if (result.success) {
        setUser(result.data);
        return { success: true };
      } else {
        throw new Error(result.error || 'Cập nhật thất bại');
      }
    } catch (err) {
      console.error('Update profile error in AuthContext:', err);
      const errorMessage = err.message || 'Có lỗi xảy ra khi cập nhật thông tin';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const changePassword = async (passwordData) => {
    try {
      clearError();
      console.log('AuthContext changePassword called');
      
      if (!passwordData.currentPassword || !passwordData.newPassword) {
        throw new Error('Vui lòng điền đầy đủ thông tin mật khẩu');
      }

      const result = await AuthAPI.changePassword(passwordData);
      console.log('Change password result:', result);
      
      if (result.success) {
        // Password changed successfully, but tokens are now invalid
        // Force logout and redirect to login
        TokenManager.clearTokens();
        setIsLoggedIn(false);
        setUser(null);
        return { 
          success: true, 
          message: 'Đổi mật khẩu thành công! Vui lòng đăng nhập lại.',
          requireRelogin: true 
        };
      } else {
        throw new Error(result.error || 'Không thể đổi mật khẩu');
      }
    } catch (err) {
      console.error('Change password error in AuthContext:', err);
      const errorMessage = err.message || 'Có lỗi xảy ra khi đổi mật khẩu';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const sendPasswordResetEmail = async (email) => {
    try {
      clearError();
      console.log('AuthContext sendPasswordResetEmail called with:', email);
      
      if (!email) {
        throw new Error('Vui lòng nhập email');
      }

      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        throw new Error('Email không hợp lệ');
      }

      console.log('Calling AuthAPI.forgotPassword...');
      const result = await AuthAPI.forgotPassword(email);
      console.log('Forgot password result:', result);
      return result;
    } catch (err) {
      console.error('Send password reset email error in AuthContext:', err);
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

      // For now, we'll just return success
      // Backend will validate when actually resetting password
      return { success: true };
    } catch (err) {
      const errorMessage = err.message || 'Token không hợp lệ';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const verifyOTP = async (email, otpCode) => {
    try {
      clearError();
      
      if (!email || !otpCode) {
        throw new Error('Thiếu thông tin email hoặc mã OTP');
      }

      if (otpCode.length !== 6) {
        throw new Error('Mã OTP phải có 6 số');
      }

      // For now, just validate format - actual verification happens in resetPassword
      console.log('OTP verification for:', email, 'with code:', otpCode);
      
      return { 
        success: true, 
        message: 'Mã OTP hợp lệ' 
      };
    } catch (err) {
      console.error('Verify OTP error:', err);
      const errorMessage = err.message || 'Mã OTP không hợp lệ';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const resendOTP = async (email) => {
    try {
      clearError();
      
      if (!email) {
        throw new Error('Thiếu địa chỉ email');
      }

      // Call the same forgot password API to resend OTP
      const result = await AuthAPI.forgotPassword(email);
      
      console.log('AuthContext resendOTP result:', result);
      
      return { 
        success: true, 
        message: 'Mã OTP mới đã được gửi đến email của bạn!' 
      };
    } catch (err) {
      console.error('Resend OTP error:', err);
      const errorMessage = err.message || 'Không thể gửi lại mã OTP';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const resetPassword = async ({ email, otpCode, newPassword }) => {
    try {
      clearError();
      
      if (!email || !otpCode || !newPassword) {
        throw new Error('Thiếu thông tin cần thiết');
      }

      if (newPassword.length < 6) {
        throw new Error('Mật khẩu phải có ít nhất 6 ký tự');
      }

      const result = await AuthAPI.resetPassword({
        email,
        otpCode,
        newPassword
      });

      return result;
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
    updateProfile,
    changePassword,
    sendPasswordResetEmail,
    verifyOTP,
    resendOTP,
    resetPassword,
    clearError
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};