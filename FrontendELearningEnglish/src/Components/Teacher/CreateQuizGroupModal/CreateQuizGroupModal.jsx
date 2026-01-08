import React, { useState, useEffect } from "react";
import { Modal, Button } from "react-bootstrap";
import { quizService } from "../../../Services/quizService";
import "./CreateQuizGroupModal.css";

export default function CreateQuizGroupModal({ show, onClose, onSuccess, quizSectionId, groupToUpdate = null, isAdmin = false }) {
  const isUpdateMode = !!groupToUpdate;
  
  // Form state
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [title, setTitle] = useState("");
  const [sumScore, setSumScore] = useState("0");

  // Validation errors
  const [errors, setErrors] = useState({});

  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [loadingGroup, setLoadingGroup] = useState(false);

  // Load group data when in update mode
  useEffect(() => {
    if (show && isUpdateMode && groupToUpdate) {
      loadGroupData();
    }
  }, [show, isUpdateMode, groupToUpdate]);

  const loadGroupData = async () => {
    if (!groupToUpdate) return;
    
    setLoadingGroup(true);
    try {
      const groupId = groupToUpdate.quizGroupId || groupToUpdate.QuizGroupId;
      const response = await quizService.getQuizGroupById(groupId);
      
      if (response.data?.success && response.data?.data) {
        const group = response.data.data;
        setName(group.name || group.Name || "");
        setDescription(group.description || group.Description || "");
        setTitle(group.title || group.Title || "");
        
        // Safely handle sumScore
        const scoreVal = group.sumScore !== undefined ? group.sumScore : (group.SumScore !== undefined ? group.SumScore : 0);
        setSumScore((scoreVal ?? 0).toString());
      }
    } catch (error) {
      console.error("Error loading group data:", error);
      setErrors({ ...errors, submit: "Không thể tải dữ liệu group" });
    } finally {
      setLoadingGroup(false);
    }
  };

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setName("");
      setDescription("");
      setTitle("");
      setSumScore("0");
      setErrors({});
    }
  }, [show]);

  const validateForm = () => {
    const newErrors = {};

    if (!name.trim()) {
      newErrors.name = "Tên nhóm là bắt buộc";
    }

    if (!title.trim()) {
      newErrors.title = "Tiêu đề là bắt buộc";
    }

    if (!sumScore || parseFloat(sumScore) < 0) {
      newErrors.sumScore = "Tổng điểm phải lớn hơn hoặc bằng 0";
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
        quizSectionId: parseInt(quizSectionId),
        name: name.trim(),
        description: description.trim() || null,
        title: title.trim(),
        sumScore: parseFloat(sumScore),
      };

      let response;
      if (isUpdateMode && groupToUpdate) {
        const groupId = groupToUpdate.quizGroupId || groupToUpdate.QuizGroupId;
        const updateData = {
          name: submitData.name,
          description: submitData.description,
          title: submitData.title,
          sumScore: submitData.sumScore,
        };
        response = isAdmin
          ? await quizService.updateAdminQuizGroup(groupId, updateData)
          : await quizService.updateQuizGroup(groupId, updateData);
      } else {
        response = isAdmin
          ? await quizService.createAdminQuizGroup(submitData)
          : await quizService.createQuizGroup(submitData);
      }

      if (response.data?.success) {
        onSuccess?.();
        onClose();
      } else {
        throw new Error(response.data?.message || (isUpdateMode ? "Cập nhật Group thất bại" : "Tạo Group thất bại"));
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} group:`, error);
      const errorMessage = error.response?.data?.message || error.message || (isUpdateMode ? "Có lỗi xảy ra khi cập nhật Group" : "Có lỗi xảy ra khi tạo Group");
      setErrors({ ...errors, submit: errorMessage });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = 
    name.trim() && 
    title.trim() && 
    sumScore && 
    parseFloat(sumScore) >= 0;

  return (
    <Modal 
      show={show} 
      onHide={onClose} 
      centered 
      className="create-quiz-group-modal" 
      dialogClassName="create-quiz-group-modal-dialog"
      style={{ zIndex: 1050 }}
    >
      <Modal.Header closeButton>
        <Modal.Title>{isUpdateMode ? "Cập nhật Group" : "Tạo Group mới"}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loadingGroup ? (
          <div className="text-center py-4">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Đang tải...</span>
            </div>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            {/* Tên nhóm */}
            <div className="mb-3">
              <label className="form-label required">Tên nhóm</label>
              <input
                type="text"
                className={`form-control ${errors.name ? "is-invalid" : ""}`}
                value={name}
                onChange={(e) => {
                  setName(e.target.value);
                  setErrors({ ...errors, name: null });
                }}
                placeholder="Nhập tên nhóm"
              />
              {errors.name && <div className="invalid-feedback">{errors.name}</div>}
              <div className="form-text">*Bắt buộc</div>
            </div>

            {/* Tiêu đề */}
            <div className="mb-3">
              <label className="form-label required">Tiêu đề</label>
              <input
                type="text"
                className={`form-control ${errors.title ? "is-invalid" : ""}`}
                value={title}
                onChange={(e) => {
                  setTitle(e.target.value);
                  setErrors({ ...errors, title: null });
                }}
                placeholder="Nhập tiêu đề"
              />
              {errors.title && <div className="invalid-feedback">{errors.title}</div>}
              <div className="form-text">*Bắt buộc</div>
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
                placeholder="Nhập mô tả (không bắt buộc)"
                rows={2}
              />
              {errors.description && <div className="invalid-feedback">{errors.description}</div>}
              <div className="form-text">Không bắt buộc</div>
            </div>

            {/* Tổng điểm */}
            <div className="mb-3">
              <label className="form-label required">Tổng điểm</label>
              <input
                type="number"
                className={`form-control ${errors.sumScore ? "is-invalid" : ""}`}
                value={sumScore}
                onChange={(e) => {
                  setSumScore(e.target.value);
                  setErrors({ ...errors, sumScore: null });
                }}
                placeholder="Nhập tổng điểm"
                min="0"
                step="0.01"
              />
              {errors.sumScore && <div className="invalid-feedback">{errors.sumScore}</div>}
              <div className="form-text">*Bắt buộc</div>
            </div>



            {/* Submit error */}
            {errors.submit && (
              <div className="alert alert-danger mt-3">{errors.submit}</div>
            )}
          </form>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onClose} disabled={submitting || loadingGroup}>
          Huỷ
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={!isFormValid || submitting || loadingGroup}
        >
          {submitting ? (isUpdateMode ? "Đang cập nhật..." : "Đang tạo...") : (isUpdateMode ? "Cập nhật" : "Tạo")}
        </Button>
      </Modal.Footer>
    </Modal>
  );
}

