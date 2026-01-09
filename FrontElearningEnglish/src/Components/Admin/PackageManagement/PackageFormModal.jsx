import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import { teacherPackageService } from "../../../Services/teacherPackageService";
import { toast } from "react-toastify";

export default function PackageFormModal({ show, onClose, onSuccess, packageToEdit }) {
    const [formData, setFormData] = useState({
        packageName: "",
        level: "1",  // Basic = 1 in backend enum
        price: "0",
        maxCourses: "5",
        maxLessons: "50",
        maxStudents: "100"
    });
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});

    useEffect(() => {
        if (packageToEdit) {
            setFormData({
                packageName: packageToEdit.packageName || packageToEdit.PackageName || "",
                level: String(packageToEdit.level || packageToEdit.Level || 1),  // Basic = 1
                price: String(packageToEdit.price || packageToEdit.Price || 0),
                maxCourses: String(packageToEdit.maxCourses || packageToEdit.MaxCourses || 5),
                maxLessons: String(packageToEdit.maxLessons || packageToEdit.MaxLessons || 50),
                maxStudents: String(packageToEdit.maxStudents || packageToEdit.MaxStudents || 100)
            });
        } else {
            setFormData({
                packageName: "",
                level: "1", // Basic = 1
                price: "0",
                maxCourses: "5",
                maxLessons: "50",
                maxStudents: "100"
            });
        }
        setErrors({});
    }, [packageToEdit, show]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
            ...formData,
            [name]: value
        });
        validateField(name, value);
    };

    const validateField = (fieldName, value) => {
        const newErrors = { ...errors };

        if (fieldName === 'price') {
            const priceValue = parseFloat(value);
            if (isNaN(priceValue) || priceValue < 0) {
                newErrors.price = "Giá không được là số âm";
            } else {
                newErrors.price = null;
            }
        } else if (fieldName === 'maxCourses') {
            const maxCoursesValue = parseInt(value);
            if (isNaN(maxCoursesValue) || maxCoursesValue < 1) {
                newErrors.maxCourses = "Số khóa học tối đa phải lớn hơn 0";
            } else {
                newErrors.maxCourses = null;
            }
        } else if (fieldName === 'maxLessons') {
            const maxLessonsValue = parseInt(value);
            if (isNaN(maxLessonsValue) || maxLessonsValue < 1) {
                newErrors.maxLessons = "Số bài học tối đa phải lớn hơn 0";
            } else {
                newErrors.maxLessons = null;
            }
        } else if (fieldName === 'maxStudents') {
            const maxStudentsValue = parseInt(value);
            if (isNaN(maxStudentsValue) || maxStudentsValue < 1) {
                newErrors.maxStudents = "Số học viên tối đa phải lớn hơn 0";
            } else {
                newErrors.maxStudents = null;
            }
        } else if (fieldName === 'packageName') {
            if (!value.trim()) {
                newErrors.packageName = "Tên gói là bắt buộc";
            } else {
                newErrors.packageName = null;
            }
        }

        setErrors(newErrors);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        // Validate all fields
        const validationErrors = {};
        if (!formData.packageName.trim()) {
            validationErrors.packageName = "Tên gói là bắt buộc";
        }
        const priceValue = parseFloat(formData.price);
        if (isNaN(priceValue) || priceValue < 0) {
            validationErrors.price = "Giá không được là số âm";
        }
        const maxCoursesValue = parseInt(formData.maxCourses);
        if (isNaN(maxCoursesValue) || maxCoursesValue < 1) {
            validationErrors.maxCourses = "Số khóa học tối đa phải lớn hơn 0";
        }
        const maxLessonsValue = parseInt(formData.maxLessons);
        if (isNaN(maxLessonsValue) || maxLessonsValue < 1) {
            validationErrors.maxLessons = "Số bài học tối đa phải lớn hơn 0";
        }
        const maxStudentsValue = parseInt(formData.maxStudents);
        if (isNaN(maxStudentsValue) || maxStudentsValue < 1) {
            validationErrors.maxStudents = "Số học viên tối đa phải lớn hơn 0";
        }
        
        setErrors(validationErrors);
        if (Object.keys(validationErrors).length > 0) {
            return;
        }

        setLoading(true);
        try {
            // Prepare data matching backend DTO exactly
            const dataToSend = {
                packageName: formData.packageName,
                level: Number(formData.level),
                price: Number(formData.price),
                maxCourses: Number(formData.maxCourses),
                maxLessons: Number(formData.maxLessons),
                maxStudents: Number(formData.maxStudents)
            };

            console.log("Data being sent to API:", dataToSend);

            let response;
            if (packageToEdit) {
                const id = packageToEdit.teacherPackageId || packageToEdit.TeacherPackageId;
                response = await teacherPackageService.update(id, dataToSend);
            } else {
                response = await teacherPackageService.create(dataToSend);
            }

            console.log("API Response:", response);

            if (response.data?.success) {
                const message = packageToEdit ? "Cập nhật gói thành công!" : "Tạo gói mới thành công!";
                onClose();
                onSuccess(message);
            } else {
                toast.error(response.data?.message || "Có lỗi xảy ra.");
            }
        } catch (error) {
            console.error("Error saving package:", error);
            console.error("Error details:", error.response?.data);
            toast.error(error.response?.data?.message || error.response?.data?.title || "Lỗi kết nối.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal 
            show={show} 
            onHide={onClose}
            centered
            size="xl"
            backdrop="static"
            className="create-course-modal"
            dialogClassName="create-course-modal-dialog"
            style={{ zIndex: 1050 }}
        >
            <Modal.Header className="p-0 border-0">
              <div className="w-100 position-relative">
                <div className="form-top-banner text-white mb-0">
                  <h4 className="mb-1 fw-bold">{packageToEdit ? 'Cập nhật gói' : 'Tạo gói mới'}</h4>
                  
                </div>
                <button type="button" className="btn-close" aria-label="Close" onClick={onClose} />
              </div>
            </Modal.Header>
            <Modal.Body>
              <form onSubmit={handleSubmit} className="p-0">

                {/* Basic Information Card */}
                <div className="card mb-4 shadow-sm">
                  <div className="card-body">
                    <h6 className="card-title fw-semibold mb-3">Thông tin cơ bản</h6>
                    <div className="row g-3">
                      <div className="col-md-6">
                        <label className="form-label">Tên gói <span className="text-danger">*</span></label>
                        <input
                          type="text"
                          className={`form-control ${errors.packageName ? 'is-invalid' : ''}`}
                          name="packageName"
                          value={formData.packageName}
                          onChange={handleChange}
                          placeholder="VD: Basic Plan"
                        />
                        {errors.packageName && <div className="invalid-feedback d-block">{errors.packageName}</div>}
                      </div>
                      <div className="col-md-6">
                        <label className="form-label">Cấp độ (Level)</label>
                        <select 
                          className="form-select" 
                          name="level"
                          value={formData.level} 
                          onChange={handleChange}
                        >
                          <option value={1}>Basic</option>
                          <option value={2}>Standard</option>
                          <option value={3}>Premium</option>
                          <option value={4}>Professional</option>
                        </select>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Settings Card */}
                <div className="card mb-4 shadow-sm">
                  <div className="card-body">
                    <h6 className="card-title fw-semibold mb-3">Thiết lập gói</h6>
                    <div className="row g-3">
                      <div className="col-md-12">
                        <label className="form-label">Giá (VND) <span className="text-danger">*</span></label>
                        <input 
                          type="number" 
                          className={`form-control ${errors.price ? 'is-invalid' : ''}`}
                          name="price"
                          value={formData.price} 
                          onChange={handleChange}
                          placeholder="0"
                          min="0"
                        />
                        {errors.price && <div className="invalid-feedback d-block">{errors.price}</div>}
                        <div className="form-text mt-1">0 = Miễn phí</div>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Resource Limits Card */}
                <div className="card mb-4 shadow-sm">
                  <div className="card-body">
                    <h6 className="card-title fw-semibold mb-3">Giới hạn tài nguyên</h6>
                    <div className="row g-3">
                      <div className="col-md-4">
                        <label className="form-label">Max Khóa học</label>
                        <input 
                          type="number" 
                          className={`form-control ${errors.maxCourses ? 'is-invalid' : ''}`}
                          name="maxCourses"
                          value={formData.maxCourses} 
                          onChange={handleChange}
                          placeholder="5"
                          min="1"
                        />
                        {errors.maxCourses && <div className="invalid-feedback d-block">{errors.maxCourses}</div>}
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">Max Bài học</label>
                        <input 
                          type="number" 
                          className={`form-control ${errors.maxLessons ? 'is-invalid' : ''}`}
                          name="maxLessons"
                          value={formData.maxLessons} 
                          onChange={handleChange}
                          placeholder="50"
                          min="1"
                        />
                        {errors.maxLessons && <div className="invalid-feedback d-block">{errors.maxLessons}</div>}
                      </div>
                      <div className="col-md-4">
                        <label className="form-label">Max Học viên</label>
                        <input 
                          type="number" 
                          className={`form-control ${errors.maxStudents ? 'is-invalid' : ''}`}
                          name="maxStudents"
                          value={formData.maxStudents} 
                          onChange={handleChange}
                          placeholder="100"
                          min="1"
                        />
                        {errors.maxStudents && <div className="invalid-feedback d-block">{errors.maxStudents}</div>}
                      </div>
                    </div>
                  </div>
                </div>

                {/* Submit error */}
                {errors.submit && (<div className="alert alert-danger mt-3">{errors.submit}</div>)}
              </form>
            </Modal.Body>
            <Modal.Footer>
              <Button variant="outline-secondary" onClick={onClose} disabled={loading}>
                Hủy
              </Button>
              <Button
                variant="primary"
                className="btn-primary"
                onClick={handleSubmit}
                disabled={loading}
              >
                {loading ? (packageToEdit ? "Đang cập nhật..." : "Đang tạo...") : (packageToEdit ? "Lưu & Cập nhật" : "Tạo gói")}
              </Button>
            </Modal.Footer>
        </Modal>
    );
}
