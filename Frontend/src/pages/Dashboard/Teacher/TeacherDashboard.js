import React from 'react';
import { useAuth } from '../../../contexts/AuthContext';
import './TeacherDashboard.css';

const TeacherDashboard = () => {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <div className="teacher-dashboard">
      <header className="teacher-header">
        <div className="teacher-header-content">
          <h1>Teacher Dashboard</h1>
          <div className="teacher-user-info">
            <span>Xin chào, {user?.firstName} {user?.lastName}</span>
            <button onClick={handleLogout} className="logout-btn">
              Đăng xuất
            </button>
          </div>
        </div>
      </header>

      <main className="teacher-main">
        <div className="teacher-overview">
          <h2>Quản lý giảng dạy</h2>
          
          <div className="teacher-cards">
            <div className="teacher-card">
              <div className="card-icon">
                <i className="fas fa-chalkboard-teacher"></i>
              </div>
              <div className="card-content">
                <h3>Khóa học của tôi</h3>
                <p>Quản lý các khóa học bạn đang giảng dạy</p>
              </div>
            </div>

            <div className="teacher-card">
              <div className="card-icon">
                <i className="fas fa-users"></i>
              </div>
              <div className="card-content">
                <h3>Học sinh</h3>
                <p>Xem danh sách học sinh và tiến độ học</p>
              </div>
            </div>

            <div className="teacher-card">
              <div className="card-icon">
                <i className="fas fa-file-alt"></i>
              </div>
              <div className="card-content">
                <h3>Bài tập & Kiểm tra</h3>
                <p>Tạo và quản lý bài tập, bài kiểm tra</p>
              </div>
            </div>

            <div className="teacher-card">
              <div className="card-icon">
                <i className="fas fa-chart-line"></i>
              </div>
              <div className="card-content">
                <h3>Báo cáo tiến độ</h3>
                <p>Theo dõi tiến độ học tập của học sinh</p>
              </div>
            </div>
          </div>
        </div>

        <div className="teacher-stats">
          <h3>Thống kê giảng dạy</h3>
          <div className="stats-grid">
            <div className="stat-item">
              <div className="stat-number">5</div>
              <div className="stat-label">Khóa học</div>
            </div>
            <div className="stat-item">
              <div className="stat-number">42</div>
              <div className="stat-label">Học sinh</div>
            </div>
            <div className="stat-item">
              <div className="stat-number">15</div>
              <div className="stat-label">Bài tập</div>
            </div>
            <div className="stat-item">
              <div className="stat-number">8</div>
              <div className="stat-label">Bài kiểm tra</div>
            </div>
          </div>
        </div>

        <div className="teacher-recent">
          <h3>Hoạt động gần đây</h3>
          <div className="recent-activities">
            <div className="activity-item">
              <div className="activity-icon">
                <i className="fas fa-plus"></i>
              </div>
              <div className="activity-content">
                <h4>Tạo bài tập mới</h4>
                <p>Bài tập từ vựng - Unit 5</p>
                <span className="activity-time">2 giờ trước</span>
              </div>
            </div>
            
            <div className="activity-item">
              <div className="activity-icon">
                <i className="fas fa-check"></i>
              </div>
              <div className="activity-content">
                <h4>Chấm bài kiểm tra</h4>
                <p>Kiểm tra giữa kỳ - Lớp A1</p>
                <span className="activity-time">1 ngày trước</span>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default TeacherDashboard;