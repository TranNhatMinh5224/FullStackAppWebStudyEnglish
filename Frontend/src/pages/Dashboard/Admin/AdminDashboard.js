import React from 'react';
import { useAuth } from '../../../contexts/AuthContext';
import './AdminDashboard.css';

const AdminDashboard = () => {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <div className="admin-dashboard">
      <header className="admin-header">
        <div className="admin-header-content">
          <h1>Admin Dashboard</h1>
          <div className="admin-user-info">
            <span>Xin chào, {user?.firstName} {user?.lastName}</span>
            <button onClick={handleLogout} className="logout-btn">
              Đăng xuất
            </button>
          </div>
        </div>
      </header>

      <main className="admin-main">
        <div className="admin-overview">
          <h2>Quản lý hệ thống</h2>
          
          <div className="admin-cards">
            <div className="admin-card">
              <div className="card-icon">
                <i className="fas fa-users"></i>
              </div>
              <div className="card-content">
                <h3>Quản lý người dùng</h3>
                <p>Xem và quản lý tài khoản người dùng</p>
              </div>
            </div>

            <div className="admin-card">
              <div className="card-icon">
                <i className="fas fa-book"></i>
              </div>
              <div className="card-content">
                <h3>Quản lý khóa học</h3>
                <p>Tạo và quản lý các khóa học</p>
              </div>
            </div>

            <div className="admin-card">
              <div className="card-icon">
                <i className="fas fa-chart-bar"></i>
              </div>
              <div className="card-content">
                <h3>Thống kê</h3>
                <p>Xem báo cáo và thống kê hệ thống</p>
              </div>
            </div>

            <div className="admin-card">
              <div className="card-icon">
                <i className="fas fa-cog"></i>
              </div>
              <div className="card-content">
                <h3>Cài đặt hệ thống</h3>
                <p>Cấu hình và quản lý hệ thống</p>
              </div>
            </div>
          </div>
        </div>

        <div className="admin-stats">
          <h3>Thống kê nhanh</h3>
          <div className="stats-grid">
            <div className="stat-item">
              <div className="stat-number">150</div>
              <div className="stat-label">Tổng người dùng</div>
            </div>
            <div className="stat-item">
              <div className="stat-number">25</div>
              <div className="stat-label">Khóa học</div>
            </div>
            <div className="stat-item">
              <div className="stat-number">8</div>
              <div className="stat-label">Giáo viên</div>
            </div>
            <div className="stat-item">
              <div className="stat-number">142</div>
              <div className="stat-label">Học sinh</div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default AdminDashboard;