import React, { useEffect } from "react";
import { Modal, Button, Form, Row, Col, Spinner } from "react-bootstrap";
import { FaLayerGroup } from "react-icons/fa";
import { useQuizGroupForm } from "../../../hooks/useQuizGroupForm";
import { MediaUploader } from "../../Common/MediaUploader";
import "./CreateQuizGroupModal.css";

const QUIZ_GROUP_BUCKET = "quizgroups";

/**
 * Modal tạo/cập nhật QuizGroup
 * Sử dụng shared components và custom hooks để dễ maintain
 * 
 * Features:
 * - Tạo/Sửa QuizGroup với đầy đủ fields
 * - Upload hình ảnh và video/audio
 * - Validation form
 * - Hỗ trợ cả Teacher và Admin
 */
export default function CreateQuizGroupModal({ 
  show, 
  onClose, 
  onSuccess, 
  quizSectionId, 
  groupToUpdate = null, 
  isAdmin = false 
}) {
  const groupId = groupToUpdate?.quizGroupId || groupToUpdate?.QuizGroupId || null;
  
  const {
    formData,
    mediaData,
    loading,
    loadingData,
    errors,
    submitError,
    isUpdateMode,
    isValid,
    updateField,
    setImageMedia,
    setVideoMedia,
    clearImage,
    clearVideo,
    submit,
    reset,
    loadGroupData,
  } = useQuizGroupForm(groupId, quizSectionId, isAdmin);

  // Load data khi mở modal ở chế độ edit
  useEffect(() => {
    if (show && isUpdateMode) {
      loadGroupData();
    }
  }, [show, isUpdateMode, loadGroupData]);

  // Reset form khi đóng modal
  useEffect(() => {
    if (!show) {
      reset();
    }
  }, [show, reset]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const result = await submit();
    
    if (result.success) {
      onSuccess?.(result.data);
      onClose();
    }
  };

  const handleImageUpload = (tempKey, type) => {
    setImageMedia(tempKey, type);
  };

  const handleVideoUpload = (tempKey, type, duration) => {
    setVideoMedia(tempKey, type, duration);
  };

  return (
    <Modal 
      show={show} 
      onHide={onClose} 
      centered 
      size="lg"
      className="create-quiz-group-modal" 
      backdrop="static"
    >
      <Modal.Header closeButton className="border-0 pb-0">
        <Modal.Title className="d-flex align-items-center">
          <FaLayerGroup className="me-2 text-primary" />
          {isUpdateMode ? "Cập nhật Group" : "Tạo Group mới"}
        </Modal.Title>
      </Modal.Header>
      
      <Modal.Body className="pt-2">
        {loadingData ? (
          <div className="text-center py-5">
            <Spinner animation="border" variant="primary" />
            <p className="text-muted mt-2">Đang tải dữ liệu...</p>
          </div>
        ) : (
          <Form onSubmit={handleSubmit}>
            <Row>
              {/* Left Column - Form Fields */}
              <Col md={7}>
                {/* Tên nhóm */}
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">
                    Tên nhóm <span className="text-danger">*</span>
                  </Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.name}
                    onChange={(e) => updateField("name", e.target.value)}
                    placeholder="VD: Part 1, Reading Passage 1"
                    isInvalid={!!errors.name}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.name}
                  </Form.Control.Feedback>
                  <Form.Text className="text-muted">
                    Mã định danh ngắn gọn cho nhóm câu hỏi
                  </Form.Text>
                </Form.Group>

                {/* Tiêu đề */}
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">
                    Tiêu đề hiển thị <span className="text-danger">*</span>
                  </Form.Label>
                  <Form.Control
                    type="text"
                    value={formData.title}
                    onChange={(e) => updateField("title", e.target.value)}
                    placeholder="VD: Đọc đoạn văn sau và trả lời câu hỏi"
                    isInvalid={!!errors.title}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.title}
                  </Form.Control.Feedback>
                  <Form.Text className="text-muted">
                    Tiêu đề sẽ hiển thị cho học viên
                  </Form.Text>
                </Form.Group>

                {/* Mô tả / Đoạn văn */}
                <Form.Group className="mb-3">
                  <Form.Label className="fw-semibold">
                    Nội dung / Đoạn văn
                  </Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={5}
                    value={formData.description}
                    onChange={(e) => updateField("description", e.target.value)}
                    placeholder="Nhập nội dung đoạn văn, bài đọc hoặc mô tả cho nhóm câu hỏi..."
                    isInvalid={!!errors.description}
                  />
                  <Form.Control.Feedback type="invalid">
                    {errors.description}
                  </Form.Control.Feedback>
                  <Form.Text className="text-muted">
                    Dùng cho Reading Comprehension, Listening passage...
                  </Form.Text>
                </Form.Group>

                {/* Tổng điểm & Thứ tự */}
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label className="fw-semibold">
                        Tổng điểm <span className="text-danger">*</span>
                      </Form.Label>
                      <Form.Control
                        type="number"
                        value={formData.sumScore}
                        onChange={(e) => updateField("sumScore", e.target.value)}
                        min="0"
                        step="0.5"
                        isInvalid={!!errors.sumScore}
                      />
                      <Form.Control.Feedback type="invalid">
                        {errors.sumScore}
                      </Form.Control.Feedback>
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label className="fw-semibold">
                        Thứ tự hiển thị
                      </Form.Label>
                      <Form.Control
                        type="number"
                        value={formData.displayOrder}
                        onChange={(e) => updateField("displayOrder", e.target.value)}
                        min="0"
                        isInvalid={!!errors.displayOrder}
                      />
                      <Form.Control.Feedback type="invalid">
                        {errors.displayOrder}
                      </Form.Control.Feedback>
                      <Form.Text className="text-muted">
                        Nhỏ hơn = hiển thị trước
                      </Form.Text>
                    </Form.Group>
                  </Col>
                </Row>
              </Col>

              {/* Right Column - Media */}
              <Col md={5}>
                {/* Image Upload */}
                <MediaUploader
                  bucket={QUIZ_GROUP_BUCKET}
                  acceptTypes={['image']}
                  onUpload={handleImageUpload}
                  onRemove={clearImage}
                  initialPreview={mediaData.imgUrl}
                  initialType="image"
                  label="Hình ảnh minh họa"
                  placeholder="Thêm hình ảnh cho nhóm câu hỏi"
                  maxSize={10}
                  height="150px"
                  disabled={loading}
                />

                {/* Video Upload */}
                <div className="mt-3">
                  <MediaUploader
                    bucket={QUIZ_GROUP_BUCKET}
                    acceptTypes={['video', 'audio']}
                    onUpload={handleVideoUpload}
                    onRemove={clearVideo}
                    initialPreview={mediaData.videoUrl}
                    initialType="video"
                    label="Video / Audio"
                    placeholder="Thêm video hoặc audio"
                    maxSize={100}
                    height="150px"
                    disabled={loading}
                  />
                </div>

                {/* Info box */}
                <div className="alert alert-light border mt-3 small">
                  <strong className="d-block mb-1">💡 Gợi ý sử dụng:</strong>
                  <ul className="mb-0 ps-3">
                    <li><strong>Reading:</strong> Nhập đoạn văn vào "Nội dung"</li>
                    <li><strong>Listening:</strong> Upload audio/video</li>
                    <li><strong>Photo:</strong> Upload hình ảnh minh họa</li>
                  </ul>
                </div>
              </Col>
            </Row>

            {/* Submit error */}
            {submitError && (
              <div className="alert alert-danger mt-3 mb-0">
                {submitError}
              </div>
            )}
          </Form>
        )}
      </Modal.Body>

      <Modal.Footer className="border-0 pt-0">
        <Button 
          variant="outline-secondary" 
          onClick={onClose} 
          disabled={loading || loadingData}
        >
          Huỷ
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={!isValid || loading || loadingData}
        >
          {loading ? (
            <>
              <Spinner size="sm" className="me-2" />
              {isUpdateMode ? "Đang cập nhật..." : "Đang tạo..."}
            </>
          ) : (
            isUpdateMode ? "Cập nhật" : "Tạo Group"
          )}
        </Button>
      </Modal.Footer>
    </Modal>
  );
}

