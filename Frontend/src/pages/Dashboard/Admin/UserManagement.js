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
        if (result.success && result.data) {
          // API returns array of courses, each course has users array
          // Need to flatten all users from all courses
          let coursesWithUsers = result.data;
          
          // If result.data is wrapped in another object (ServiceResponse), unwrap it
          if (result.data.success !== undefined && Array.isArray(result.data.data)) {
            coursesWithUsers = result.data.data;
          } else if (!Array.isArray(result.data)) {
            // Check if it's a direct array
            coursesWithUsers = [];
          }
          
          // Debug: log the structure to understand the data
          console.log('Courses with users structure:', coursesWithUsers);
          
          // Flatten users from all courses into a single array
          // Remove duplicates by userId
          const userMap = new Map();
          
          if (Array.isArray(coursesWithUsers)) {
            coursesWithUsers.forEach(course => {
              // Handle both camelCase and PascalCase property names
              const users = course.users || course.Users || [];
              
              if (Array.isArray(users) && users.length > 0) {
                users.forEach(user => {
                  const userId = user.userId || user.UserId;
                  if (userId) {
                    if (!userMap.has(userId)) {
                      // First time seeing this user
                      userMap.set(userId, {
                        ...user,
                        // Store course info
                        enrolledCourses: [course.title || course.Title || course.courseId || '']
                      });
                    } else {
                      // User already exists, add course to enrolled courses
                      const existingUser = userMap.get(userId);
                      const courseTitle = course.title || course.Title || course.courseId || '';
                      if (!existingUser.enrolledCourses.includes(courseTitle)) {
                        existingUser.enrolledCourses.push(courseTitle);
                      }
                    }
                  }
                });
              }
            });
          }
          
          // Convert map to array
          const allStudents = Array.from(userMap.values());
          console.log('Flattened students:', allStudents);
          setStudents(allStudents);
        } else {
          const errorMsg = result.error || result.data?.message || 'Không thể tải danh sách học sinh';
          console.error('Error loading students:', errorMsg, result);
          setError(errorMsg);
          setStudents([]);
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
            {activeTab === 'students' && <th>Khóa học đã đăng ký</th>}
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {data.map((user) => {
            const userId = user.userId || user.UserId;
            const firstName = user.firstName || user.FirstName || '';
            const lastName = user.lastName || user.LastName || '';
            const email = user.email || user.Email || '';
            const phoneNumber = user.phoneNumber || user.PhoneNumber || '-';
            const status = user.status || user.Status || 'Active';
            
            return (
              <tr key={userId}>
                <td>{userId}</td>
                <td>{firstName} {lastName}</td>
                <td>{email}</td>
                <td>{phoneNumber || '-'}</td>
                {activeTab === 'students' && (
                  <td>
                    {user.enrolledCourses && user.enrolledCourses.length > 0 
                      ? user.enrolledCourses.join(', ') 
                      : '-'}
                  </td>
                )}
                <td>
                  <span className={`status-badge ${status === 'Active' ? 'active' : 'blocked'}`}>
                    {status === 'Active' ? 'Hoạt động' : 'Bị khóa'}
                  </span>
                </td>
                <td>
                  {status === 'Active' ? (
                    <button 
                      className="btn-danger"
                      onClick={() => handleBlock(userId)}
                    >
                      Khóa
                    </button>
                  ) : (
                    <button 
                      className="btn-success"
                      onClick={() => handleUnblock(userId)}
                    >
                      Mở khóa
                    </button>
                  )}
                </td>
              </tr>
            );
          })}
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

