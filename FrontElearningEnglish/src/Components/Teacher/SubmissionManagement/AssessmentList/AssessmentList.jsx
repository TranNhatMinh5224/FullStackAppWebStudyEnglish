import React, { useState, useEffect } from "react";
import { Card, Spinner, Badge } from "react-bootstrap";
import { FaClipboardCheck, FaClock, FaCalendarAlt } from "react-icons/fa";
import { assessmentService } from "../../../../Services/assessmentService";
import "./AssessmentList.css";

export default function AssessmentList({ moduleId, onSelect, isAdmin = false }) {
  const [assessments, setAssessments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchAssessments();
  }, [moduleId]);

  const fetchAssessments = async () => {
    try {
      setLoading(true);
      setError("");
      // Use teacher or admin API based on isAdmin prop
      const response = isAdmin 
        ? await assessmentService.getAdminAssessmentsByModule(moduleId)
        : await assessmentService.getTeacherAssessmentsByModule(moduleId);
      if (response.data?.success) {
        const data = response.data.data || [];
        setAssessments(data);
      }
    } catch (err) {
      console.error("Error fetching assessments:", err);
      setError("Không thể tải danh sách assessment");
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    );
  }

  if (error) {
    return <div className="alert alert-danger">{error}</div>;
  }

  return (
    <div className="assessment-list">
      {assessments.length === 0 ? (
        <div className="text-center text-muted py-5">
          <p>Chưa có assessment nào</p>
        </div>
      ) : (
        <div className="assessment-grid">
          {assessments.map((assessment) => {
            const assessmentId = assessment.assessmentId || assessment.AssessmentId;
            const title = assessment.title || assessment.Title || "Untitled Assessment";
            const openAt = assessment.openAt || assessment.OpenAt;
            const dueAt = assessment.dueAt || assessment.DueAt;
            const timeLimit = assessment.timeLimit || assessment.TimeLimit;
            const isPublished = assessment.isPublished !== undefined 
              ? assessment.isPublished 
              : assessment.IsPublished;

            return (
              <Card
                key={assessmentId}
                className="assessment-card h-100"
                onClick={() => onSelect(assessment)}
                style={{ cursor: "pointer" }}
              >
                <Card.Body>
                  <div className="d-flex align-items-start gap-3 mb-3">
                    <div className="assessment-icon flex-shrink-0">
                      <FaClipboardCheck size={24} />
                    </div>
                    <div className="flex-grow-1">
                      <div className="d-flex justify-content-between align-items-start">
                        <Card.Title className="assessment-title mb-1 text-break">{title}</Card.Title>
                        <Badge bg={isPublished ? "success" : "secondary"} className="ms-2">
                          {isPublished ? "Đã xuất bản" : "Nháp"}
                        </Badge>
                      </div>
                      
                      {timeLimit && (
                        <div className="text-muted small d-flex align-items-center mt-1">
                          <FaClock className="me-1" size={12} />
                          {timeLimit}
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="border-top pt-2 mt-2">
                    <div className="d-flex justify-content-between text-muted small">
                      <div title="Thời gian mở">
                        <FaCalendarAlt className="me-1" size={12} />
                        Mở: {formatDate(openAt)}
                      </div>
                    </div>
                    <div className="d-flex justify-content-between text-muted small mt-1">
                      <div title="Hạn nộp" className="text-danger">
                        <FaCalendarAlt className="me-1" size={12} />
                        Đóng: {formatDate(dueAt)}
                      </div>
                    </div>
                  </div>
                </Card.Body>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}

