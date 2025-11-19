import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import './Unauthorized.css';

const Unauthorized = () => {
  const navigate = useNavigate();
  const { userRole, logout } = useAuth();

  const handleGoToDashboard = () => {
    switch (userRole) {
      case 'Admin':
        navigate('/admin/dashboard');
        break;
      case 'Teacher':
        navigate('/teacher/dashboard');
        break;
      case 'Student':
      case 'User':
        navigate('/home');
        break;
      default:
        navigate('/home');
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="unauthorized-container">
      <div className="unauthorized-content">
        <div className="unauthorized-icon">
          <i className="fas fa-ban"></i>
        </div>
        
        <h1 className="unauthorized-title">Không có quyền truy cập</h1>
        
        <p className="unauthorized-message">
          Bạn không có quyền truy cập vào trang này. 
          Vui lòng liên hệ quản trị viên nếu bạn nghĩ đây là lỗi.
        </p>

        <div className="unauthorized-role-info">
          <span className="role-label">Vai trò hiện tại:</span>
          <span className="role-value">{userRole || 'Không xác định'}</span>
        </div>

        <div className="unauthorized-actions">
          <button 
            className="btn btn-primary"
            onClick={handleGoToDashboard}
          >
            <i className="fas fa-home"></i>
            Về trang chủ
          </button>
          
          <button 
            className="btn btn-secondary"
            onClick={handleLogout}
          >
            <i className="fas fa-sign-out-alt"></i>
            Đăng xuất
          </button>
        </div>
      </div>
    </div>
  );
};

export default Unauthorized;