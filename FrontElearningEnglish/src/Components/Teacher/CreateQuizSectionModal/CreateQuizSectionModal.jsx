import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import { quizService } from "../../../Services/quizService";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import { useAuth } from "../../../Context/AuthContext";
import "./CreateQuizSectionModal.css";

export default function CreateQuizSectionModal({ show, onClose, onSuccess, quizId, sectionToUpdate = null, isAdmin: propIsAdmin = false }) {
  const { roles } = useAuth();
  const isUpdateMode = !!sectionToUpdate;
  
  // Auto-detect admin role from AuthContext if not explicitly provided
  const isAdmin = propIsAdmin || (roles && roles.some(role => {
    const roleName = typeof role === 'string' ? role : (role?.name || '');
    return roleName === "SuperAdmin" || 
           roleName === "ContentAdmin" || 
           roleName === "FinanceAdmin";
  }));
  
  // Form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");

  // Original data for comparison (update mode)
  const [originalData, setOriginalData] = useState(null);

  // Validation errors
  const [errors, setErrors] = useState({});

  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [loadingSection, setLoadingSection] = useState(false);
  const [showConfirmClose, setShowConfirmClose] = useState(false);

  // Load section data when in update mode
  useEffect(() => {
    if (show && isUpdateMode && sectionToUpdate) {
      loadSectionData();
    }
  }, [show, isUpdateMode, sectionToUpdate]);

  const loadSectionData = async () => {
    if (!sectionToUpdate) return;
    
    setLoadingSection(true);
    try {
      const sectionId = sectionToUpdate.quizSectionId || sectionToUpdate.QuizSectionId;
      const response = isAdmin
        ? await quizService.getAdminQuizSectionById(sectionId)
        : await quizService.getQuizSectionById(sectionId);
      
      if (response.data?.success && response.data?.data) {
        const section = response.data.data;
        const loadedTitle = section.title || section.Title || "";
        const loadedDescription = section.description || section.Description || "";
        setTitle(loadedTitle);
        setDescription(loadedDescription);
        // Save original data for comparison
        setOriginalData({
          title: loadedTitle,
          description: loadedDescription
        });
      }
    } catch (error) {
      console.error("Error loading section data:", error);
      setErrors({ ...errors, submit: "Không thể tải dữ liệu section" });
    } finally {
      setLoadingSection(false);
    }
  };

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setTitle("");
      setDescription("");
      setOriginalData(null);
      setErrors({});
      setShowConfirmClose(false);
    }
  }, [show]);

  // Check if form has data or has been modified
  const hasFormData = () => {
    if (isUpdateMode && originalData) {
      // In update mode, check if data changed from original
      return title !== originalData.title || description !== originalData.description;
    }
    // In create mode, check if any field has data
    return title.trim() !== "" || description.trim() !== "";
  };

  // Handle close with confirmation
  const handleClose = () => {
    // Always allow closing if submitting or loading
    if (submitting || loadingSection) {
      return; // Don't close if submitting/loading
    }
    
    // Check if form has data
    if (hasFormData()) {
      setShowConfirmClose(true);
    } else {
      onClose();
    }
  };

  // Handle confirm close
  const handleConfirmClose = () => {
    setShowConfirmClose(false);
    onClose();
  };

  const validateForm = () => {
    const newErrors = {};

    if (!title.trim()) {
      newErrors.title = "Tiêu đề là bắt buộc";
    } else if (title.trim().length > 200) {
      newErrors.title = "Tiêu đề không được vượt quá 200 ký tự";
    }

    if (description && description.trim().length > 1000) {
      newErrors.description = "Mô tả không được vượt quá 1000 ký tự";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setSubmitting(true);

    try {
      const submitData = {
        quizId: parseInt(quizId),
        title: title.trim(),
        description: description.trim() || null,
      };

      let response;
      if (isUpdateMode && sectionToUpdate) {
        const sectionId = sectionToUpdate.quizSectionId || sectionToUpdate.QuizSectionId;
        const updateData = {
          title: submitData.title,
          description: submitData.description,
        };
        response = isAdmin
          ? await quizService.updateAdminQuizSection(sectionId, updateData)
          : await quizService.updateQuizSection(sectionId, updateData);
      } else {
        response = isAdmin
          ? await quizService.createAdminQuizSection(submitData)
          : await quizService.createQuizSection(submitData);
      }

      if (response.data?.success) {
        onSuccess?.();
        onClose();
      } else {
        throw new Error(response.data?.message || (isUpdateMode ? "Cập nhật Section thất bại" : "Tạo Section thất bại"));
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} section:`, error);
      const errorMessage = error.response?.data?.message || error.message || (isUpdateMode ? "Có lỗi xảy ra khi cập nhật Section" : "Có lỗi xảy ra khi tạo Section");
      setErrors({ ...errors, submit: errorMessage });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = title.trim();

  return (
    <>
    <Modal 
      show={show} 
      onHide={handleClose}
      backdrop={submitting ? "static" : true}
      keyboard={!submitting}
      centered 
      className="create-quiz-section-modal modal-modern" 
      dialogClassName="create-quiz-section-modal-dialog"
    >
      <Modal.Header>
        <Modal.Title>{isUpdateMode ? "Cập nhật Section" : "Tạo Section mới"}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loadingSection ? (
          <div className="text-center py-4">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Đang tải...</span>
            </div>
          </div>
        ) : (
          <form onSubmit={(e) => {
            e.preventDefault();
            handleSubmit(e);
          }}>
            {/* Thông tin cơ bản */}
            <div className="form-section-card mb-4">
              <div className="form-section-title">Thông tin cơ bản</div>
              
              {/* Tiêu đề */}
              <div className="mb-4">
                <label className="form-label required">Tiêu đề Section</label>
                <input
                  type="text"
                  className={`form-control ${errors.title ? "is-invalid" : ""}`}
                  value={title}
                  onChange={(e) => {
                    setTitle(e.target.value);
                    setErrors({ ...errors, title: null });
                  }}
                  placeholder="Nhập tiêu đề Section"
                  maxLength={200}
                />
                {errors.title && <div className="invalid-feedback">{errors.title}</div>}
                <div className="form-text">*Bắt buộc (tối đa 200 ký tự)</div>
              </div>

              {/* Mô tả */}
              <div className="mb-3">
                <label className="form-label">Mô tả</label>
                <textarea
                  className={`form-control ${errors.description ? "is-invalid" : ""}`}
                  value={description}
                  onChange={(e) => {
                    setDescription(e.target.value);
                    setErrors({ ...errors, description: null });
                  }}
                  placeholder="Nhập mô tả Section (không bắt buộc)"
                  rows={4}
                  maxLength={1000}
                />
                {errors.description && <div className="invalid-feedback">{errors.description}</div>}
                <div className="form-text">Không bắt buộc (tối đa 1000 ký tự)</div>
              </div>
            </div>

            {/* Submit error */}
            {errors.submit && (
              <div className="alert alert-danger mt-3">{errors.submit}</div>
            )}
          </form>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button 
          variant="secondary" 
          onClick={handleClose} 
          disabled={submitting}
          type="button"
        >
          Huỷ
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={!isFormValid || submitting || loadingSection}
          type="button"
        >
          {submitting ? (isUpdateMode ? "Đang cập nhật..." : "Đang tạo...") : (isUpdateMode ? "Cập nhật" : "Tạo")}
        </Button>
      </Modal.Footer>
    </Modal>

    <ConfirmModal
      show={showConfirmClose}
      onHide={() => setShowConfirmClose(false)}
      onConfirm={handleConfirmClose}
      title="Xác nhận đóng"
      message="Bạn có dữ liệu chưa lưu. Bạn có chắc chắn muốn đóng không?"
      confirmText="Đóng"
      cancelText="Tiếp tục chỉnh sửa"
      variant="warning"
    />
    </>
  );
}

