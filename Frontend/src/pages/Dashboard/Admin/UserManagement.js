import React, { useState, useEffect } from 'react';
import { AdminService } from '../../../services/api/admin/adminService';
import './AdminDashboard.css';

const UserManagement = () => {
  const [users, setUsers] = useState([]);
  const [teachers, setTeachers] = useState([]);
  const [students, setStudents] = useState([]);
  const [blockedUsers, setBlockedUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState('all');

  useEffect(() => {
    loadData();
  }, [activeTab]);

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      if (activeTab === 'all') {
        const result = await AdminService.getAllUsers();
        if (result.success) {
          setUsers(result.data || []);
        } else {
          setError(result.error || 'Không thể tải danh sách người dùng');
        }
      } else if (activeTab === 'teachers') {
        const result = await AdminService.getTeachers();
        if (result.success) {
          setTeachers(result.data || []);
        } else {
          setError(result.error || 'Không thể tải danh sách giáo viên');
        }
      } else if (activeTab === 'students') {
        const result = await AdminService.getStudentsByAllCourses();
        if (result.success) {
          setStudents(result.data || []);
        } else {
          setError(result.error || 'Không thể tải danh sách học sinh');
        }
      } else if (activeTab === 'blocked') {
        const result = await AdminService.getBlockedAccounts();
        if (result.success) {
          setBlockedUsers(result.data || []);
        } else {
          setError(result.error || 'Không thể tải danh sách tài khoản bị khóa');
        }
      }
    } catch (err) {
      setError('Có lỗi xảy ra khi tải dữ liệu');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleBlock = async (userId) => {
    if (!window.confirm('Bạn có chắc chắn muốn khóa tài khoản này?')) return;
    
    try {
      const result = await AdminService.blockAccount(userId);
      if (result.success) {
        alert('Khóa tài khoản thành công');
        loadData();
      } else {
        alert(result.error || 'Không thể khóa tài khoản');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const handleUnblock = async (userId) => {
    if (!window.confirm('Bạn có chắc chắn muốn mở khóa tài khoản này?')) return;
    
    try {
      const result = await AdminService.unblockAccount(userId);
      if (result.success) {
        alert('Mở khóa tài khoản thành công');
        loadData();
      } else {
        alert(result.error || 'Không thể mở khóa tài khoản');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const getCurrentData = () => {
    switch (activeTab) {
      case 'teachers': return teachers;
      case 'students': return students;
      case 'blocked': return blockedUsers;
      default: return users;
    }
  };

  const renderUserTable = (data) => {
    if (!data || data.length === 0) {
      return <p className="no-data">Không có dữ liệu</p>;
    }

    return (
      <table className="admin-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Họ tên</th>
            <th>Email</th>
            <th>Số điện thoại</th>
            <th>Vai trò</th>
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {data.map((user) => (
            <tr key={user.userId}>
              <td>{user.userId}</td>
              <td>{user.firstName} {user.lastName}</td>
              <td>{user.email}</td>
              <td>{user.phoneNumber || '-'}</td>
              <td>
                {user.roles?.map(role => role.roleName || role).join(', ') || 'User'}
              </td>
              <td>
                <span className={`status-badge ${user.status === 'Active' ? 'active' : 'blocked'}`}>
                  {user.status === 'Active' ? 'Hoạt động' : 'Bị khóa'}
                </span>
              </td>
              <td>
                {user.status === 'Active' ? (
                  <button 
                    className="btn-danger"
                    onClick={() => handleBlock(user.userId)}
                  >
                    Khóa
                  </button>
                ) : (
                  <button 
                    className="btn-success"
                    onClick={() => handleUnblock(user.userId)}
                  >
                    Mở khóa
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  };

  return (
    <div className="admin-section">
      <h2>Quản lý người dùng</h2>
      
      <div className="tab-container">
        <button 
          className={`tab-btn ${activeTab === 'all' ? 'active' : ''}`}
          onClick={() => setActiveTab('all')}
        >
          Tất cả
        </button>
        <button 
          className={`tab-btn ${activeTab === 'teachers' ? 'active' : ''}`}
          onClick={() => setActiveTab('teachers')}
        >
          Giáo viên
        </button>
        <button 
          className={`tab-btn ${activeTab === 'students' ? 'active' : ''}`}
          onClick={() => setActiveTab('students')}
        >
          Học sinh
        </button>
        <button 
          className={`tab-btn ${activeTab === 'blocked' ? 'active' : ''}`}
          onClick={() => setActiveTab('blocked')}
        >
          Tài khoản bị khóa
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}
      
      {loading ? (
        <div className="loading">Đang tải...</div>
      ) : (
        renderUserTable(getCurrentData())
      )}
    </div>
  );
};

export default UserManagement;

