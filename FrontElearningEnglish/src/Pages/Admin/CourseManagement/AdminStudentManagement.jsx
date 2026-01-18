import React, { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container, Pagination, Row, Col } from "react-bootstrap";
import "./AdminStudentManagement.css";
import Breadcrumb from "../../../Components/Common/Breadcrumb/Breadcrumb";
import { useAuth } from "../../../Context/AuthContext";
import { adminService } from "../../../Services/adminService";
import StudentDetailModal from "../../../Components/Teacher/StudentDetailModal/StudentDetailModal";
import AddStudentModal from "../../../Components/Teacher/AddStudentModal/AddStudentModal";
import SuccessModal from "../../../Components/Common/SuccessModal/SuccessModal";
import { FaPlus } from "react-icons/fa";

export default function AdminStudentManagement() {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const { roles, isAuthenticated } = useAuth();
  const [course, setCourse] = useState(null);
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [searchTerm] = useState("");
  
  // Modal states
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [showStudentDetailModal, setShowStudentDetailModal] = useState(false);
  const [showAddStudentModal, setShowAddStudentModal] = useState(false);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");

  const isAdmin = roles.includes("SuperAdmin") || roles.includes("ContentAdmin") || roles.includes("FinanceAdmin");

  const fetchCourseDetail = useCallback(async () => {
    try {
      const response = await adminService.getCourseContent(courseId);
      if (response.data?.success && response.data?.data) {
        setCourse(response.data.data);
      }
    } catch (err) {
      console.error("Error fetching course detail:", err);
    }
  }, [courseId]);

  const fetchStudents = useCallback(async () => {
    try {
      setLoading(true);
      setError("");

      const params = {
        pageNumber: currentPage,
        pageSize: pageSize,
        ...(searchTerm && { searchTerm: searchTerm })
      };

      const response = await adminService.getCourseStudents(courseId, params);
      
      if (response.data?.success && response.data?.data) {
        const data = response.data.data;
        const items = data.items || data.Items || [];
        const total = data.totalCount || data.TotalCount || 0;
        const pages = data.totalPages || data.TotalPages || 1;
        
        setStudents(items);
        setTotalCount(total);
        setTotalPages(pages);
      } else {
        setError(response.data?.message || response.data?.Message || "Không thể tải danh sách học viên");
      }
    } catch (err) {
      console.error("Error fetching students:", err);
      setError("Không thể tải danh sách học viên");
    } finally {
      setLoading(false);
    }
  }, [courseId, currentPage, pageSize, searchTerm]);

  useEffect(() => {
    if (!isAuthenticated || !isAdmin) {
      navigate("/home");
      return;
    }

    fetchCourseDetail();
    fetchStudents();
  }, [isAuthenticated, isAdmin, navigate, fetchCourseDetail, fetchStudents]);

  const handleStudentClick = async (studentId) => {
    try {
      const response = await adminService.getStudentDetail(courseId, studentId);
      if (response.data?.success && response.data?.data) {
        setSelectedStudent(response.data.data);
        setShowStudentDetailModal(true);
      }
    } catch (err) {
      console.error("Error fetching student detail:", err);
    }
  };

  const handleAddStudentSuccess = () => {
    setShowAddStudentModal(false);
    setSuccessMessage("Đã thêm học viên vào khóa học thành công!");
    setShowSuccessModal(true);
    fetchStudents();
    fetchCourseDetail(); // Refresh course to update totalStudents
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  if (!isAuthenticated || !isAdmin) {
    return null;
  }

  const courseTitle = course?.title || course?.Title || "Khóa học";

  return (
    <>
      <div className="admin-student-management-container">
        <div className="breadcrumb-section">
          <Breadcrumb
            items={[
              { label: "Quản lý khoá học", path: "/admin/courses" },
              { label: courseTitle, path: `/admin/courses/${courseId}` },
              { label: "Quản lý học viên", isCurrent: true }
            ]}
            showHomeIcon={false}
          />
        </div>

        <Container fluid className="student-management-content">
          <div className="student-management-header d-flex justify-content-between align-items-center flex-column flex-md-row gap-3">
            <h2 className="page-title">Quản lý học viên</h2>
            <button 
              className="add-student-btn d-flex align-items-center"
              onClick={() => setShowAddStudentModal(true)}
            >
              <FaPlus className="add-icon" />
              Thêm học viên
            </button>
          </div>

          {loading ? (
            <div className="loading-message">Đang tải danh sách học viên...</div>
          ) : error ? (
            <div className="error-message">{error}</div>
          ) : (
            <>
              <Row className="students-list g-4">
                {students.length > 0 ? (
                  students.map((student) => {
                    const studentId = student.userId || student.UserId;
                    const displayName = student.displayName || student.DisplayName || 
                      `${student.firstName || student.FirstName || ""} ${student.lastName || student.LastName || ""}`.trim();
                    const email = student.email || student.Email || "";
                    const avatarUrl = student.avatarUrl || student.AvatarUrl;
                    
                    return (
                      <Col key={studentId} xs={12} sm={6} md={4} lg={3}>
                        <div 
                          className="student-card d-flex align-items-center"
                          onClick={() => handleStudentClick(studentId)}
                        >
                          {avatarUrl && avatarUrl.trim() && (
                            <div className="student-avatar d-flex align-items-center justify-content-center">
                              <img src={avatarUrl} alt={displayName} />
                            </div>
                          )}
                          <div className="student-info">
                            <h3 className="student-name">{displayName || "Chưa có tên"}</h3>
                            <p className="student-email">{email}</p>
                          </div>
                          <div className="student-arrow">
                            <span>›</span>
                          </div>
                        </div>
                      </Col>
                    );
                  })
                ) : (
                  <Col xs={12}>
                    <div className="no-students-message">
                      {searchTerm ? "Không tìm thấy học viên nào" : "Chưa có học viên nào trong khóa học"}
                    </div>
                  </Col>
                )}
              </Row>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="pagination-wrapper d-flex flex-column align-items-center">
                  <Pagination>
                    <Pagination.First 
                      onClick={() => handlePageChange(1)} 
                      disabled={currentPage === 1}
                    />
                    <Pagination.Prev 
                      onClick={() => handlePageChange(currentPage - 1)} 
                      disabled={currentPage === 1}
                    />
                    
                    {[...Array(totalPages)].map((_, index) => {
                      const page = index + 1;
                      // Show first page, last page, current page, and pages around current
                      if (
                        page === 1 ||
                        page === totalPages ||
                        (page >= currentPage - 1 && page <= currentPage + 1)
                      ) {
                        return (
                          <Pagination.Item
                            key={page}
                            active={page === currentPage}
                            onClick={() => handlePageChange(page)}
                          >
                            {page}
                          </Pagination.Item>
                        );
                      } else if (page === currentPage - 2 || page === currentPage + 2) {
                        return <Pagination.Ellipsis key={page} />;
                      }
                      return null;
                    })}
                    
                    <Pagination.Next 
                      onClick={() => handlePageChange(currentPage + 1)} 
                      disabled={currentPage === totalPages}
                    />
                    <Pagination.Last 
                      onClick={() => handlePageChange(totalPages)} 
                      disabled={currentPage === totalPages}
                    />
                  </Pagination>
                  
                  <div className="pagination-info">
                    Hiển thị {((currentPage - 1) * pageSize) + 1} - {Math.min(currentPage * pageSize, totalCount)} / {totalCount} học viên
                  </div>
                </div>
              )}
            </>
          )}
        </Container>
      </div>

      {/* Student Detail Modal */}
      <StudentDetailModal
        show={showStudentDetailModal}
        onClose={() => setShowStudentDetailModal(false)}
        student={selectedStudent}
        courseId={courseId}
        onStudentRemoved={fetchStudents}
        isAdmin={true}
      />

      {/* Add Student Modal */}
      <AddStudentModal
        show={showAddStudentModal}
        onClose={() => setShowAddStudentModal(false)}
        onSuccess={handleAddStudentSuccess}
        courseId={courseId}
        isAdmin={true}
      />

      {/* Success Modal */}
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
