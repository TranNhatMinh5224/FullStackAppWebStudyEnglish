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
      
      // httpClient wraps the response, so result.data is the ServiceResponse
      // Check if the ServiceResponse itself is successful
      if (result.success && result.data) {
        // If result.data has a success property, it's a ServiceResponse
        if (result.data.success === false || result.data.success === undefined) {
          setError(result.data.message || 'Không thể tải danh sách khóa học');
          setCourses([]);
          return;
        }
        
        // The actual array is in result.data.data
        const courseList = Array.isArray(result.data.data) ? result.data.data : 
                          (Array.isArray(result.data) ? result.data : []);
        
        // Map backend fields to frontend fields
        const mappedCourses = courseList.map(course => ({
          ...course,
          courseName: course.title || course.courseName || '',
          // CourseType.System = 1 (public), CourseType.Teacher = 2 (private)
          isPublic: course.type === 1 || course.type === 'System' || course.type === undefined
        }));
        setCourses(mappedCourses);
      } else {
        setError(result.error || result.data?.message || 'Không thể tải danh sách khóa học');
        setCourses([]);
      }
    } catch (err) {
      setError('Có lỗi xảy ra khi tải dữ liệu');
      console.error('Error loading courses:', err);
      setCourses([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      // Map frontend formData to backend DTO format
      const courseData = {
        title: formData.courseName,
        description: formData.description,
        price: formData.price || 0,
        maxStudent: 0, // Default value
        isFeatured: false, // Default value
        type: formData.isPublic ? 1 : 2 // 1 = System (public), 2 = Teacher (private)
      };
      
      const result = await AdminService.createCourse(courseData);
      if (result.success) {
        alert('Tạo khóa học thành công');
        setShowCreateForm(false);
        setFormData({ courseName: '', description: '', price: 0, isPublic: true });
        loadCourses();
      } else {
        const errorMsg = result.error || result.data?.message || 'Không thể tạo khóa học';
        alert(errorMsg);
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error('Error creating course:', err);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    try {
      // Map frontend formData to backend DTO format
      const courseData = {
        title: formData.courseName,
        description: formData.description,
        price: formData.price || 0,
        maxStudent: editingCourse?.maxStudent || 0,
        isFeatured: editingCourse?.isFeatured || false,
        type: formData.isPublic ? 1 : 2 // 1 = System (public), 2 = Teacher (private)
      };
      
      const result = await AdminService.updateCourse(editingCourse.courseId, courseData);
      if (result.success) {
        alert('Cập nhật khóa học thành công');
        setEditingCourse(null);
        setFormData({ courseName: '', description: '', price: 0, isPublic: true });
        loadCourses();
      } else {
        const errorMsg = result.error || result.data?.message || 'Không thể cập nhật khóa học';
        alert(errorMsg);
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error('Error updating course:', err);
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
      courseName: course.courseName || course.title || '',
      description: course.description || '',
      price: course.price || 0,
      // Map Type enum to isPublic boolean: 1/System = true, 2/Teacher = false
      isPublic: course.isPublic !== undefined 
        ? course.isPublic 
        : (course.type === 1 || course.type === 'System' || course.type === undefined)
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

