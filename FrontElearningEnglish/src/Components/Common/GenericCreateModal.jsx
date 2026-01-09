import React, { useState, useEffect } from "react";
import CreateWithImageModal from "../Common/CreateWithImageModal";
import { teacherService } from "../../Services/teacherService";
import { adminService } from "../../Services/adminService";

/**
 * Generic Create Modal Component
 * Reusable component for creating/updating entities with image upload
 *
 * @param {Object} config - Configuration object
 * @param {string} config.entityName - Name of the entity (e.g., 'lesson', 'module')
 * @param {string} config.bucketName - S3 bucket name for images
 * @param {Array} config.fields - Array of field configurations
 * @param {string} config.parentIdField - Name of parent ID field (e.g., 'courseId' for lessons, 'lessonId' for modules)
 * @param {Object} props - Component props
 */
export default function GenericCreateModal({
  config,
  show,
  onClose,
  onSuccess,
  parentId,
  entityData,
  isUpdateMode = false,
  isAdmin = false
}) {
  const { entityName, bucketName, fields, parentIdField } = config;

  // Initialize form state based on config fields
  const [formData, setFormData] = useState(() => {
    const initialState = {};
    fields.forEach(field => {
      initialState[field.name] = field.defaultValue || "";
    });
    return initialState;
  });

  const [existingImageUrl, setExistingImageUrl] = useState(null);
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);

  // Pre-fill form when in update mode
  useEffect(() => {
    if (show && isUpdateMode && entityData) {
      const newFormData = { ...formData };

      fields.forEach(field => {
        const value = entityData[field.name] || entityData[field.name.charAt(0).toUpperCase() + field.name.slice(1)] || field.defaultValue || "";
        newFormData[field.name] = value;
      });

      const imageUrl = entityData.imageUrl || entityData.ImageUrl || null;
      setExistingImageUrl(imageUrl);
      setFormData(newFormData);
    }
  }, [show, isUpdateMode, entityData, fields]);

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      const resetData = {};
      fields.forEach(field => {
        resetData[field.name] = field.defaultValue || "";
      });
      setFormData(resetData);
      setExistingImageUrl(null);
      setErrors({});
    }
  }, [show, fields]);

  const validateForm = () => {
    const newErrors = {};

    fields.forEach(field => {
      if (field.required && !formData[field.name]?.toString().trim()) {
        newErrors[field.name] = `${field.label} là bắt buộc`;
      }

      if (field.validate && typeof field.validate === 'function') {
        const fieldError = field.validate(formData[field.name], formData);
        if (fieldError) {
          newErrors[field.name] = fieldError;
        }
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (imageData) => {
    if (!validateForm()) {
      return;
    }

    setSubmitting(true);

    try {
      let response;
      const submitData = {};

      // Add form fields
      fields.forEach(field => {
        if (formData[field.name] !== undefined && formData[field.name] !== "") {
          submitData[field.name] = field.type === 'number' ? parseInt(formData[field.name]) : formData[field.name];
        }
      });

      // Add parent ID if provided
      if (parentId && parentIdField) {
        submitData[parentIdField] = parseInt(parentId);
      }

      // Add image data if uploaded
      if (imageData?.imageTempKey && imageData?.imageType) {
        submitData.imageTempKey = imageData.imageTempKey;
        submitData.imageType = imageData.imageType;
      }

      if (isUpdateMode && entityData) {
        const entityId = entityData[`${entityName}Id`] || entityData[`${entityName.charAt(0).toUpperCase() + entityName.slice(1)}Id`];
        if (!entityId) {
          throw new Error(`Không tìm thấy ID ${entityName}`);
        }

        response = isAdmin
          ? await adminService[`update${entityName.charAt(0).toUpperCase() + entityName.slice(1)}`](entityId, submitData)
          : await teacherService[`update${entityName.charAt(0).toUpperCase() + entityName.slice(1)}`](entityId, submitData);
      } else {
        response = isAdmin
          ? await adminService[`create${entityName.charAt(0).toUpperCase() + entityName.slice(1)}`](submitData)
          : await teacherService[`create${entityName.charAt(0).toUpperCase() + entityName.slice(1)}`](submitData);
      }

      if (response.data?.success) {
        onSuccess?.(response.data.data);
        onClose();
      } else {
        throw new Error(response.data?.message || `${isUpdateMode ? "Cập nhật" : "Tạo"} ${entityName} thất bại`);
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} ${entityName}:`, error);
      const errorMessage = error.response?.data?.message || error.message || `Có lỗi xảy ra khi ${isUpdateMode ? "cập nhật" : "tạo"} ${entityName}`;
      setErrors({ ...errors, submit: errorMessage });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = fields.every(field => !field.required || formData[field.name]?.toString().trim());

  const renderFormContent = () => (
    <>
      {fields.map(field => (
        <div key={field.name} className="form-group">
          <label className={`form-label ${field.required ? 'required' : ''}`}>
            {field.label}
          </label>

          {field.type === 'select' ? (
            <select
              className={`form-control ${errors[field.name] ? "is-invalid" : ""}`}
              value={formData[field.name]}
              onChange={(e) => {
                setFormData({ ...formData, [field.name]: e.target.value });
                setErrors({ ...errors, [field.name]: null });
              }}
            >
              <option value="">Chọn {field.label.toLowerCase()}</option>
              {field.options?.map(option => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          ) : field.type === 'textarea' ? (
            <textarea
              className={`form-control ${errors[field.name] ? "is-invalid" : ""}`}
              value={formData[field.name]}
              onChange={(e) => {
                const value = e.target.value;
                if (!field.maxLength || value.length <= field.maxLength) {
                  setFormData({ ...formData, [field.name]: value });
                  setErrors({ ...errors, [field.name]: null });
                }
              }}
              placeholder={field.placeholder}
              rows={field.rows || 3}
            />
          ) : (
            <input
              type={field.type || 'text'}
              className={`form-control ${errors[field.name] ? "is-invalid" : ""}`}
              value={formData[field.name]}
              onChange={(e) => {
                setFormData({ ...formData, [field.name]: e.target.value });
                setErrors({ ...errors, [field.name]: null });
              }}
              placeholder={field.placeholder}
            />
          )}

          {errors[field.name] && <div className="invalid-feedback">{errors[field.name]}</div>}

          {field.hint && (
            <div className="form-hint">
              {field.hint}
              {field.maxLength && (
                <span className={`float-end ${formData[field.name]?.length > field.maxLength * 0.9 ? 'text-warning' : ''} ${formData[field.name]?.length > field.maxLength ? 'text-danger' : ''}`}>
                  {formData[field.name]?.length || 0}/{field.maxLength}
                </span>
              )}
            </div>
          )}
        </div>
      ))}

      {/* Submit error */}
      {errors.submit && (
        <div className="alert alert-danger mt-3">{errors.submit}</div>
      )}
    </>
  );

  return (
    <CreateWithImageModal
      show={show}
      onClose={onClose}
      onSuccess={onSuccess}
      title={isUpdateMode ? `Cập nhật ${entityName}` : `Tạo ${entityName} mới`}
      isUpdateMode={isUpdateMode}
      bucketName={bucketName}
      onSubmit={handleSubmit}
      isFormValid={isFormValid}
      submitting={submitting}
      uploadingImage={uploadingImage}
      setUploadingImage={setUploadingImage}
      modalClass="create-with-image-modal"
      dialogClass="create-with-image-modal-dialog"
    >
      {() => renderFormContent()}
    </CreateWithImageModal>
  );
}