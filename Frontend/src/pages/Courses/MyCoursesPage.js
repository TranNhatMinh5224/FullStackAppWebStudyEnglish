import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Spinner, Alert, ProgressBar, Tabs, Tab } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaBook, FaPlay, FaClock, FaCheckCircle, FaChartLine } from 'react-icons/fa';
import { EnrollmentService } from '../../services/api/user';
import { useAuth } from '../../contexts/AuthContext';
import { toast, ToastContainer } from 'react-toastify';
import './MyCoursesPage.css';

const MyCoursesPage = () => {
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();

  // State management
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState('inProgress');

  // Fetch my courses on component mount
  useEffect(() => {
    if (!isLoggedIn) {
      toast.warning('Vui lòng đăng nhập để xem khóa học của bạn');
      navigate('/login');
      return;
    }
    fetchMyCourses();
  }, [isLoggedIn, navigate]);

  /**
   * Fetch enrolled courses
   */
  const fetchMyCourses = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await EnrollmentService.getMyEnrolledCourses();

      if (response.success && response.data) {
        setCourses(response.data);
      } else {
        setError(response.message || 'Không thể tải danh sách khóa học');
        toast.error('Không thể tải danh sách khóa học của bạn');
      }
    } catch (err) {
      console.error('Error fetching my courses:', err);
      setError('Đã xảy ra lỗi khi tải khóa học');
      toast.error('Đã xảy ra lỗi khi tải khóa học');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Filter courses by status
   */
  const getFilteredCourses = () => {
    switch (activeTab) {
      case 'inProgress':
        return courses.filter(c => c.progress > 0 && c.progress < 100);
      case 'completed':
        return courses.filter(c => c.progress === 100);
      case 'all':
      default:
        return courses;
    }
  };

  /**
   * Handle continue learning
   */
  const handleContinueLearning = (courseId) => {
    navigate(`/learning/course/${courseId}`);
  };

  /**
   * Handle view details
   */
  const handleViewDetails = (courseId) => {
    navigate(`/courses/${courseId}`);
  };

  /**
   * Render course card
   */
  const renderCourseCard = (course) => {
    const progress = course.progress || 0;
    const isCompleted = progress === 100;

    return (
      <Col key={course.courseId} xs={12} md={6} lg={4} className="mb-4">
        <Card className="my-course-card h-100 shadow-sm">
          {/* Course Image */}
          <div className="position-relative">
            <Card.Img
              variant="top"
              src={course.imageUrl || '/images/default-course.jpg'}
              alt={course.title}
              style={{ height: '200px', objectFit: 'cover' }}
            />
            {isCompleted && (
              <div className="completion-badge">
                <FaCheckCircle size={50} />
                <span>Hoàn thành</span>
              </div>
            )}
          </div>

          <Card.Body className="d-flex flex-column">
            {/* Course Title */}
            <Card.Title className="course-title mb-2">
              {course.title}
            </Card.Title>

            {/* Course Stats */}
            <div className="course-stats mb-3 text-muted small">
              <div className="mb-1">
                <FaBook className="me-2" />
                {course.lessonCount || 0} bài học
              </div>
              <div>
                <FaClock className="me-2" />
                Đăng ký: {new Date(course.enrolledAt).toLocaleDateString('vi-VN')}
              </div>
            </div>

            {/* Progress Bar */}
            <div className="mb-3">
              <div className="d-flex justify-content-between mb-1">
                <small className="text-muted">Tiến độ</small>
                <small className="text-primary fw-bold">{progress}%</small>
              </div>
              <ProgressBar 
                now={progress} 
                variant={isCompleted ? 'success' : 'primary'}
                style={{ height: '8px' }}
              />
            </div>

            {/* Action Buttons */}
            <div className="mt-auto">
              <Button
                variant={isCompleted ? 'success' : 'primary'}
                className="w-100 mb-2"
                onClick={() => handleContinueLearning(course.courseId)}
              >
                {isCompleted ? (
                  <><FaCheckCircle className="me-2" />Xem lại</>
                ) : (
                  <><FaPlay className="me-2" />Tiếp tục học</>
                )}
              </Button>
              <Button
                variant="outline-secondary"
                size="sm"
                className="w-100"
                onClick={() => handleViewDetails(course.courseId)}
              >
                Xem chi tiết
              </Button>
            </div>
          </Card.Body>
        </Card>
      </Col>
    );
  };

  // Loading state
  if (loading) {
    return (
      <Container className="py-5 text-center">
        <Spinner animation="border" role="status" variant="primary">
          <span className="visually-hidden">Đang tải...</span>
        </Spinner>
        <p className="mt-3">Đang tải khóa học của bạn...</p>
      </Container>
    );
  }

  // Error state
  if (error) {
    return (
      <Container className="py-5">
        <Alert variant="danger">
          <Alert.Heading>Có lỗi xảy ra</Alert.Heading>
          <p>{error}</p>
          <Button variant="outline-danger" onClick={fetchMyCourses}>
            Thử lại
          </Button>
        </Alert>
      </Container>
    );
  }

  const filteredCourses = getFilteredCourses();

  return (
    <div className="my-courses-page">
      <ToastContainer position="top-right" autoClose={3000} />

      {/* Page Header */}
      <div className="page-header bg-primary text-white py-5 mb-4">
        <Container>
          <h1 className="display-4 mb-3">
            <FaBook className="me-3" />
            Khóa học của tôi
          </h1>
          <p className="lead">
            Quản lý và tiếp tục học tập các khóa học đã đăng ký
          </p>
        </Container>
      </div>

      <Container>
        {courses.length === 0 ? (
          // Empty state
          <Alert variant="info" className="text-center py-5">
            <FaBook size={60} className="mb-3 text-muted" />
            <h4>Bạn chưa đăng ký khóa học nào</h4>
            <p className="mb-4">Khám phá các khóa học tiếng Anh chất lượng cao và bắt đầu học ngay!</p>
            <Button variant="primary" size="lg" onClick={() => navigate('/courses')}>
              Khám phá khóa học
            </Button>
          </Alert>
        ) : (
          <>
            {/* Statistics Cards */}
            <Row className="mb-4">
              <Col md={4}>
                <Card className="stat-card text-center shadow-sm">
                  <Card.Body>
                    <FaBook size={40} className="text-primary mb-2" />
                    <h3 className="mb-0">{courses.length}</h3>
                    <p className="text-muted mb-0">Tổng khóa học</p>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={4}>
                <Card className="stat-card text-center shadow-sm">
                  <Card.Body>
                    <FaChartLine size={40} className="text-warning mb-2" />
                    <h3 className="mb-0">
                      {courses.filter(c => c.progress > 0 && c.progress < 100).length}
                    </h3>
                    <p className="text-muted mb-0">Đang học</p>
                  </Card.Body>
                </Card>
              </Col>
              <Col md={4}>
                <Card className="stat-card text-center shadow-sm">
                  <Card.Body>
                    <FaCheckCircle size={40} className="text-success mb-2" />
                    <h3 className="mb-0">
                      {courses.filter(c => c.progress === 100).length}
                    </h3>
                    <p className="text-muted mb-0">Hoàn thành</p>
                  </Card.Body>
                </Card>
              </Col>
            </Row>

            {/* Course Tabs */}
            <Tabs
              activeKey={activeTab}
              onSelect={(k) => setActiveTab(k)}
              className="mb-4"
            >
              <Tab eventKey="inProgress" title={`Đang học (${courses.filter(c => c.progress > 0 && c.progress < 100).length})`}>
                <Row>
                  {filteredCourses.length > 0 ? (
                    filteredCourses.map(course => renderCourseCard(course))
                  ) : (
                    <Col>
                      <Alert variant="info">
                        Bạn chưa có khóa học nào đang học
                      </Alert>
                    </Col>
                  )}
                </Row>
              </Tab>

              <Tab eventKey="completed" title={`Hoàn thành (${courses.filter(c => c.progress === 100).length})`}>
                <Row>
                  {filteredCourses.length > 0 ? (
                    filteredCourses.map(course => renderCourseCard(course))
                  ) : (
                    <Col>
                      <Alert variant="info">
                        Bạn chưa hoàn thành khóa học nào
                      </Alert>
                    </Col>
                  )}
                </Row>
              </Tab>

              <Tab eventKey="all" title={`Tất cả (${courses.length})`}>
                <Row>
                  {filteredCourses.map(course => renderCourseCard(course))}
                </Row>
              </Tab>
            </Tabs>
          </>
        )}
      </Container>
    </div>
  );
};

export default MyCoursesPage;
