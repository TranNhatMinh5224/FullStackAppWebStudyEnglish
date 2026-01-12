import React, { useState, useEffect, useCallback } from "react";
import { Modal, Button } from "react-bootstrap";
import { essayService } from "../../../Services/essayService";
import FileUpload from "../../Common/FileUpload/FileUpload";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import "./CreateEssayModal.css";

const ESSAY_BUCKET = "essays"; // Backend uses "essays" bucket for both images and audios

export default function CreateEssayModal({ show, onClose, onSuccess, assessmentId, essayToUpdate = null, isAdmin = false }) {
  const isUpdateMode = !!essayToUpdate;
  
  // Form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [totalPoints, setTotalPoints] = useState("100");
  
  // Image state - simplified with FileUpload component
  const [imageUrl, setImageUrl] = useState(null);
  const [imageTempKey, setImageTempKey] = useState(null);
  const [imageType, setImageType] = useState(null);
  
  // Audio state - simplified with FileUpload component
  const [audioUrl, setAudioUrl] = useState(null);
  const [audioTempKey, setAudioTempKey] = useState(null);
  const [audioType, setAudioType] = useState(null);

  // Validation errors
  const [errors, setErrors] = useState({});

  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [loadingEssay, setLoadingEssay] = useState(false);
  const [showConfirmClose, setShowConfirmClose] = useState(false);

  const loadEssayData = useCallback(async () => {
    if (!essayToUpdate) return;
    
    setLoadingEssay(true);
    try {
      const essayId = essayToUpdate.essayId || essayToUpdate.EssayId;
      const response = isAdmin 
        ? await essayService.getAdminEssayById(essayId)
        : await essayService.getTeacherEssayById(essayId);
      
      if (response.data?.success && response.data?.data) {
        const essay = response.data.data;
        setTitle(essay.title || essay.Title || "");
        setDescription(essay.description || essay.Description || "");
        setTotalPoints((essay.totalPoints !== undefined ? essay.totalPoints : (essay.TotalPoints !== undefined ? essay.TotalPoints : 100)).toString());
        
        // Handle image
        const imageUrlValue = essay.imageUrl || essay.ImageUrl;
        if (imageUrlValue) {
          setImageUrl(imageUrlValue);
        }
        
        // Handle audio
        const audioUrlValue = essay.audioUrl || essay.AudioUrl;
        if (audioUrlValue) {
          setAudioUrl(audioUrlValue);
        }
      }
    } catch (error) {
      console.error("Error loading essay data:", error);
      setErrors({ ...errors, submit: "Không thể tải dữ liệu essay" });
    } finally {
      setLoadingEssay(false);
    }
  }, [isAdmin, essayToUpdate, errors]);

  // Load essay data when in update mode
  useEffect(() => {
    if (show && isUpdateMode && essayToUpdate) {
      loadEssayData();
    }
  }, [show, isUpdateMode, essayToUpdate, loadEssayData]);

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setTitle("");
      setDescription("");
      setTotalPoints("100");
      setImageUrl(null);
      setImageTempKey(null);
      setImageType(null);
      setAudioUrl(null);
      setAudioTempKey(null);
      setAudioType(null);
      setErrors({});
      setShowConfirmClose(false);
    }
  }, [show]);

  // Check if form has data
  const hasFormData = () => {
    return (
      title.trim() !== "" ||
      description.trim() !== "" ||
      totalPoints !== "100" ||
      imageUrl !== null ||
      imageTempKey !== null ||
      audioUrl !== null ||
      audioTempKey !== null
    );
  };

  // Handle close with confirmation
  const handleClose = () => {
    if (hasFormData() && !submitting && !loadingEssay) {
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

  // FileUpload callbacks for Image
  const handleImageUploadSuccess = (tempKey, fileType, previewUrl) => {
    setImageTempKey(tempKey);
    setImageType(fileType);
    setImageUrl(previewUrl);
    setErrors({ ...errors, image: null });
  };

  const handleImageRemove = () => {
    setImageTempKey(null);
    setImageType(null);
    setImageUrl(null);
  };

  const handleImageError = (errorMessage) => {
    setErrors({ ...errors, image: errorMessage });
  };

  // FileUpload callbacks for Audio
  const handleAudioUploadSuccess = (tempKey, fileType, previewUrl) => {
    setAudioTempKey(tempKey);
    setAudioType(fileType);
    setAudioUrl(previewUrl);
    setErrors({ ...errors, audio: null });
  };

  const handleAudioRemove = () => {
    setAudioTempKey(null);
    setAudioType(null);
    setAudioUrl(null);
  };

  const handleAudioError = (errorMessage) => {
    setErrors({ ...errors, audio: errorMessage });
  };

  const validateForm = () => {
    const newErrors = {};

    if (!title.trim()) {
      newErrors.title = "Tiêu đề là bắt buộc";
    }

    if (!totalPoints || parseFloat(totalPoints) <= 0) {
      newErrors.totalPoints = "Tổng điểm phải lớn hơn 0";
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
        assessmentId: parseInt(assessmentId),
        title: title.trim(),
        description: description.trim() || null,
        totalPoints: parseFloat(totalPoints),
        imageTempKey: imageTempKey || null,
        imageType: imageType || null,
        audioTempKey: audioTempKey || null,
        audioType: audioType || null,
      };

      let response;
      if (isUpdateMode && essayToUpdate) {
        const essayId = essayToUpdate.essayId || essayToUpdate.EssayId;
        // UpdateEssayDto doesn't have totalPoints
        const updateData = {
          title: submitData.title,
          description: submitData.description,
          imageTempKey: submitData.imageTempKey,
          imageType: submitData.imageType,
          audioTempKey: submitData.audioTempKey,
          audioType: submitData.audioType,
        };
        response = isAdmin
          ? await essayService.updateAdminEssay(essayId, updateData)
          : await essayService.updateEssay(essayId, updateData);
      } else {
        response = isAdmin
          ? await essayService.createAdminEssay(submitData)
          : await essayService.createEssay(submitData);
      }

      if (response.data?.success) {
        onSuccess?.();
        onClose();
      } else {
        throw new Error(response.data?.message || (isUpdateMode ? "Cập nhật Essay thất bại" : "Tạo Essay thất bại"));
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} essay:`, error);
      const errorMessage = error.response?.data?.message || error.message || (isUpdateMode ? "Có lỗi xảy ra khi cập nhật Essay" : "Có lỗi xảy ra khi tạo Essay");
      setErrors({ ...errors, submit: errorMessage });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = 
    title.trim() && 
    totalPoints && 
    parseFloat(totalPoints) > 0;

  return (
    <>
    <Modal 
      show={show} 
      onHide={handleClose}
      backdrop="static"
      keyboard={false}
      centered 
      className="create-essay-modal modal-modern" 
      dialogClassName="create-essay-modal-dialog"
    >
      <Modal.Header closeButton>
        <Modal.Title>{isUpdateMode ? "Cập nhật Essay" : "Tạo Essay mới"}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loadingEssay ? (
          <div className="text-center py-4">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Đang tải...</span>
            </div>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
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
                placeholder="Nhập tiêu đề Essay"
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
                placeholder="Nhập mô tả Essay (không bắt buộc)"
                rows={3}
              />
              {errors.description && <div className="invalid-feedback">{errors.description}</div>}
              <div className="form-text">Không bắt buộc</div>
            </div>

            {/* Tổng điểm */}
            <div className="mb-3">
              <label className="form-label required">Tổng điểm</label>
              <input
                type="number"
                className={`form-control ${errors.totalPoints ? "is-invalid" : ""}`}
                value={totalPoints}
                onChange={(e) => {
                  setTotalPoints(e.target.value);
                  setErrors({ ...errors, totalPoints: null });
                }}
                placeholder="Nhập tổng điểm"
                min="0.01"
                step="0.01"
                disabled={isUpdateMode}
              />
              {errors.totalPoints && <div className="invalid-feedback">{errors.totalPoints}</div>}
              <div className="form-text">*Bắt buộc{isUpdateMode ? " (không thể thay đổi khi cập nhật)" : ""}</div>
            </div>

            {/* Image Upload - Using FileUpload component */}
            <div className="mb-3">
              <label className="form-label">Ảnh đính kèm</label>
              <FileUpload
                bucket={ESSAY_BUCKET}
                accept="image/*"
                maxSize={5}
                existingUrl={imageUrl}
                onUploadSuccess={handleImageUploadSuccess}
                onRemove={handleImageRemove}
                onError={handleImageError}
                label="Chọn ảnh hoặc kéo thả vào đây"
                hint="Hỗ trợ: JPG, PNG, GIF (tối đa 5MB)"
              />
              {errors.image && <div className="text-danger small mt-1">{errors.image}</div>}
              <div className="form-text">Không bắt buộc</div>
            </div>

            {/* Audio Upload - Using FileUpload component */}
            <div className="mb-3">
              <label className="form-label">Âm thanh đính kèm</label>
              <FileUpload
                bucket={ESSAY_BUCKET}
                accept="audio/*"
                maxSize={10}
                existingUrl={audioUrl}
                onUploadSuccess={handleAudioUploadSuccess}
                onRemove={handleAudioRemove}
                onError={handleAudioError}
                label="Chọn file âm thanh hoặc kéo thả vào đây"
                hint="Hỗ trợ: MP3, WAV, OGG (tối đa 10MB)"
              />
              {errors.audio && <div className="text-danger small mt-1">{errors.audio}</div>}
              <div className="form-text">Không bắt buộc</div>
            </div>

            {/* Submit error */}
            {errors.submit && (
              <div className="alert alert-danger mt-3">{errors.submit}</div>
            )}
          </form>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={handleClose} disabled={submitting || loadingEssay}>
          Huỷ
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={!isFormValid || submitting || loadingEssay}
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

