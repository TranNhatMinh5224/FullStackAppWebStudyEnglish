import React, { useState, useEffect } from 'react';
import { AdminService } from '../../../services/api/admin/adminService';
import './AdminDashboard.css';

const TeacherPackageManagement = () => {
  const [packages, setPackages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingPackage, setEditingPackage] = useState(null);
  const [formData, setFormData] = useState({
    packageName: '',
    description: '',
    price: 0,
    durationDays: 30,
    maxCourses: 5
  });

  useEffect(() => {
    loadPackages();
  }, []);

  const loadPackages = async () => {
    setLoading(true);
    setError('');
    try {
      const result = await AdminService.getAllTeacherPackages();
      if (result.success) {
        setPackages(result.data || []);
      } else {
        setError(result.error || 'Không thể tải danh sách gói giáo viên');
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
      const result = await AdminService.createTeacherPackage(formData);
      if (result.success) {
        alert('Tạo gói giáo viên thành công');
        setShowCreateForm(false);
        setFormData({ packageName: '', description: '', price: 0, durationDays: 30, maxCourses: 5 });
        loadPackages();
      } else {
        alert(result.error || 'Không thể tạo gói giáo viên');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    try {
      const result = await AdminService.updateTeacherPackage(editingPackage.teacherPackageId, formData);
      if (result.success) {
        alert('Cập nhật gói giáo viên thành công');
        setEditingPackage(null);
        setFormData({ packageName: '', description: '', price: 0, durationDays: 30, maxCourses: 5 });
        loadPackages();
      } else {
        alert(result.error || 'Không thể cập nhật gói giáo viên');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa gói giáo viên này?')) return;
    
    try {
      const result = await AdminService.deleteTeacherPackage(id);
      if (result.success) {
        alert('Xóa gói giáo viên thành công');
        loadPackages();
      } else {
        alert(result.error || 'Không thể xóa gói giáo viên');
      }
    } catch (err) {
      alert('Có lỗi xảy ra');
      console.error(err);
    }
  };

  const startEdit = (pkg) => {
    setEditingPackage(pkg);
    setFormData({
      packageName: pkg.packageName || '',
      description: pkg.description || '',
      price: pkg.price || 0,
      durationDays: pkg.durationDays || 30,
      maxCourses: pkg.maxCourses || 5
    });
    setShowCreateForm(true);
  };

  const cancelForm = () => {
    setShowCreateForm(false);
    setEditingPackage(null);
    setFormData({ packageName: '', description: '', price: 0, durationDays: 30, maxCourses: 5 });
  };

  return (
    <div className="admin-section">
      <div className="section-header">
        <h2>Quản lý gói giáo viên</h2>
        <button 
          className="btn-primary"
          onClick={() => {
            setShowCreateForm(true);
            setEditingPackage(null);
            setFormData({ packageName: '', description: '', price: 0, durationDays: 30, maxCourses: 5 });
          }}
        >
          + Tạo gói mới
        </button>
      </div>

      {showCreateForm && (
        <div className="form-modal">
          <div className="form-content">
            <h3>{editingPackage ? 'Cập nhật gói giáo viên' : 'Tạo gói giáo viên mới'}</h3>
            <form onSubmit={editingPackage ? handleUpdate : handleCreate}>
              <div className="form-group">
                <label>Tên gói *</label>
                <input
                  type="text"
                  value={formData.packageName}
                  onChange={(e) => setFormData({ ...formData, packageName: e.target.value })}
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
                <label>Giá (VNĐ)</label>
                <input
                  type="number"
                  value={formData.price}
                  onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) || 0 })}
                  min="0"
                />
              </div>
              <div className="form-group">
                <label>Thời hạn (ngày)</label>
                <input
                  type="number"
                  value={formData.durationDays}
                  onChange={(e) => setFormData({ ...formData, durationDays: parseInt(e.target.value) || 30 })}
                  min="1"
                />
              </div>
              <div className="form-group">
                <label>Số khóa học tối đa</label>
                <input
                  type="number"
                  value={formData.maxCourses}
                  onChange={(e) => setFormData({ ...formData, maxCourses: parseInt(e.target.value) || 5 })}
                  min="1"
                />
              </div>
              <div className="form-actions">
                <button type="submit" className="btn-primary">
                  {editingPackage ? 'Cập nhật' : 'Tạo mới'}
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
              <th>Tên gói</th>
              <th>Mô tả</th>
              <th>Giá</th>
              <th>Thời hạn</th>
              <th>Số khóa học tối đa</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {packages.length === 0 ? (
              <tr>
                <td colSpan="7" className="no-data">Không có gói giáo viên nào</td>
              </tr>
            ) : (
              packages.map((pkg) => (
                <tr key={pkg.teacherPackageId}>
                  <td>{pkg.teacherPackageId}</td>
                  <td>{pkg.packageName}</td>
                  <td>{pkg.description || '-'}</td>
                  <td>{pkg.price ? `${pkg.price.toLocaleString()} VNĐ` : 'Miễn phí'}</td>
                  <td>{pkg.durationDays} ngày</td>
                  <td>{pkg.maxCourses}</td>
                  <td>
                    <button 
                      className="btn-edit"
                      onClick={() => startEdit(pkg)}
                    >
                      Sửa
                    </button>
                    <button 
                      className="btn-danger"
                      onClick={() => handleDelete(pkg.teacherPackageId)}
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

export default TeacherPackageManagement;

