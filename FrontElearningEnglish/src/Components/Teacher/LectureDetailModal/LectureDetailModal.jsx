import React, { useState, useEffect } from "react";
import { Modal, Button, Badge, Spinner } from "react-bootstrap";
import { FaBook, FaVideo, FaFileAlt, FaTimes } from "react-icons/fa";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { lectureService } from "../../../Services/lectureService";
import { useEnums } from "../../../Context/EnumContext";
import "./LectureDetailModal.css";

export default function LectureDetailModal({ 
  show, 
  onClose, 
  lectureId, 
  isAdmin = false 
}) {
  const [lecture, setLecture] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const { getEnumLabel } = useEnums();

  useEffect(() => {
    if (show && lectureId) {
      fetchLecture();
    } else {
      setLecture(null);
      setError(null);
    }
  }, [show, lectureId, isAdmin]);

  const fetchLecture = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = isAdmin
        ? await lectureService.getAdminLectureById(lectureId)
        : await lectureService.getTeacherLectureById(lectureId);

      if (response.data?.success && response.data?.data) {
        setLecture(response.data.data);
      } else {
        setError("Không thể tải thông tin lecture");
      }
    } catch (err) {
      console.error("Error fetching lecture:", err);
      setError("Có lỗi xảy ra khi tải lecture");
    } finally {
      setLoading(false);
    }
  };

  const getLectureIcon = (type) => {
    const typeName = getEnumLabel("LectureType", type)?.toLowerCase() || "";
    if (typeName === "content") return <FaBook className="text-primary" />;
    if (typeName === "document") return <FaFileAlt className="text-secondary" />;
    if (typeName === "video") return <FaVideo className="text-danger" />;
    return <FaBook />;
  };

  const getLectureLabel = (type) => {
    const typeName = getEnumLabel("LectureType", type) || "";
    const labels = {
      "Content": "Nội dung",
      "Document": "Tài liệu",
      "Video": "Video"
    };
    return labels[typeName] || "Không xác định";
  };

  return (
    <Modal 
      show={show} 
      onHide={onClose} 
      centered 
      size="xl" 
      className="lecture-detail-modal modal-modern"
    >
      <Modal.Header className="lecture-detail-header">
        <Modal.Title className="d-flex align-items-center gap-2">
          {lecture && getLectureIcon(lecture.type || lecture.Type)}
          <span>Chi tiết Lecture</span>
        </Modal.Title>
        <Button variant="link" className="text-muted p-0" onClick={onClose}>
          <FaTimes />
        </Button>
      </Modal.Header>

      <Modal.Body>
        {loading ? (
          <div className="text-center py-5">
            <Spinner animation="border" variant="primary" />
            <p className="mt-3 text-muted">Đang tải...</p>
          </div>
        ) : error ? (
          <div className="text-center py-5">
            <p className="text-danger">{error}</p>
            <Button variant="primary" onClick={fetchLecture}>
              Thử lại
            </Button>
          </div>
        ) : lecture ? (
          <div className="lecture-detail-content">
            {/* Header Info */}
            <div className="lecture-detail-header-info mb-4">
              <h3 className="fw-bold mb-2">{lecture.title || lecture.Title}</h3>
              <div className="d-flex align-items-center gap-2 flex-wrap">
                <Badge bg="info">{getLectureLabel(lecture.type || lecture.Type)}</Badge>
                {lecture.numberingLabel && (
                  <Badge bg="secondary">{lecture.numberingLabel}</Badge>
                )}
                {lecture.duration && (
                  <Badge bg="success">
                    {Math.floor(lecture.duration / 60)}:{(lecture.duration % 60).toString().padStart(2, '0')}
                  </Badge>
                )}
              </div>
            </div>

            {/* Markdown Content */}
            {(lecture.type === 1 || lecture.type === 2) && lecture.markdownContent && (
              <div className="lecture-detail-markdown mb-4">
                <h5 className="mb-3">Nội dung</h5>
                <div className="markdown-content">
                  <ReactMarkdown remarkPlugins={[remarkGfm]}>
                    {lecture.markdownContent || lecture.MarkdownContent || ""}
                  </ReactMarkdown>
                </div>
              </div>
            )}

            {/* Media */}
            {lecture.mediaUrl && (
              <div className="lecture-detail-media mb-4">
                <h5 className="mb-3">Media</h5>
                {lecture.type === 3 && (
                  <video 
                    src={lecture.mediaUrl} 
                    controls 
                    className="w-100 rounded"
                    style={{ maxHeight: '500px' }}
                  />
                )}
                {lecture.type === 2 && (
                  <div className="media-document">
                    <a 
                      href={lecture.mediaUrl} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="btn btn-outline-primary"
                    >
                      <FaFileAlt className="me-2" />
                      Mở tài liệu
                    </a>
                  </div>
                )}
              </div>
            )}

            {/* Metadata */}
            <div className="lecture-detail-metadata">
              <small className="text-muted">
                Tạo lúc: {new Date(lecture.createdAt || lecture.CreatedAt).toLocaleString('vi-VN')}
              </small>
            </div>
          </div>
        ) : null}
      </Modal.Body>

      <Modal.Footer>
        <Button variant="secondary" onClick={onClose}>
          Đóng
        </Button>
      </Modal.Footer>
    </Modal>
  );
}
