import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Spinner, Alert, Badge, Form, InputGroup } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaSearch, FaStar, FaUser, FaBook, FaLock, FaCheck } from 'react-icons/fa';
import { CourseService } from '../../services/api/user';
import { useAuth } from '../../contexts/AuthContext';
import { toast, ToastContainer } from 'react-toastify';
import './CoursesPage.css';

const CoursesPage = () => {
  const navigate = useNavigate();
  const { isLoggedIn } = useAuth();
  
  // State management
  const [courses, setCourses] = useState([]);
  const [filteredCourses, setFilteredCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [filterType, setFilterType] = useState('all'); // all, free, paid, enrolled

  // Fetch courses on component mount
  useEffect(() => {
    fetchCourses();
  }, []);

  // Apply filters when courses or filters change
  useEffect(() => {
    applyFilters();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [courses, searchKeyword, filterType]);

  /**
   * Fetch all system courses
   */
  const fetchCourses = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await CourseService.getSystemCourses();
      
      if (response.success && response.data) {
        setCourses(response.data);
        setFilteredCourses(response.data);
      } else {
        setError(response.message || 'Không thể tải danh sách khóa học');
        toast.error('Không thể tải danh sách khóa học');
      }
    } catch (err) {
      console.error('Error fetching courses:', err);
      setError('Đã xảy ra lỗi khi tải khóa học');
      toast.error('Đã xảy ra lỗi khi tải khóa học');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Apply search and filter
   */
  const applyFilters = () => {
    let filtered = [...courses];

    // Search filter
    if (searchKeyword.trim()) {
      filtered = filtered.filter(course =>
        course.title?.toLowerCase().includes(searchKeyword.toLowerCase()) ||
        course.description?.toLowerCase().includes(searchKeyword.toLowerCase())
      );
    }

    // Type filter
    switch (filterType) {
      case 'free':
        filtered = filtered.filter(course => !course.price || course.price === 0);
        break;
      case 'paid':
        filtered = filtered.filter(course => course.price && course.price > 0);
        break;
      case 'enrolled':
        filtered = filtered.filter(course => course.isEnrolled);
        break;
      case 'featured':
        filtered = filtered.filter(course => course.isFeatured);
        break;
      default:
        // 'all' - no additional filtering
        break;
    }

    setFilteredCourses(filtered);
  };

  /**
   * Handle course card click
   */
  const handleCourseClick = (courseId) => {
    navigate(`/courses/${courseId}`);
  };

  /**
   * Handle enroll button click
   */
  const handleEnrollClick = (e, courseId, course) => {
    e.stopPropagation(); // Prevent card click
    
    if (!isLoggedIn) {
      toast.warning('Vui lòng đăng nhập để đăng ký khóa học');
      navigate('/login');
      return;
    }

    if (course.isEnrolled) {
      navigate(`/my-courses/${courseId}`);
    } else {
      navigate(`/courses/${courseId}`);
    }
  };

  /**
   * Render course card
   */
  const renderCourseCard = (course) => (
    <Col key={course.courseId} xs={12} sm={6} lg={4} xl={3} className="mb-4">
      <Card 
        className="course-card h-100 shadow-sm"
        onClick={() => handleCourseClick(course.courseId)}
        style={{ cursor: 'pointer' }}
      >
        {/* Course Image */}
        <div className="course-image-wrapper position-relative">
          <Card.Img
            variant="top"
            src={course.imageUrl || '/images/default-course.jpg'}
            alt={course.title}
            className="course-image"
          />
          
          {/* Featured Badge */}
          {course.isFeatured && (
            <Badge bg="warning" className="position-absolute top-0 start-0 m-2">
              <FaStar /> Nổi bật
            </Badge>
          )}
          
          {/* Enrolled Badge */}
          {course.isEnrolled && (
            <Badge bg="success" className="position-absolute top-0 end-0 m-2">
              <FaCheck /> Đã đăng ký
            </Badge>
          )}
          
          {/* Price Badge */}
          <Badge 
            bg={course.price && course.price > 0 ? 'danger' : 'info'}
            className="position-absolute bottom-0 end-0 m-2"
          >
            {course.price && course.price > 0 ? (
              <><FaLock /> {course.price.toLocaleString('vi-VN')}đ</>
            ) : (
              'Miễn phí'
            )}
          </Badge>
        </div>

        <Card.Body className="d-flex flex-column">
          {/* Course Type */}
          <div className="mb-2">
            <Badge bg={course.type === 'System' ? 'primary' : 'secondary'} className="me-1">
              {course.type === 'System' ? 'Chính thức' : 'Giáo viên'}
            </Badge>
            <Badge bg={course.status === 'Published' ? 'success' : 'warning'}>
              {course.status === 'Published' ? 'Đang mở' : 'Nháp'}
            </Badge>
          </div>

          {/* Course Title */}
          <Card.Title className="course-title mb-2">
            {course.title}
          </Card.Title>

          {/* Course Description */}
          <Card.Text className="course-description text-muted small flex-grow-1">
            {course.description?.substring(0, 100)}
            {course.description?.length > 100 && '...'}
          </Card.Text>

          {/* Course Stats */}
          <div className="course-stats mb-3">
            <div className="d-flex justify-content-between text-muted small">
              <span>
                <FaUser className="me-1" />
                {course.enrollmentCount || 0} học viên
              </span>
              <span>
                <FaBook className="me-1" />
                {course.lessonCount || 0} bài học
              </span>
            </div>
          </div>

          {/* Action Button */}
          <Button
            variant={course.isEnrolled ? 'success' : 'primary'}
            className="w-100"
            onClick={(e) => handleEnrollClick(e, course.courseId, course)}
          >
            {course.isEnrolled ? (
              <><FaCheck className="me-2" />Tiếp tục học</>
            ) : (
              course.price && course.price > 0 ? 'Mua khóa học' : 'Đăng ký ngay'
            )}
          </Button>
        </Card.Body>
      </Card>
    </Col>
  );

  // Loading state
  if (loading) {
    return (
      <Container className="py-5 text-center">
        <Spinner animation="border" role="status" variant="primary">
          <span className="visually-hidden">Đang tải...</span>
        </Spinner>
        <p className="mt-3">Đang tải danh sách khóa học...</p>
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
          <Button variant="outline-danger" onClick={fetchCourses}>
            Thử lại
          </Button>
        </Alert>
      </Container>
    );
  }

  return (
    <div className="courses-page">
      <ToastContainer position="top-right" autoClose={3000} />
      
      {/* Page Header */}
      <div className="page-header bg-primary text-white py-5 mb-4">
        <Container>
          <h1 className="display-4 mb-3">
            <FaBook className="me-3" />
            Khóa học của chúng tôi
          </h1>
          <p className="lead">
            Khám phá các khóa học tiếng Anh chất lượng cao từ cơ bản đến nâng cao
          </p>
        </Container>
      </div>

      <Container>
        {/* Search and Filter Bar */}
        <Row className="mb-4">
          <Col md={6} lg={8}>
            <InputGroup>
              <InputGroup.Text>
                <FaSearch />
              </InputGroup.Text>
              <Form.Control
                type="text"
                placeholder="Tìm kiếm khóa học..."
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
              />
            </InputGroup>
          </Col>
          <Col md={6} lg={4}>
            <Form.Select
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
            >
              <option value="all">Tất cả khóa học</option>
              <option value="free">Miễn phí</option>
              <option value="paid">Có phí</option>
              <option value="featured">Nổi bật</option>
              {isLoggedIn && <option value="enrolled">Đã đăng ký</option>}
            </Form.Select>
          </Col>
        </Row>

        {/* Course Count */}
        <div className="mb-3">
          <h5>
            Tìm thấy <Badge bg="primary">{filteredCourses.length}</Badge> khóa học
          </h5>
        </div>

        {/* Course Grid */}
        {filteredCourses.length > 0 ? (
          <Row>
            {filteredCourses.map(course => renderCourseCard(course))}
          </Row>
        ) : (
          <Alert variant="info">
            <Alert.Heading>Không tìm thấy khóa học</Alert.Heading>
            <p>Không có khóa học nào phù hợp với tiêu chí tìm kiếm của bạn.</p>
            <Button variant="outline-info" onClick={() => {
              setSearchKeyword('');
              setFilterType('all');
            }}>
              Xóa bộ lọc
            </Button>
          </Alert>
        )}
      </Container>
    </div>
  );
};

export default CoursesPage;
