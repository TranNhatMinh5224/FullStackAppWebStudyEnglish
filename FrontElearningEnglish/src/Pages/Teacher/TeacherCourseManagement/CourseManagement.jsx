import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Pagination, Row, Col } from "react-bootstrap";
import "./CourseManagement.css";
import TeacherHeader from "../../../Components/Header/TeacherHeader";
import { useAuth } from "../../../Context/AuthContext";
import { teacherService } from "../../../Services/teacherService";
import { teacherPackageService } from "../../../Services/teacherPackageService";
import { FaPlus } from "react-icons/fa";
import { ROUTE_PATHS } from "../../../Routes/Paths";
import CreateCourseModal from "../../../Components/Teacher/CreateCourseModal/CreateCourseModal";
import CourseLimitModal from "../../../Components/Common/CourseLimitModal/CourseLimitModal";
import SuccessModal from "../../../Components/Common/SuccessModal/SuccessModal";
import TeacherCourseCard from "../../../Components/Teacher/TeacherCourseCard/TeacherCourseCard";

export default function CourseManagement() {
  const { user, roles, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  
  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  // Modal state
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showLimitModal, setShowLimitModal] = useState(false);
  const [maxCoursesLimit, setMaxCoursesLimit] = useState(0);
  const [showSuccessModal, setShowSuccessModal] = useState(false);

  const isTeacher = roles.includes("Teacher") || user?.teacherSubscription?.isTeacher === true;

  // Redirect from /teacher to /teacher/course-management
  useEffect(() => {
    if (location.pathname === ROUTE_PATHS.TEACHER) {
      navigate(ROUTE_PATHS.TEACHER_COURSE_MANAGEMENT, { replace: true });
    }
  }, [location.pathname, navigate]);

  useEffect(() => {
    if (!isAuthenticated || !isTeacher) {
      navigate("/home");
      return;
    }

    fetchCourses();
  }, [isAuthenticated, isTeacher, navigate, currentPage]);

  const fetchCourses = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await teacherService.getMyCourses({
        pageNumber: currentPage,
        pageSize: pageSize,
      });

      if (response.data?.success && response.data?.data) {
        const data = response.data.data;
        const items = data.items || data || [];
        const total = data.totalCount || data.totalItems || 0;
        const pages = data.totalPages || Math.ceil(total / pageSize);

        setCourses(items);
        setTotalCount(total);
        setTotalPages(pages);
      } else {
        setCourses([]);
        setTotalCount(0);
        setTotalPages(0);
      }
    } catch (err) {
      console.error("Error fetching courses:", err);
      setError("Không thể tải danh sách khóa học");
      setCourses([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCourse = async () => {
    // Check course limit before opening modal
    try {
      // Get package info
      const packageResponse = await teacherPackageService.getAll();
      const userPackageLevel = user?.teacherSubscription?.packageLevel;
      
      if (packageResponse.data?.success && packageResponse.data?.data && userPackageLevel) {
        const packages = packageResponse.data.data;
        const levelMap = {
          "Basic": 0,
          "Standard": 1,
          "Premium": 2,
          "Professional": 3
        };
        const expectedLevel = levelMap[userPackageLevel];
        
        const matchedPackage = packages.find(
          (pkg) => {
            const pkgLevel = pkg.level !== undefined ? pkg.level : (pkg.Level !== undefined ? pkg.Level : null);
            return (
              pkgLevel === expectedLevel ||
              pkgLevel?.toString() === userPackageLevel ||
              (typeof pkgLevel === "string" && pkgLevel === userPackageLevel)
            );
          }
        );

        if (matchedPackage) {
          const maxCourses = matchedPackage.maxCourses || 0;
          
          // Get current course count
          const coursesResponse = await teacherService.getMyCourses({
            pageNumber: 1,
            pageSize: 1000,
          });

          if (coursesResponse.data?.success && coursesResponse.data?.data) {
            const data = coursesResponse.data.data;
            const total = data.totalCount || data.totalItems || (data.items?.length || 0);
            
            if (total >= maxCourses) {
              // Show limit modal
              setMaxCoursesLimit(maxCourses);
              setShowLimitModal(true);
              return;
            }
          }
        }
      }
    } catch (error) {
      console.error("Error checking course limit:", error);
    }
    
    setShowCreateModal(true);
  };

  const handleCreateSuccess = () => {
    // Close create modal
    setShowCreateModal(false);
    // Show success modal
    setShowSuccessModal(true);
    // Refresh courses list
    fetchCourses();
    // Reset to first page
    setCurrentPage(1);
  };

  const displayName = user?.fullName || "giáo viên";

  if (!isAuthenticated || !isTeacher) {
    return null;
  }

  return (
    <>
      <TeacherHeader />

      <div className="course-management-container">
        <div className="course-management-header d-flex justify-content-between align-items-center flex-column flex-md-row gap-4">
          <div className="welcome-section">
            <h1>Chào mừng giáo viên {displayName}!</h1>
          </div>
          <button className="create-course-btn d-flex align-items-center" onClick={handleCreateCourse}>
            <FaPlus className="create-course-icon d-flex align-items-center justify-content-center" />
            <span>Tạo lớp học</span>
          </button>
        </div>

        <div className="courses-section">
          {loading ? (
            <div className="loading-message">Đang tải danh sách khóa học...</div>
          ) : error ? (
            <div className="error-message">{error}</div>
          ) : courses.length > 0 ? (
            <>
              <Row className="courses-grid g-4">
                {courses.map((course) => (
                  <Col
                    key={course.courseId || course.id}
                    xs={12}
                    sm={6}
                    md={4}
                    lg={3}
                    xl={2}
                  >
                    <TeacherCourseCard course={course} />
                  </Col>
                ))}
              </Row>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="pagination-wrapper d-flex justify-content-center">
                  <Pagination>
                    <Pagination.Prev
                      disabled={currentPage === 1}
                      onClick={() => currentPage > 1 && setCurrentPage(currentPage - 1)}
                    />
                    {Array.from({ length: totalPages }, (_, i) => i + 1)
                      .filter(page => {
                        return (
                          page === 1 ||
                          page === totalPages ||
                          (page >= currentPage - 1 && page <= currentPage + 1)
                        );
                      })
                      .map((page, index, array) => {
                        const showEllipsisBefore =
                          index > 0 && array[index - 1] !== page - 1;
                        return (
                          <React.Fragment key={page}>
                            {showEllipsisBefore && <Pagination.Ellipsis disabled />}
                            <Pagination.Item
                              active={page === currentPage}
                              onClick={() => setCurrentPage(page)}
                            >
                              {page}
                            </Pagination.Item>
                          </React.Fragment>
                        );
                      })}
                    <Pagination.Next
                      disabled={currentPage === totalPages}
                      onClick={() =>
                        currentPage < totalPages && setCurrentPage(currentPage + 1)
                      }
                    />
                  </Pagination>
                </div>
              )}
            </>
          ) : (
            <div className="empty-state">
              <p>Bạn chưa có khóa học nào. Hãy tạo khóa học đầu tiên!</p>
            </div>
          )}
        </div>
      </div>

      {/* Create Course Modal */}
      <CreateCourseModal
        show={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onSuccess={handleCreateSuccess}
      />

      {/* Course Limit Modal */}
      <CourseLimitModal
        isOpen={showLimitModal}
        onClose={() => setShowLimitModal(false)}
        maxCourses={maxCoursesLimit}
        onUpgrade={() => {
          setShowLimitModal(false);
          navigate("/home");
        }}
      />

      {/* Success Modal */}
      <SuccessModal
        isOpen={showSuccessModal}
        onClose={() => setShowSuccessModal(false)}
        title="Tạo khóa học thành công"
        message="Khóa học của bạn đã được tạo thành công!"
        autoClose={true}
        autoCloseDelay={1500}
      />
    </>
  );
}

