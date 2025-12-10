import React, { createContext, useContext, useState, useEffect } from "react";
import { AuthAPI, TokenManager } from "../services/api/user";
import { getUserInfoFromToken, getPrimaryRole, USER_ROLES } from "../utils/jwtUtils";

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
  const [userRole, setUserRole] = useState(null);
  const [userRoles, setUserRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Helper function to extract and set role information from token
  const extractAndSetRoles = (token) => {
    if (token) {
      const userInfo = getUserInfoFromToken(token);
      if (userInfo && userInfo.roles) {
        setUserRoles(userInfo.roles);
        setUserRole(getPrimaryRole(token));
      } else {
        setUserRoles([]);
        setUserRole(null);
      }
    } else {
      setUserRoles([]);
      setUserRole(null);
    }
  };

  // Initialize auth state from tokens
  useEffect(() => {
    const initializeAuth = async () => {
      setLoading(true);
      
      try {
        const accessToken = TokenManager.getAccessToken();
        const storedUser = localStorage.getItem('user');
        
        if (accessToken && !TokenManager.isTokenExpired(accessToken) && storedUser) {
          // Extract role information from token
          extractAndSetRoles(accessToken);
          
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
            extractAndSetRoles(null);
          }
        } else {
          // No valid token, check for guest mode
          const storedGuest = localStorage.getItem('isGuest');
          if (storedGuest === 'true') {
            setIsGuest(true);
          }
          extractAndSetRoles(null);
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
        TokenManager.clearTokens();
        setUser(null);
        setIsLoggedIn(false);
        extractAndSetRoles(null);
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
      
      if (result.success && result.data) {
        // result.data should already be unwrapped by authService
        // It should be: { accessToken, refreshToken, user, expiresAt }
        const userData = result.data.user;
        const accessToken = TokenManager.getAccessToken();
        
        if (!accessToken) {
          throw new Error('Không nhận được token từ server');
        }
        
        if (!userData) {
          console.error('No user data in response!', result.data);
          throw new Error('Không nhận được thông tin người dùng từ server');
        }
        
        // Extract role information from the access token
        extractAndSetRoles(accessToken);
        
        const userInfo = getUserInfoFromToken(accessToken);
        const primaryRole = getPrimaryRole(accessToken);
        
        // Verify user data matches token
        if (userInfo && userInfo.userId && userData.userId) {
          const tokenUserId = parseInt(userInfo.userId);
          const responseUserId = userData.userId;
          if (tokenUserId !== responseUserId) {
            console.error('User ID mismatch!', {
              tokenUserId,
              responseUserId
            });
            throw new Error('Thông tin người dùng không khớp với token');
          }
        }
        
        setIsLoggedIn(true);
        setIsGuest(false);
        setUser(userData);
        localStorage.removeItem('isGuest');
        
        return { 
          success: true, 
          user: userData,
          role: primaryRole,
          roles: userInfo?.roles || []
        };
      } else {
        const errorMsg = result.error || result.data?.message || 'Đăng nhập thất bại';
        console.error('Login failed:', errorMsg, result);
        throw new Error(errorMsg);
      }
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đăng nhập';
      console.error('[AuthContext] Login exception:', err);
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
      setUserRole(null);
      setUserRoles([]);
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
      console.log('updateProfile called with:', updateData);
      
      // Validation
      if (!updateData || !updateData.firstName || !updateData.lastName) {
        throw new Error('Vui lòng điền đầy đủ thông tin');
      }

      // Update profile information first
      console.log('Updating profile...');
      const result = await AuthAPI.updateProfile(updateData);
      console.log('Profile update result:', result);
      
      if (result.success) {
        // After successful update, fetch the full user profile to ensure we have all data
        console.log('Fetching full user profile after update...');
        const profileResult = await AuthAPI.getProfile();
        
        let updatedUser = null;
        
        if (profileResult.success && profileResult.data) {
          // Extract UserDto from profile response
          // Backend returns: { success: true, data: UserDto }
          // httpClient may wrap it: { success: true, data: { success: true, data: UserDto } }
          let userData = null;
          
          if (profileResult.data.data && typeof profileResult.data.data === 'object') {
            // Nested structure: result.data.data is the UserDto
            userData = profileResult.data.data;
          } else if (profileResult.data && typeof profileResult.data === 'object') {
            // Direct structure: result.data is the UserDto
            userData = profileResult.data;
          }
          
          // Normalize field names (handle both PascalCase and camelCase)
          if (userData) {
            updatedUser = {
              userId: userData.userId || userData.UserId,
              firstName: userData.firstName || userData.FirstName || '',
              lastName: userData.lastName || userData.LastName || '',
              email: userData.email || userData.Email || '',
              phoneNumber: userData.phoneNumber || userData.PhoneNumber || '',
              status: userData.status || userData.Status || ''
            };
          }
        }
        
        // Fallback: merge updateData with existing user data
        if (!updatedUser) {
          console.warn('Could not get full user profile, merging updateData with existing user data');
          updatedUser = {
            ...user, // Keep existing user data (email, userId, etc.)
            ...updateData // Override with new data (firstName, lastName, phoneNumber)
          };
        }
        
        // Ensure we have all required fields
        if (!updatedUser.email && user?.email) {
          updatedUser.email = user.email;
        }
        if (!updatedUser.userId && user?.userId) {
          updatedUser.userId = user.userId;
        }
        
        // Update user state with new data
        setUser(updatedUser);
        
        // Update localStorage to persist the changes
        localStorage.setItem('user', JSON.stringify(updatedUser));
        
        // Ensure user is still logged in
        setIsLoggedIn(true);
        setIsGuest(false);
        
        console.log('Profile updated successfully, user state:', updatedUser);
        return { success: true, data: updatedUser };
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
      console.log('sendPasswordResetEmail called with:', email);
      
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

  const verifyOTP = async (email, otpCode) => {
    try {
      clearError();
      
      if (!email || !otpCode) {
        throw new Error('Thiếu thông tin email hoặc mã OTP');
      }

      if (otpCode.length !== 6) {
        throw new Error('Mã OTP phải có 6 số');
      }

      // Call backend API to verify OTP
      console.log('OTP verification for:', email, 'with code:', otpCode);
      const result = await AuthAPI.verifyOTP({
        email,
        otpCode
      });
      
      if (result.success) {
        return { 
          success: true, 
          message: 'Mã OTP hợp lệ' 
        };
      } else {
        const errorMessage = result.error || result.data?.message || 'Mã OTP không chính xác hoặc đã hết hạn';
        setError(errorMessage);
        return { success: false, error: errorMessage };
      }
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
      await AuthAPI.forgotPassword(email);
      

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

  const resetPassword = async ({ email, otpCode, newPassword, confirmPassword }) => {
    try {
      clearError();
      
      if (!email || !otpCode || !newPassword || !confirmPassword) {
        throw new Error('Thiếu thông tin cần thiết');
      }

      if (newPassword.length < 6) {
        throw new Error('Mật khẩu phải có ít nhất 6 ký tự');
      }

      if (newPassword !== confirmPassword) {
        throw new Error('Mật khẩu xác nhận không khớp');
      }

      const result = await AuthAPI.resetPassword({
        email,
        otpCode,
        newPassword,
        confirmPassword
      });

      return result;
    } catch (err) {
      const errorMessage = err.message || 'Có lỗi xảy ra khi đặt lại mật khẩu';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  // Role utility functions
  const checkRole = (roleName) => {
    return userRoles.includes(roleName);
  };

  const isAdmin = () => checkRole(USER_ROLES.ADMIN);
  const isTeacher = () => checkRole(USER_ROLES.TEACHER);
  const isStudent = () => checkRole(USER_ROLES.STUDENT) || checkRole(USER_ROLES.USER);

  const value = {
    isLoggedIn,
    isGuest,
    user,
    userRole,
    userRoles,
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
    clearError,
    // Role utilities
    checkRole,
    isAdmin,
    isTeacher,
    isStudent,
    USER_ROLES
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};