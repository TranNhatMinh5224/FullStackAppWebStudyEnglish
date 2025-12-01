import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Spinner, Alert, Badge, ListGroup, Accordion, Modal } from 'react-bootstrap';
import { FaBook, FaUser, FaClock, FaCheck, FaLock, FaStar, FaArrowLeft, FaPlay, FaChevronDown } from 'react-icons/fa';
import { CourseService, EnrollmentService } from '../../services/api/user';
import { useAuth } from '../../contexts/AuthContext';
import { toast, ToastContainer } from 'react-toastify';
import './CourseDetailPage.css';

const CourseDetailPage = () => {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();

  // State management
  const [course, setCourse] = useState(null);
  const [loading, setLoading] = useState(true);
  const [enrolling, setEnrolling] = useState(false);
  const [error, setError] = useState(null);
  const [showEnrollModal, setShowEnrollModal] = useState(false);
  const [showClassCodeModal, setShowClassCodeModal] = useState(false);
  const [classCode, setClassCode] = useState('');

  // Fetch course details
  useEffect(() => {
    if (courseId) {
      fetchCourseDetails();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [courseId]);

  /**
   * Fetch course by ID
   */
  const fetchCourseDetails = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await CourseService.getCourseById(courseId);

      if (response.success && response.data) {
        setCourse(response.data);
      } else {
        setError(response.message || 'Không thể tải thông tin khóa học');
        toast.error('Không thể tải thông tin khóa học');
      }
    } catch (err) {
      console.error('Error fetching course details:', err);
      setError('Đã xảy ra lỗi khi tải thông tin khóa học');
      toast.error('Đã xảy ra lỗi khi tải thông tin khóa học');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle enrollment
   */
  const handleEnroll = async () => {
    if (!isLoggedIn) {
      toast.warning('Vui lòng đăng nhập để đăng ký khóa học');
      navigate('/login');
      return;
    }

    // Check if course is free or paid
    if (course.price && course.price > 0) {
      // Redirect to payment page
      navigate(`/payment/course/${courseId}`);
      return;
    }

    // Free course - direct enrollment
    setEnrolling(true);

    try {
      const response = await EnrollmentService.enrollInCourse({
        courseId: parseInt(courseId)
      });

      if (response.success) {
        toast.success('Đăng ký khóa học thành công!');
        setShowEnrollModal(false);
        // Refresh course details to update enrollment status
        await fetchCourseDetails();
      } else {
        toast.error(response.message || 'Đăng ký khóa học thất bại');
      }
    } catch (err) {
      console.error('Error enrolling:', err);
      toast.error('Đã xảy ra lỗi khi đăng ký khóa học');
    } finally {
      setEnrolling(false);
    }
  };

  /**
   * Handle join by class code
   */
  const handleJoinByClassCode = async () => {
    if (!classCode.trim()) {
      toast.warning('Vui lòng nhập mã lớp học');
      return;
    }

    setEnrolling(true);

    try {
      const response = await EnrollmentService.joinByClassCode(classCode);

      if (response.success) {
        toast.success('Tham gia lớp học thành công!');
        setShowClassCodeModal(false);
        setClassCode('');
        // Refresh course details
        await fetchCourseDetails();
      } else {
        toast.error(response.message || 'Mã lớp không hợp lệ');
      }
    } catch (err) {
      console.error('Error joining by class code:', err);
      toast.error('Đã xảy ra lỗi khi tham gia lớp học');
    } finally {
      setEnrolling(false);
    }
  };

  /**
   * Handle start learning
   */
  const handleStartLearning = () => {
    navigate(`/learning/course/${courseId}`);
  };

  // Loading state
  if (loading) {
    return (
      <Container className="py-5 text-center">
        <Spinner animation="border" role="status" variant="primary">
          <span className="visually-hidden">Đang tải...</span>
        </Spinner>
        <p className="mt-3">Đang tải thông tin khóa học...</p>
      </Container>
    );
  }

  // Error state
  if (error || !course) {
    return (
      <Container className="py-5">
        <Alert variant="danger">
          <Alert.Heading>Không tìm thấy khóa học</Alert.Heading>
          <p>{error || 'Khóa học không tồn tại hoặc đã bị xóa'}</p>
          <Button variant="outline-danger" onClick={() => navigate('/courses')}>
            Quay lại danh sách khóa học
          </Button>
        </Alert>
      </Container>
    );
  }

  return (
    <div className="course-detail-page">
      <ToastContainer position="top-right" autoClose={3000} />

      {/* Back Button */}
      <Container className="py-3">
        <Button variant="link" onClick={() => navigate('/courses')} className="ps-0">
          <FaArrowLeft className="me-2" />
          Quay lại danh sách khóa học
        </Button>
      </Container>

      {/* Course Header */}
      <div className="course-header bg-primary text-white py-5">
        <Container>
          <Row>
            <Col lg={8}>
              {/* Course Type & Status Badges */}
              <div className="mb-3">
                <Badge bg="light" text="dark" className="me-2">
                  {course.type === 'System' ? 'Khóa học chính thức' : 'Khóa học giáo viên'}
                </Badge>
                {course.isFeatured && (
                  <Badge bg="warning" text="dark" className="me-2">
                    <FaStar /> Nổi bật
                  </Badge>
                )}
                {course.isEnrolled && (
                  <Badge bg="success" className="me-2">
                    <FaCheck /> Đã đăng ký
                  </Badge>
                )}
              </div>

              {/* Course Title */}
              <h1 className="display-4 mb-3">{course.title}</h1>

              {/* Course Description */}
              <p className="lead mb-4">{course.description}</p>

              {/* Course Stats */}
              <div className="course-stats-header d-flex flex-wrap gap-4 mb-4">
                <div>
                  <FaUser className="me-2" />
                  <span>{course.enrollmentCount || 0} học viên</span>
                </div>
                <div>
                  <FaBook className="me-2" />
                  <span>{course.lessonCount || 0} bài học</span>
                </div>
                <div>
                  <FaClock className="me-2" />
                  <span>Cập nhật: {new Date(course.updatedAt || course.createdAt).toLocaleDateString('vi-VN')}</span>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="d-flex flex-wrap gap-3">
                {course.isEnrolled ? (
                  <Button 
                    variant="success" 
                    size="lg"
                    onClick={handleStartLearning}
                  >
                    <FaPlay className="me-2" />
                    Tiếp tục học
                  </Button>
                ) : (
                  <>
                    <Button 
                      variant="light" 
                      size="lg"
                      onClick={() => setShowEnrollModal(true)}
                      disabled={enrolling}
                    >
                      {enrolling ? (
                        <>
                          <Spinner animation="border" size="sm" className="me-2" />
                          Đang xử lý...
                        </>
                      ) : (
                        course.price && course.price > 0 ? (
                          <>Mua khóa học - {course.price.toLocaleString('vi-VN')}đ</>
                        ) : (
                          'Đăng ký miễn phí'
                        )
                      )}
                    </Button>

                    {course.classCode && (
                      <Button 
                        variant="outline-light" 
                        size="lg"
                        onClick={() => setShowClassCodeModal(true)}
                      >
                        Tham gia bằng mã lớp
                      </Button>
                    )}
                  </>
                )}
              </div>
            </Col>

            {/* Course Image */}
            <Col lg={4} className="mt-4 mt-lg-0">
              <Card className="shadow-lg border-0">
                <Card.Img
                  variant="top"
                  src={course.imageUrl || '/images/default-course.jpg'}
                  alt={course.title}
                  style={{ height: '300px', objectFit: 'cover' }}
                />
              </Card>
            </Col>
          </Row>
        </Container>
      </div>

      {/* Course Content */}
      <Container className="py-5">
        <Row>
          {/* Main Content */}
          <Col lg={8}>
            {/* Course Details */}
            <Card className="mb-4 shadow-sm">
              <Card.Header as="h4">
                <FaBook className="me-2" />
                Thông tin khóa học
              </Card.Header>
              <Card.Body>
                <h5>Mô tả chi tiết</h5>
                <p className="text-muted">
                  {course.description || 'Chưa có mô tả chi tiết cho khóa học này.'}
                </p>

                {course.teacher && (
                  <>
                    <h5 className="mt-4">Giảng viên</h5>
                    <p>
                      <FaUser className="me-2" />
                      {course.teacher.fullName || course.teacher.email}
                    </p>
                  </>
                )}
              </Card.Body>
            </Card>

            {/* Course Curriculum */}
            <Card className="mb-4 shadow-sm">
              <Card.Header as="h4">
                <FaChevronDown className="me-2" />
                Nội dung khóa học
              </Card.Header>
              <Card.Body>
                {course.lessons && course.lessons.length > 0 ? (
                  <Accordion defaultActiveKey="0">
                    {course.lessons.map((lesson, index) => (
                      <Accordion.Item key={lesson.lessonId} eventKey={index.toString()}>
                        <Accordion.Header>
                          <div className="d-flex justify-content-between w-100 pe-3">
                            <span>
                              <strong>Bài {index + 1}:</strong> {lesson.title}
                            </span>
                            <Badge bg="secondary">
                              {lesson.modules?.length || 0} modules
                            </Badge>
                          </div>
                        </Accordion.Header>
                        <Accordion.Body>
                          {lesson.description && (
                            <p className="text-muted small">{lesson.description}</p>
                          )}
                          
                          {lesson.modules && lesson.modules.length > 0 ? (
                            <ListGroup variant="flush">
                              {lesson.modules.map((module) => (
                                <ListGroup.Item key={module.moduleId}>
                                  <div className="d-flex justify-content-between align-items-center">
                                    <span>
                                      {module.contentType === 'Lecture' && <FaPlay className="me-2 text-primary" />}
                                      {module.contentType === 'FlashCard' && <FaBook className="me-2 text-success" />}
                                      {module.contentType === 'Assessment' && <FaCheck className="me-2 text-warning" />}
                                      {module.name}
                                    </span>
                                    <Badge bg="light" text="dark">
                                      {module.contentType}
                                    </Badge>
                                  </div>
                                </ListGroup.Item>
                              ))}
                            </ListGroup>
                          ) : (
                            <p className="text-muted small">Chưa có nội dung</p>
                          )}
                        </Accordion.Body>
                      </Accordion.Item>
                    ))}
                  </Accordion>
                ) : (
                  <p className="text-muted">Nội dung khóa học đang được cập nhật...</p>
                )}
              </Card.Body>
            </Card>
          </Col>

          {/* Sidebar */}
          <Col lg={4}>
            {/* Price Card */}
            <Card className="mb-4 shadow-sm sticky-top" style={{ top: '20px' }}>
              <Card.Body className="text-center">
                <h3 className="mb-3">
                  {course.price && course.price > 0 ? (
                    <>
                      <FaLock className="me-2 text-danger" />
                      {course.price.toLocaleString('vi-VN')}đ
                    </>
                  ) : (
                    <Badge bg="success" className="fs-5 py-2 px-3">
                      Miễn phí
                    </Badge>
                  )}
                </h3>

                {course.isEnrolled ? (
                  <Button 
                    variant="success" 
                    size="lg" 
                    className="w-100"
                    onClick={handleStartLearning}
                  >
                    <FaPlay className="me-2" />
                    Tiếp tục học
                  </Button>
                ) : (
                  <Button 
                    variant="primary" 
                    size="lg" 
                    className="w-100"
                    onClick={() => setShowEnrollModal(true)}
                    disabled={enrolling}
                  >
                    {enrolling ? 'Đang xử lý...' : 
                      course.price && course.price > 0 ? 'Mua ngay' : 'Đăng ký ngay'
                    }
                  </Button>
                )}

                {!course.isEnrolled && course.classCode && (
                  <Button 
                    variant="outline-primary" 
                    size="sm" 
                    className="w-100 mt-2"
                    onClick={() => setShowClassCodeModal(true)}
                  >
                    Có mã lớp học?
                  </Button>
                )}
              </Card.Body>
            </Card>

            {/* Course Info */}
            <Card className="shadow-sm">
              <Card.Header as="h5">Thông tin khóa học</Card.Header>
              <ListGroup variant="flush">
                <ListGroup.Item>
                  <strong>Trạng thái:</strong>{' '}
                  <Badge bg={course.status === 'Published' ? 'success' : 'warning'}>
                    {course.status === 'Published' ? 'Đang mở' : 'Nháp'}
                  </Badge>
                </ListGroup.Item>
                <ListGroup.Item>
                  <strong>Học viên:</strong> {course.enrollmentCount || 0} người
                </ListGroup.Item>
                <ListGroup.Item>
                  <strong>Số bài học:</strong> {course.lessonCount || 0} bài
                </ListGroup.Item>
                {course.maxStudent && course.maxStudent > 0 && (
                  <ListGroup.Item>
                    <strong>Giới hạn:</strong> {course.maxStudent} học viên
                  </ListGroup.Item>
                )}
                <ListGroup.Item>
                  <strong>Ngày tạo:</strong>{' '}
                  {new Date(course.createdAt).toLocaleDateString('vi-VN')}
                </ListGroup.Item>
              </ListGroup>
            </Card>
          </Col>
        </Row>
      </Container>

      {/* Enroll Confirmation Modal */}
      <Modal show={showEnrollModal} onHide={() => setShowEnrollModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Xác nhận đăng ký</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>Bạn có chắc chắn muốn đăng ký khóa học <strong>{course.title}</strong>?</p>
          {course.price && course.price > 0 && (
            <Alert variant="info">
              Khóa học này có phí <strong>{course.price.toLocaleString('vi-VN')}đ</strong>.
              Bạn sẽ được chuyển đến trang thanh toán.
            </Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowEnrollModal(false)}>
            Hủy
          </Button>
          <Button variant="primary" onClick={handleEnroll} disabled={enrolling}>
            {enrolling ? 'Đang xử lý...' : 'Xác nhận'}
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Class Code Modal */}
      <Modal show={showClassCodeModal} onHide={() => setShowClassCodeModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Tham gia bằng mã lớp</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>Nhập mã lớp học do giảng viên cung cấp:</p>
          <input
            type="text"
            className="form-control"
            placeholder="Nhập mã lớp..."
            value={classCode}
            onChange={(e) => setClassCode(e.target.value)}
          />
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowClassCodeModal(false)}>
            Hủy
          </Button>
          <Button variant="primary" onClick={handleJoinByClassCode} disabled={enrolling}>
            {enrolling ? 'Đang xử lý...' : 'Tham gia'}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default CourseDetailPage;
