import React, { useState, useEffect, useCallback } from "react";
import { Modal, Button, Form } from "react-bootstrap";
import { FaLayerGroup, FaImage } from "react-icons/fa";
import { teacherService } from "../../../Services/teacherService";
import { adminService } from "../../../Services/adminService";
import FileUpload from "../../Common/FileUpload/FileUpload";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import "./CreateModuleModal.css";

const MODULE_IMAGE_BUCKET = "modules";

// ModuleType enum mapping (matching backend ModuleType enum)
// Lecture = 1, FlashCard = 2, Assessment = 3
const MODULE_TYPES = [
  { value: 1, label: "Lecture" },
  { value: 2, label: "FlashCard" },
  { value: 3, label: "Assessment" },
];

export default function CreateModuleModal({ show, onClose, onSuccess, lessonId, moduleData, isUpdateMode = false, isAdmin = false }) {
  // Form state
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [contentType, setContentType] = useState("");
  
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
    if (show && isUpdateMode && moduleData) {
      const moduleName = moduleData.name || moduleData.Name || "";
      const moduleDescription = moduleData.description || moduleData.Description || "";
      const moduleContentType = moduleData.contentType || moduleData.ContentType || "";
      const moduleImageUrl = moduleData.imageUrl || moduleData.ImageUrl || null;
      
      setName(moduleName);
      setDescription(moduleDescription || "");
      setContentType(moduleContentType.toString());
      setExistingImageUrl(moduleImageUrl);
      setImageTempKey(null);
      setImageType(null);
    }
  }, [show, isUpdateMode, moduleData]);

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setName("");
      setDescription("");
      setContentType("");
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
      name.trim() ||
      description.trim() ||
      contentType ||
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

    // Name validation: NotEmpty, MaxLength(200)
    if (!name.trim()) {
      newErrors.name = "Tên module không được để trống";
    } else if (name.trim().length > 200) {
      newErrors.name = "Tên module không được vượt quá 200 ký tự";
    }

    // Description validation: Optional, MaxLength(200) - Khớp với DB
    if (description.trim().length > 200) {
      newErrors.description = "Mô tả module không được vượt quá 200 ký tự";
    }

    // ContentType validation: IsInEnum (required for create, optional for update)
    if (!isUpdateMode && !contentType) {
      newErrors.contentType = "Loại nội dung là bắt buộc";
    } else if (contentType) {
      const contentTypeValue = parseInt(contentType);
      if (contentTypeValue !== 1 && contentTypeValue !== 2 && contentTypeValue !== 3) {
        newErrors.contentType = "Loại nội dung không hợp lệ";
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Real-time validation for individual fields
  const validateField = (fieldName, value) => {
    const newErrors = { ...errors };
    
    switch (fieldName) {
      case "name":
        if (!value.trim()) {
          newErrors.name = "Tên module không được để trống";
        } else if (value.trim().length > 200) {
          newErrors.name = "Tên module không được vượt quá 200 ký tự";
        } else {
          delete newErrors.name;
        }
        break;
      case "description":
        if (value.trim().length > 200) {
          newErrors.description = "Mô tả module không được vượt quá 200 ký tự";
        } else {
          delete newErrors.description;
        }
        break;
      case "contentType":
        if (!isUpdateMode && !value) {
          newErrors.contentType = "Loại nội dung là bắt buộc";
        } else if (value) {
          const contentTypeValue = parseInt(value);
          if (contentTypeValue !== 1 && contentTypeValue !== 2 && contentTypeValue !== 3) {
            newErrors.contentType = "Loại nội dung không hợp lệ";
          } else {
            delete newErrors.contentType;
          }
        } else {
          delete newErrors.contentType;
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
      let response;
      
      if (isUpdateMode && moduleData) {
        const moduleId = moduleData.moduleId || moduleData.ModuleId;
        const updateData = {
          name: name.trim(),
        };

        // Description is optional for update
        if (description.trim()) {
          updateData.description = description.trim();
        }

        // ContentType is optional for update
        if (contentType) {
          updateData.contentType = parseInt(contentType);
        }

        // Chỉ thêm imageTempKey và imageType nếu có upload ảnh mới
        if (imageTempKey && imageType) {
          updateData.imageTempKey = imageTempKey;
          updateData.imageType = imageType;
        }

        response = isAdmin
          ? await adminService.updateModule(moduleId, updateData)
          : await teacherService.updateModule(moduleId, updateData);
      } else {
        const createData = {
          lessonId: parseInt(lessonId),
          name: name.trim(),
          contentType: parseInt(contentType),
          orderIndex: 0, // Backend will auto-assign if 0
        };

        // Description is optional
        if (description.trim()) {
          createData.description = description.trim();
        }

        // Chỉ thêm imageTempKey và imageType nếu có upload ảnh
        if (imageTempKey && imageType) {
          createData.imageTempKey = imageTempKey;
          createData.imageType = imageType;
        }

        response = isAdmin
          ? await adminService.createModule(createData)
          : await teacherService.createModule(createData);
      }

      if (response.data?.success) {
        onSuccess?.();
        onClose();
      } else {
        throw new Error(response.data?.message || (isUpdateMode ? "Cập nhật module thất bại" : "Tạo module thất bại"));
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} module:`, error);
      
      // Xử lý validation errors từ backend
      const backendErrors = {};
      let submitError = "";
      
      if (error.response?.data?.errors) {
        const validationErrors = error.response.data.errors;
        if (validationErrors.Name) {
          backendErrors.name = Array.isArray(validationErrors.Name) 
            ? validationErrors.Name[0] 
            : validationErrors.Name;
        }
        if (validationErrors.Description) {
          backendErrors.description = Array.isArray(validationErrors.Description) 
            ? validationErrors.Description[0] 
            : validationErrors.Description;
        }
        if (validationErrors.ContentType) {
          backendErrors.contentType = Array.isArray(validationErrors.ContentType) 
            ? validationErrors.ContentType[0] 
            : validationErrors.ContentType;
        }
      }
      
      if (error.response?.data?.message) {
        submitError = error.response.data.message;
      } else if (error.message) {
        submitError = error.message;
      } else {
        submitError = `Có lỗi xảy ra khi ${isUpdateMode ? "cập nhật" : "tạo"} module`;
      }
      
      setErrors({ ...backendErrors, submit: submitError });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = name.trim() && (!isUpdateMode ? contentType : true);

  return (
    <>
      <Modal show={show} onHide={onClose} centered size="xl" backdrop="static" className="create-module-modal modal-modern" dialogClassName="create-module-modal-dialog">
        <Modal.Header>
          <Modal.Title className="modal-title-custom">
            {isUpdateMode ? "Cập nhật Module" : "Tạo Module mới"}
          </Modal.Title>
        </Modal.Header>

        <Modal.Body>
          <Form onSubmit={handleSubmit}>
            {/* SECTION 1: CƠ BẢN */}
            <div className="form-section">
              <div className="section-title"><FaLayerGroup /> Thông tin chung</div>
              <div className="row g-3">
                <div className="col-12">
                  <Form.Label className="fw-bold">Tên module <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    isInvalid={!!errors.name}
                    value={name}
                    onChange={(e) => {
                      setName(e.target.value);
                      validateField("name", e.target.value);
                    }}
                    placeholder="Nhập tên module..."
                    maxLength={200}
                  />
                  <div className="d-flex justify-content-between align-items-center mt-2">
                    {errors.name && (
                      <Form.Control.Feedback type="invalid" className="d-block mb-0">
                        {errors.name}
                      </Form.Control.Feedback>
                    )}
                    <div className={`char-count ms-auto ${name.length > 180 ? 'text-warning' : name.length > 195 ? 'text-danger' : ''}`}>
                      {name.length.toLocaleString('vi-VN')} / 200 ký tự
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
                    placeholder="Nhập mô tả module (tùy chọn)..."
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

                <div className="col-12">
                  <Form.Label className="fw-bold">
                    Loại nội dung {!isUpdateMode && <span className="text-danger">*</span>}
                  </Form.Label>
                  <Form.Select
                    value={contentType}
                    onChange={(e) => {
                      setContentType(e.target.value);
                      validateField("contentType", e.target.value);
                    }}
                    isInvalid={!!errors.contentType}
                  >
                    <option value="">Chọn loại nội dung</option>
                    {MODULE_TYPES.map((type) => (
                      <option key={type.value} value={type.value}>
                        {type.label}
                      </option>
                    ))}
                  </Form.Select>
                  {errors.contentType && (
                    <Form.Control.Feedback type="invalid" className="d-block mt-1">
                      {errors.contentType}
                    </Form.Control.Feedback>
                  )}
                </div>
              </div>
            </div>

            {/* SECTION 2: ẢNH */}
            <div className="form-section">
              <div className="section-title"><FaImage /> Hình ảnh đại diện</div>
              <FileUpload
                bucket={MODULE_IMAGE_BUCKET}
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
            {submitting ? "Đang lưu..." : (isUpdateMode ? "Cập nhật Module" : "Tạo Module")}
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
