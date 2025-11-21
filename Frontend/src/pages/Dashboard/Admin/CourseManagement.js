import React, { useState, useEffect } from 'react';
import { AdminService } from '../../../services/api/admin/adminService';
import './AdminDashboard.css';

const CourseManagement = () => {
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingCourse, setEditingCourse] = useState(null);
  const [formData, setFormData] = useState({
    courseName: '',
    description: '',
    price: 0,
    isPublic: true
  });

  useEffect(() => {
    loadCourses();
  }, []);

  const loadCourses = async () => {
    setLoading(true);
    setError('');
    try {
      const result = await AdminService.getAllCourses();
      if (result.success) {
        setCourses(result.data || []);
      } else {
        setError(result.error || 'Không thể tải danh sách khóa học');
      }
    } catch (err) {
      setError('Có lỗi xảy ra khi tải dữ liệu');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      const result = await AdminService.createCourse(formData);
      if (result.success) {
        alert('Tạo khóa học thành công');
        setShowCreateForm(false);
        setFormData({ courseName: '', description: '', price: 0, isPublic: true });
        loadCourses();
      } else {
        alert(result.error || 'Không thể tạo khóa học');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    try {
      const result = await AdminService.updateCourse(editingCourse.courseId, formData);
      if (result.success) {
        alert('Cập nhật khóa học thành công');
        setEditingCourse(null);
        setFormData({ courseName: '', description: '', price: 0, isPublic: true });
        loadCourses();
      } else {
        alert(result.error || 'Không thể cập nhật khóa học');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const handleDelete = async (courseId) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa khóa học này?')) return;
    
    try {
      const result = await AdminService.deleteCourse(courseId);
      if (result.success) {
        alert('Xóa khóa học thành công');
        loadCourses();
      } else {
        alert(result.error || 'Không thể xóa khóa học');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const startEdit = (course) => {
    setEditingCourse(course);
    setFormData({
      courseName: course.courseName || '',
      description: course.description || '',
      price: course.price || 0,
      isPublic: course.isPublic !== undefined ? course.isPublic : true
    });
    setShowCreateForm(true);
  };

  const cancelForm = () => {
    setShowCreateForm(false);
    setEditingCourse(null);
    setFormData({ courseName: '', description: '', price: 0, isPublic: true });
  };

  return (
    <div className="admin-section">
      <div className="section-header">
        <h2>Quản lý khóa học</h2>
        <button 
          className="btn-primary"
          onClick={() => {
            setShowCreateForm(true);
            setEditingCourse(null);
            setFormData({ courseName: '', description: '', price: 0, isPublic: true });
          }}
        >
          + Tạo khóa học mới
        </button>
      </div>

      {showCreateForm && (
        <div className="form-modal">
          <div className="form-content">
            <h3>{editingCourse ? 'Cập nhật khóa học' : 'Tạo khóa học mới'}</h3>
            <form onSubmit={editingCourse ? handleUpdate : handleCreate}>
              <div className="form-group">
                <label>Tên khóa học *</label>
                <input
                  type="text"
                  value={formData.courseName}
                  onChange={(e) => setFormData({ ...formData, courseName: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Mô tả</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows="4"
                />
              </div>
              <div className="form-group">
                <label>Giá</label>
                <input
                  type="number"
                  value={formData.price}
                  onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) || 0 })}
                  min="0"
                />
              </div>
              <div className="form-group">
                <label>
                  <input
                    type="checkbox"
                    checked={formData.isPublic}
                    onChange={(e) => setFormData({ ...formData, isPublic: e.target.checked })}
                  />
                  Công khai
                </label>
              </div>
              <div className="form-actions">
                <button type="submit" className="btn-primary">
                  {editingCourse ? 'Cập nhật' : 'Tạo mới'}
                </button>
                <button type="button" className="btn-secondary" onClick={cancelForm}>
                  Hủy
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {error && <div className="error-message">{error}</div>}
      
      {loading ? (
        <div className="loading">Đang tải...</div>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Tên khóa học</th>
              <th>Mô tả</th>
              <th>Giá</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {courses.length === 0 ? (
              <tr>
                <td colSpan="6" className="no-data">Không có khóa học nào</td>
              </tr>
            ) : (
              courses.map((course) => (
                <tr key={course.courseId}>
                  <td>{course.courseId}</td>
                  <td>{course.courseName}</td>
                  <td>{course.description || '-'}</td>
                  <td>{course.price ? `${course.price.toLocaleString()} VNĐ` : 'Miễn phí'}</td>
                  <td>
                    <span className={`status-badge ${course.isPublic ? 'active' : 'inactive'}`}>
                      {course.isPublic ? 'Công khai' : 'Riêng tư'}
                    </span>
                  </td>
                  <td>
                    <button 
                      className="btn-edit"
                      onClick={() => startEdit(course)}
                    >
                      Sửa
                    </button>
                    <button 
                      className="btn-danger"
                      onClick={() => handleDelete(course.courseId)}
                    >
                      Xóa
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default CourseManagement;

