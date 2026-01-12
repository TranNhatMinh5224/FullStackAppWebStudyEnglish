import React, { useState, useEffect, useCallback } from "react";
import { Modal, Button, Form } from "react-bootstrap";
import { FaLayerGroup, FaImage } from "react-icons/fa";
import { teacherService } from "../../../Services/teacherService";
import { adminService } from "../../../Services/adminService";
import FileUpload from "../../Common/FileUpload/FileUpload";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import "./CreateLessonModal.css";

const LESSON_IMAGE_BUCKET = "lessons";

export default function CreateLessonModal({ show, onClose, onSuccess, courseId, lessonData, isUpdateMode = false, isAdmin = false }) {
  // Form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  
  // Image upload state
  const [imageTempKey, setImageTempKey] = useState(null);
  const [imageType, setImageType] = useState(null);
  const [existingImageUrl, setExistingImageUrl] = useState(null);
  
  // Validation errors
  const [errors, setErrors] = useState({});
  
  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [showConfirmCancel, setShowConfirmCancel] = useState(false);

  // Pre-fill form when in update mode
  useEffect(() => {
    if (show && isUpdateMode && lessonData) {
      const lessonTitle = lessonData.title || lessonData.Title || "";
      const lessonDescription = lessonData.description || lessonData.Description || "";
      const lessonImageUrl = lessonData.imageUrl || lessonData.ImageUrl || null;
      
      setTitle(lessonTitle);
      setDescription(lessonDescription || "");
      setExistingImageUrl(lessonImageUrl);
      setImageTempKey(null);
      setImageType(null);
    }
  }, [show, isUpdateMode, lessonData]);

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setTitle("");
      setDescription("");
      setImageTempKey(null);
      setImageType(null);
      setExistingImageUrl(null);
      setErrors({});
      setSubmitting(false);
      setShowConfirmCancel(false);
    }
  }, [show]);

  // --- LOGIC: KIỂM TRA FORM CÓ NỘI DUNG ---
  const hasFormContent = () => {
    return !!(
      title.trim() ||
      description.trim() ||
      imageTempKey ||
      existingImageUrl
    );
  };

  // --- LOGIC: XỬ LÝ HỦY BỎ VỚI XÁC NHẬN ---
  const handleCancel = () => {
    if (hasFormContent()) {
      setShowConfirmCancel(true);
    } else {
      onClose();
    }
  };

  const handleConfirmCancel = () => {
    setShowConfirmCancel(false);
    onClose();
  };

  // --- LOGIC: HANDLE IMAGE UPLOAD SUCCESS ---
  const handleImageUploadSuccess = useCallback((tempKey, fileType, previewUrl) => {
    setImageTempKey(tempKey);
    setImageType(fileType);
    setErrors(prev => ({ ...prev, image: null }));
  }, []);

  // --- LOGIC: HANDLE IMAGE REMOVE ---
  const handleImageRemove = useCallback(() => {
    setImageTempKey(null);
    setImageType(null);
    setExistingImageUrl(null);
    setErrors(prev => ({ ...prev, image: null }));
  }, []);

  // --- LOGIC: HANDLE IMAGE UPLOAD ERROR ---
  const handleImageUploadError = useCallback((errorMessage) => {
    setErrors(prev => ({ ...prev, image: errorMessage }));
  }, []);

  // --- LOGIC: VALIDATION - Khớp với Backend Validator ---
  const validateForm = () => {
    const newErrors = {};

    // Title validation: NotEmpty, MaxLength(200)
    if (!title.trim()) {
      newErrors.title = "Tiêu đề bài học là bắt buộc";
    } else if (title.trim().length > 200) {
      newErrors.title = "Tiêu đề bài học không được vượt quá 200 ký tự";
    }

    // Description validation: Optional, MaxLength(200) - Khớp với DB
    if (description.trim().length > 200) {
      newErrors.description = "Mô tả bài học không được vượt quá 200 ký tự";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Real-time validation for individual fields
  const validateField = (fieldName, value) => {
    const newErrors = { ...errors };
    
    switch (fieldName) {
      case "title":
        if (!value.trim()) {
          newErrors.title = "Tiêu đề bài học là bắt buộc";
        } else if (value.trim().length > 200) {
          newErrors.title = "Tiêu đề bài học không được vượt quá 200 ký tự";
        } else {
          delete newErrors.title;
        }
        break;
      case "description":
        if (value.trim().length > 200) {
          newErrors.description = "Mô tả bài học không được vượt quá 200 ký tự";
        } else {
          delete newErrors.description;
        }
        break;
      default:
        break;
    }
    
    setErrors(newErrors);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setSubmitting(true);

    try {
      const submitData = {
        title: title.trim(),
      };

      // Description is optional
      if (description.trim()) {
        submitData.description = description.trim();
      }

      // Chỉ thêm imageTempKey và imageType nếu có upload ảnh mới
      if (imageTempKey && imageType) {
        submitData.imageTempKey = imageTempKey;
        submitData.imageType = imageType;
      }

      let response;
      if (isUpdateMode && lessonData) {
        const lessonId = lessonData.lessonId || lessonData.LessonId;
        if (!lessonId) {
          throw new Error("Không tìm thấy ID bài học");
        }
        response = isAdmin 
          ? await adminService.updateLesson(lessonId, submitData)
          : await teacherService.updateLesson(lessonId, submitData);
      } else {
        submitData.courseId = parseInt(courseId);
        response = isAdmin
          ? await adminService.createLesson(submitData)
          : await teacherService.createLesson(submitData);
      }

      if (response.data?.success) {
        onSuccess?.();
        onClose();
      } else {
        throw new Error(response.data?.message || (isUpdateMode ? "Cập nhật bài học thất bại" : "Tạo bài học thất bại"));
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} lesson:`, error);
      
      // Xử lý validation errors từ backend
      const backendErrors = {};
      let submitError = "";
      
      if (error.response?.data?.errors) {
        const validationErrors = error.response.data.errors;
        if (validationErrors.Title) {
          backendErrors.title = Array.isArray(validationErrors.Title) 
            ? validationErrors.Title[0] 
            : validationErrors.Title;
        }
        if (validationErrors.Description) {
          backendErrors.description = Array.isArray(validationErrors.Description) 
            ? validationErrors.Description[0] 
            : validationErrors.Description;
        }
      }
      
      if (error.response?.data?.message) {
        submitError = error.response.data.message;
      } else if (error.message) {
        submitError = error.message;
      } else {
        submitError = isUpdateMode ? "Có lỗi xảy ra khi cập nhật bài học" : "Có lỗi xảy ra khi tạo bài học";
      }
      
      setErrors({ ...backendErrors, submit: submitError });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = title.trim();

  return (
    <>
      <Modal show={show} onHide={handleCancel} centered size="xl" backdrop="static" className="create-lesson-modal modal-modern" dialogClassName="create-lesson-modal-dialog">
        <Modal.Header>
          <Modal.Title className="modal-title-custom">
            {isUpdateMode ? "Cập nhật bài học" : "Tạo bài học mới"}
          </Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <Form onSubmit={handleSubmit}>
            {/* SECTION 1: CƠ BẢN */}
            <div className="form-section">
              <div className="section-title"><FaLayerGroup /> Thông tin chung</div>
              <div className="row g-3">
                <div className="col-12">
                  <Form.Label className="fw-bold">Tiêu đề bài học <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    isInvalid={!!errors.title}
                    value={title}
                    onChange={(e) => {
                      setTitle(e.target.value);
                      validateField("title", e.target.value);
                    }}
                    placeholder="Nhập tiêu đề bài học..."
                    maxLength={200}
                  />
                  <div className="d-flex justify-content-between align-items-center mt-2">
                    {errors.title && (
                      <Form.Control.Feedback type="invalid" className="d-block mb-0">
                        {errors.title}
                      </Form.Control.Feedback>
                    )}
                    <div className={`char-count ms-auto ${title.length > 180 ? 'text-warning' : title.length > 195 ? 'text-danger' : ''}`}>
                      {title.length.toLocaleString('vi-VN')} / 200 ký tự
                    </div>
                  </div>
                </div>

                <div className="col-12">
                  <Form.Label className="fw-bold">Mô tả</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={4}
                    isInvalid={!!errors.description}
                    value={description}
                    onChange={(e) => {
                      setDescription(e.target.value);
                      validateField("description", e.target.value);
                    }}
                    placeholder="Nhập mô tả bài học (tùy chọn)..."
                    maxLength={200}
                    style={{ resize: 'vertical', minHeight: '100px' }}
                  />
                  <div className="d-flex justify-content-between align-items-center mt-2">
                    {errors.description && (
                      <Form.Control.Feedback type="invalid" className="d-block mb-0">
                        {errors.description}
                      </Form.Control.Feedback>
                    )}
                    <div className={`char-count ms-auto ${description.length > 180 ? 'text-warning' : description.length > 195 ? 'text-danger' : ''}`}>
                      {description.length.toLocaleString('vi-VN')} / 200 ký tự
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* SECTION 2: ẢNH */}
            <div className="form-section">
              <div className="section-title"><FaImage /> Hình ảnh đại diện</div>
              <FileUpload
                bucket={LESSON_IMAGE_BUCKET}
                accept="image/*"
                maxSize={5}
                existingUrl={existingImageUrl}
                onUploadSuccess={handleImageUploadSuccess}
                onRemove={handleImageRemove}
                onError={handleImageUploadError}
                label="Chọn ảnh hoặc kéo thả vào đây"
                hint="Hỗ trợ Paste (Ctrl+V) từ Clipboard"
                enablePaste={true}
              />
            </div>

            {errors.submit && <div className="alert alert-danger mt-3">{errors.submit}</div>}
          </Form>
        </Modal.Body>

        <Modal.Footer>
          <Button variant="link" className="text-muted text-decoration-none fw-bold" onClick={handleCancel} disabled={submitting}>
            Hủy bỏ
          </Button>
          <Button className="btn-primary-custom" onClick={handleSubmit} disabled={submitting || !isFormValid}>
            {submitting ? "Đang lưu..." : (isUpdateMode ? "Cập nhật bài học" : "Tạo bài học")}
          </Button>
        </Modal.Footer>
      </Modal>

      <ConfirmModal
        isOpen={showConfirmCancel}
        onClose={() => setShowConfirmCancel(false)}
        onConfirm={handleConfirmCancel}
        title="Xác nhận hủy bỏ"
        message="Bạn có nội dung chưa lưu. Bạn có chắc chắn muốn hủy bỏ không?"
        confirmText="Hủy bỏ"
        cancelText="Tiếp tục chỉnh sửa"
        type="warning"
      />
    </>
  );
}
