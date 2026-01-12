import React, { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container, Button, Badge } from "react-bootstrap";
import { FaPlus } from "react-icons/fa";
import TeacherHeader from "../../../Components/Header/TeacherHeader";
import { useAuth } from "../../../Context/AuthContext";
import { teacherService } from "../../../Services/teacherService";
import { lectureService } from "../../../Services/lectureService";
import CreateLectureModal from "../../../Components/Teacher/CreateLectureModal/CreateLectureModal";
import LectureTreeView from "../../../Components/Teacher/LectureTreeView/LectureTreeView";
import LectureDetailModal from "../../../Components/Teacher/LectureDetailModal/LectureDetailModal";
import ConfirmModal from "../../../Components/Common/ConfirmModal/ConfirmModal";
import SuccessModal from "../../../Components/Common/SuccessModal/SuccessModal";
import "./TeacherModuleLectureDetail.css";

export default function TeacherModuleLectureDetail() {
  const { moduleId } = useParams();
  const navigate = useNavigate();
  const { user, roles, isAuthenticated } = useAuth();
  
  const [module, setModule] = useState(null);
  const [lectures, setLectures] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Modals
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [lectureToUpdate, setLectureToUpdate] = useState(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [lectureToDelete, setLectureToDelete] = useState(null);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedLectureId, setSelectedLectureId] = useState(null);

  const isTeacher = roles.includes("Teacher") || user?.teacherSubscription?.isTeacher === true;

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const [moduleRes, lecturesRes] = await Promise.all([
        teacherService.getModuleById(moduleId),
        lectureService.getTeacherLectureTree(moduleId)
      ]);

      if (moduleRes.data?.success) setModule(moduleRes.data.data);
      if (lecturesRes.data?.success) setLectures(lecturesRes.data.data || []);
      
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [moduleId]);

  useEffect(() => {
    if (!isAuthenticated || !isTeacher) {
      navigate("/home");
      return;
    }
    fetchData();
  }, [isAuthenticated, isTeacher, navigate, fetchData]);

  const handleCreateSuccess = () => {
    setSuccessMessage("Tạo lecture thành công!");
    setShowSuccessModal(true);
    fetchData();
  };

  const handleAddChild = (parentLecture) => {
    const parentId = parentLecture.lectureId || parentLecture.LectureId;
    const parentTitle = parentLecture.title || parentLecture.Title;
    setLectureToUpdate({ 
      parentLectureId: parentId,
      parentTitle: parentTitle,
      _isChildCreation: true 
    });
    setShowCreateModal(true);
  };

  const handleViewLecture = (lecture) => {
    const lectureId = lecture.lectureId || lecture.LectureId;
    setSelectedLectureId(lectureId);
    setShowDetailModal(true);
  };

  const handleUpdateSuccess = () => {
    setSuccessMessage("Cập nhật lecture thành công!");
    setShowSuccessModal(true);
    fetchData();
  };

  const handleDeleteClick = (lecture) => {
    setLectureToDelete(lecture);
    setShowDeleteModal(true);
  };

  const confirmDelete = async () => {
    if (!lectureToDelete) return;
    try {
      const lectureId = lectureToDelete.lectureId || lectureToDelete.LectureId;
      const res = await lectureService.deleteLecture(lectureId);
      if (res.data?.success) {
        setSuccessMessage("Xóa lecture thành công!");
        setShowSuccessModal(true);
        setShowDeleteModal(false);
        fetchData();
      } else {
        alert("Xóa thất bại: " + res.data?.message);
      }
    } catch (err) {
      console.error(err);
      alert("Lỗi khi xóa lecture");
    }
  };

  return (
    <>
      <TeacherHeader />
      <div className="teacher-module-lecture-detail-container">
        <Container>
          <div className="lecture-management-header mb-4 mt-4">
            <div className="d-flex align-items-center justify-content-between">
              <div className="header-content">
                <h2 className="mb-1 fw-bold text-primary">Quản lý bài giảng</h2>
                <div className="text-muted d-flex align-items-center gap-2">
                  <span className="module-name">{module?.name || "Module"}</span>
                  {lectures.length > 0 && (
                    <Badge bg="secondary" className="px-2 py-1">
                      {lectures.length} {lectures.length === 1 ? 'bài giảng' : 'bài giảng'}
                    </Badge>
                  )}
                </div>
              </div>
              <Button 
                variant="primary" 
                className="create-lecture-btn"
                onClick={() => { setLectureToUpdate(null); setShowCreateModal(true); }}
              >
                <FaPlus className="me-2" /> Tạo Lecture mới
              </Button>
            </div>
          </div>

          {loading ? (
             <div className="text-center py-5"><div className="spinner-border text-primary"></div></div>
          ) : lectures.length === 0 ? (
             <div className="text-center py-5 bg-light rounded text-muted">
                 <p>Chưa có bài giảng nào trong module này.</p>
                 <Button variant="primary" onClick={() => { setLectureToUpdate(null); setShowCreateModal(true); }}>Tạo bài giảng đầu tiên</Button>
             </div>
          ) : (
             <LectureTreeView
               lectures={lectures}
               onAddChild={handleAddChild}
               onEdit={(lec) => { setLectureToUpdate(lec); setShowCreateModal(true); }}
               onDelete={handleDeleteClick}
               onView={handleViewLecture}
             />
          )}
        </Container>
      </div>

      <CreateLectureModal 
        show={showCreateModal}
        onClose={() => { setShowCreateModal(false); setLectureToUpdate(null); }}
        onSuccess={lectureToUpdate && !lectureToUpdate._isChildCreation ? handleUpdateSuccess : handleCreateSuccess}
        moduleId={moduleId}
        moduleName={module?.name || module?.Name}
        lectureToUpdate={lectureToUpdate}
      />

      <LectureDetailModal
        show={showDetailModal}
        onClose={() => { setShowDetailModal(false); setSelectedLectureId(null); }}
        lectureId={selectedLectureId}
        isAdmin={false}
      />

      <ConfirmModal 
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        onConfirm={confirmDelete}
        title="Xóa Lecture?"
        message="Hành động này không thể hoàn tác."
        confirmText="Xóa"
        cancelText="Hủy"
        type="danger"
      />

      <SuccessModal 
        isOpen={showSuccessModal}
        onClose={() => setShowSuccessModal(false)}
        title="Thành công"
        message={successMessage}
        autoClose={true}
        autoCloseDelay={1500}
      />
    </>
  );
}