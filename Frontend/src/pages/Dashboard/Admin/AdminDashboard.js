import React, { useState, useEffect } from 'react';
import { useAuth } from '../../../contexts/AuthContext';
import { AdminService } from '../../../services/api/admin/adminService';
import UserManagement from './UserManagement';
import CourseManagement from './CourseManagement';
import TeacherPackageManagement from './TeacherPackageManagement';
import './AdminDashboard.css';

const AdminDashboard = () => {
  const { user, logout } = useAuth();
  const [activeSection, setActiveSection] = useState('overview');
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalCourses: 0,
    totalTeachers: 0,
    totalStudents: 0,
    blockedAccounts: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    setLoading(true);
    try {
      const [usersResult, coursesResult, teachersResult, studentsResult, blockedResult] = await Promise.all([
        AdminService.getAllUsers(),
        AdminService.getAllCourses(),
        AdminService.getTeachers(),
        AdminService.getStudentsByAllCourses(),
        AdminService.getBlockedAccounts()
      ]);

      setStats({
        totalUsers: usersResult.success ? (usersResult.data?.length || 0) : 0,
        totalCourses: coursesResult.success ? (coursesResult.data?.length || 0) : 0,
        totalTeachers: teachersResult.success ? (teachersResult.data?.length || 0) : 0,
        totalStudents: studentsResult.success ? (studentsResult.data?.length || 0) : 0,
        blockedAccounts: blockedResult.success ? (blockedResult.data?.length || 0) : 0
      });
    } catch (err) {
      console.error('Error loading stats:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
  };

  const renderContent = () => {
    switch (activeSection) {
      case 'users':
        return <UserManagement />;
      case 'courses':
        return <CourseManagement />;
      case 'packages':
        return <TeacherPackageManagement />;
      default:
        return (
          <>
            <div className="admin-overview">
              <h2>Quản lý hệ thống</h2>
              
              <div className="admin-cards">
                <div 
                  className="admin-card"
                  onClick={() => setActiveSection('users')}
                >
                  <div className="card-icon">
                    <i className="fas fa-users"></i>
                  </div>
                  <div className="card-content">
                    <h3>Quản lý người dùng</h3>
                    <p>Xem và quản lý tài khoản người dùng, khóa/mở khóa tài khoản</p>
                  </div>
                </div>

                <div 
                  className="admin-card"
                  onClick={() => setActiveSection('courses')}
                >
                  <div className="card-icon">
                    <i className="fas fa-book"></i>
                  </div>
                  <div className="card-content">
                    <h3>Quản lý khóa học</h3>
                    <p>Tạo, cập nhật và xóa các khóa học trong hệ thống</p>
                  </div>
                </div>

                <div 
                  className="admin-card"
                  onClick={() => setActiveSection('packages')}
                >
                  <div className="card-icon">
                    <i className="fas fa-gift"></i>
                  </div>
                  <div className="card-content">
                    <h3>Quản lý gói giáo viên</h3>
                    <p>Tạo và quản lý các gói đăng ký cho giáo viên</p>
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
              </div>
            </div>

            <div className="admin-stats">
              <h3>Thống kê nhanh</h3>
              {loading ? (
                <div className="loading">Đang tải...</div>
              ) : (
                <div className="stats-grid">
                  <div className="stat-item">
                    <div className="stat-number">{stats.totalUsers}</div>
                    <div className="stat-label">Tổng người dùng</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-number">{stats.totalCourses}</div>
                    <div className="stat-label">Khóa học</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-number">{stats.totalTeachers}</div>
                    <div className="stat-label">Giáo viên</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-number">{stats.totalStudents}</div>
                    <div className="stat-label">Học sinh</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-number">{stats.blockedAccounts}</div>
                    <div className="stat-label">Tài khoản bị khóa</div>
                  </div>
                </div>
              )}
            </div>
          </>
        );
    }
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

      <nav className="admin-nav">
        <button 
          className={`nav-btn ${activeSection === 'overview' ? 'active' : ''}`}
          onClick={() => setActiveSection('overview')}
        >
          <i className="fas fa-home"></i> Tổng quan
        </button>
        <button 
          className={`nav-btn ${activeSection === 'users' ? 'active' : ''}`}
          onClick={() => setActiveSection('users')}
        >
          <i className="fas fa-users"></i> Quản lý người dùng
        </button>
        <button 
          className={`nav-btn ${activeSection === 'courses' ? 'active' : ''}`}
          onClick={() => setActiveSection('courses')}
        >
          <i className="fas fa-book"></i> Quản lý khóa học
        </button>
        <button 
          className={`nav-btn ${activeSection === 'packages' ? 'active' : ''}`}
          onClick={() => setActiveSection('packages')}
        >
          <i className="fas fa-gift"></i> Gói giáo viên
        </button>
      </nav>

      <main className="admin-main">
        {renderContent()}
      </main>
    </div>
  );
};

export default AdminDashboard;