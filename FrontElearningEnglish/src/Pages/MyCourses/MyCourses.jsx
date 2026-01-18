import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Container, Pagination, Row, Col } from "react-bootstrap";
import "./MyCourses.css";
import "../../Components/Home/WelcomeSection/WelcomeSection.css"; // Import WelcomeSection CSS
import MainHeader from "../../Components/Header/MainHeader";
import JoinClassModal from "../../Components/Common/JoinClassModal/JoinClassModal";
import NotificationModal from "../../Components/Common/NotificationModal/NotificationModal";
import SuccessModal from "../../Components/Common/SuccessModal/SuccessModal";
import SuggestedCourseCard from "../../Components/Home/SuggestedCourseCard/SuggestedCourseCard";
import AccountUpgradeSection from "../../Components/Home/AccountUpgradeSection/AccountUpgradeSection";
import { FaPlus } from "react-icons/fa";
import { enrollmentService } from "../../Services/enrollmentService";
import { useAuth } from "../../Context/AuthContext";
import { useAssets } from "../../Context/AssetContext";
import LoginRequiredModal from "../../Components/Common/LoginRequiredModal/LoginRequiredModal";

export default function MyCourses() {
    const navigate = useNavigate();
    const { isAuthenticated, user } = useAuth();
    const { getDefaultCourseImage } = useAssets();
    const [selectedPackage, setSelectedPackage] = useState(null);
    
    // Enrolled courses state với pagination
    const [enrolledCourses, setEnrolledCourses] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize] = useState(20); // Hiển thị 20 courses mỗi trang (4 columns x 5 rows)
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(0);
    const [refreshTrigger, setRefreshTrigger] = useState(0); // Trigger để refresh danh sách
    
    // Modal states
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [showLoginModal, setShowLoginModal] = useState(false);
    
    // Notification states
    const [showSuccessModal, setShowSuccessModal] = useState(false);
    const [successMessage, setSuccessMessage] = useState("");
    const [showInfoModal, setShowInfoModal] = useState(false);
    const [infoMessage, setInfoMessage] = useState("");
    const [showErrorModal, setShowErrorModal] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");

    // Fetch enrolled courses với pagination
    useEffect(() => {
        const fetchEnrolledCourses = async () => {
            try {
                setLoading(true);
                setError("");

                // Lấy danh sách khóa học đã đăng ký với phân trang
                const registeredRes = await enrollmentService.getMyCourses(currentPage, pageSize);
                
                // Handle both camelCase and PascalCase responses
                const isSuccess = registeredRes.data?.Success !== false && registeredRes.data?.success !== false;
                const data = registeredRes.data?.data ?? registeredRes.data?.Data;
                
                if (isSuccess && data) {
                    // Handle paginated response
                    if (data.items || data.Items) {
                        const items = data.items || data.Items || [];
                        const total = data.totalCount || data.TotalCount || 0;
                        const pages = data.totalPages || data.TotalPages || 1;
                        
                        const mappedCourses = items.map((course) => ({
                            id: course.courseId || course.CourseId,
                            courseId: course.courseId || course.CourseId,
                            title: course.title || course.Title,
                            imageUrl: (course.imageUrl || course.ImageUrl) && (course.imageUrl || course.ImageUrl).trim() !== ""
                                ? (course.imageUrl || course.ImageUrl)
                                : getDefaultCourseImage(),
                            price: course.price || course.Price || 0,
                        }));
                        
                        setEnrolledCourses(mappedCourses);
                        setTotalCount(total);
                        setTotalPages(pages);
                    } else {
                        // Fallback: assume it's a direct array (backward compatibility)
                        const registeredData = Array.isArray(data) ? data : [];
                        const mappedCourses = registeredData.map((course) => ({
                            id: course.courseId || course.CourseId,
                            courseId: course.courseId || course.CourseId,
                            title: course.title || course.Title,
                            imageUrl: (course.imageUrl || course.ImageUrl) && (course.imageUrl || course.ImageUrl).trim() !== ""
                                ? (course.imageUrl || course.ImageUrl)
                                : getDefaultCourseImage(),
                            price: course.price || course.Price || 0,
                        }));
                        
                        setEnrolledCourses(mappedCourses);
                        setTotalCount(mappedCourses.length);
                        setTotalPages(1);
                    }
                } else {
                    setError(
                        registeredRes.data?.Message || registeredRes.data?.message || "Không thể tải danh sách khóa học"
                    );
                }
            } catch (err) {
                console.error("Error fetching enrolled courses:", err);
                setError("Không thể tải danh sách khóa học");
            } finally {
                setLoading(false);
            }
        };

        if (isAuthenticated) {
            fetchEnrolledCourses();
        } else {
            setEnrolledCourses([]);
            setLoading(false);
        }
    }, [currentPage, pageSize, isAuthenticated, refreshTrigger]); // Thêm refreshTrigger vào dependencies

    const handlePageChange = (pageNumber) => {
        setCurrentPage(pageNumber);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleJoinClass = async (classCode) => {
        try {
            const response = await enrollmentService.joinByClassCode({ classCode });

            if (response.data?.success) {
                // Join thành công
                setIsModalOpen(false);
                
                // Refresh courses list ngay lập tức
                setRefreshTrigger(prev => prev + 1);
                
                // Reset về page 1 để thấy khóa học mới
                if (currentPage !== 1) {
                    setCurrentPage(1);
                }

                // Hiển thị SuccessModal đẹp cho thông báo thành công sau khi danh sách đã cập nhật
                setTimeout(() => {
                    setSuccessMessage("Đã tham gia lớp học thành công! Khóa học đã được thêm vào danh sách của bạn.");
                    setShowSuccessModal(true);
                }, 300);
            } else {
                // Join thất bại - kiểm tra xem có phải "đã đăng ký rồi" không
                const errorMessage = response.data?.message || "";
                const isAlreadyEnrolled = errorMessage.includes("đã đăng ký") || errorMessage.includes("đã tham gia");

                if (isAlreadyEnrolled) {
                    // Nếu đã đăng ký rồi, chỉ thông báo
                    setIsModalOpen(false);
                    setInfoMessage("Bạn đã tham gia khóa học này rồi! Kiểm tra trong danh sách bên dưới.");
                    setShowInfoModal(true);
                } else {
                    // Các lỗi khác
                    setErrorMessage(errorMessage || "Không thể tham gia lớp học. Vui lòng kiểm tra lại mã lớp.");
                    setShowErrorModal(true);
                }
            }
        } catch (error) {
            console.error("Error joining class:", error);
            const errorMessage = error.response?.data?.message || "Không thể tham gia lớp học. Vui lòng kiểm tra lại mã lớp.";

            // Kiểm tra xem có phải "đã đăng ký rồi" không
            const isAlreadyEnrolled = errorMessage.includes("đã đăng ký") || errorMessage.includes("đã tham gia");

            if (isAlreadyEnrolled) {
                setIsModalOpen(false);
                setInfoMessage("Bạn đã tham gia khóa học này rồi! Kiểm tra trong danh sách bên dưới.");
                setShowInfoModal(true);
            } else {
                setErrorMessage(errorMessage);
                setShowErrorModal(true);
            }
        }
    };


    // Account upgrade handlers
    const handlePackageHover = (teacherPackageId) => {
        setSelectedPackage(teacherPackageId);
    };

    const handlePackageLeave = () => {
        setSelectedPackage(null);
    };

    const handleUpgradeClick = (e, teacherPackageId, packageType) => {
        e.stopPropagation();

        if (!isAuthenticated) {
            setShowLoginModal(true);
            return;
        }

        // Kiểm tra nếu user đã là giáo viên
        const teacherSubscription = user?.teacherSubscription || user?.TeacherSubscription;
        const isTeacher = teacherSubscription?.isTeacher || teacherSubscription?.IsTeacher;
        
        if (isTeacher === true) {
            setInfoMessage("Gói giáo viên hiện tại của bạn đang hoạt động, vui lòng chờ đến khi hết hạn để kích hoạt gói giáo viên mới!");
            setShowInfoModal(true);
            return;
        }

        navigate(`/payment?packageId=${teacherPackageId}&package=${packageType}`);
    };

    const displayName = user?.fullName || "bạn";

    return (
        <>
            <MainHeader />
            <div className="my-courses-container">
                <Container>
                    {/* Welcome Section giống trang chủ */}
                    <Row className="welcome-section g-3 g-md-4 align-items-center mb-4">
                        <Col xs={12} lg={7} className="welcome-section__left d-flex flex-column justify-content-center align-items-start">
                            <h1>Chào mừng trở lại, {displayName}</h1>
                            <p>Hãy tiếp tục hành trình học tiếng Anh nào.</p>
                        </Col>
                        <Col xs={12} lg={5} className="welcome-section__right d-flex align-items-center justify-content-end">
                            <button
                                className="join-class-btn d-flex align-items-center"
                                onClick={() => setIsModalOpen(true)}
                            >
                                <FaPlus />
                                Nhập mã lớp học
                            </button>
                        </Col>
                    </Row>

                    {/* Main Content: 2 columns layout - Bootstrap Grid */}
                    <Row className="g-4 mt-3">
                        {/* Left: Enrolled Courses Section */}
                        <Col xs={12} lg={8} className="suggested-courses-section">
                            {loading ? (
                                <div className="loading-message">Đang tải khóa học...</div>
                            ) : error ? (
                                <div className="error-message">{error}</div>
                            ) : enrolledCourses.length > 0 ? (
                                <>
                                    <h2>Khóa học của tôi</h2>
                                    <Row className="g-3 g-md-4">
                                        {enrolledCourses.map((course, index) => (
                                            <Col key={course.id || index} xs={12} sm={6} lg={4} xl={3}>
                                                <SuggestedCourseCard
                                                    course={course}
                                                    isEnrolled={true} // Tất cả đều đã đăng ký
                                                    showEnrolledBadge={true} // Hiển thị badge "Đã tham gia"
                                                />
                                            </Col>
                                        ))}
                                    </Row>
                                    
                                    {/* Pagination */}
                                    {totalPages > 1 && (
                                        <div className="d-flex justify-content-between align-items-center flex-column flex-md-row gap-3 gap-md-4 mt-4 mt-md-5 pt-4 pt-md-5 border-top">
                                            <div className="pagination-info text-center text-md-start">
                                                Trang {currentPage} / {totalPages} ({totalCount} khóa học)
                                            </div>
                                            <Pagination className="custom-pagination">
                                                <Pagination.First 
                                                    onClick={() => handlePageChange(1)}
                                                    disabled={currentPage === 1}
                                                />
                                                <Pagination.Prev 
                                                    onClick={() => handlePageChange(currentPage - 1)}
                                                    disabled={currentPage === 1}
                                                />
                                                
                                                {[...Array(totalPages)].map((_, index) => {
                                                    const pageNumber = index + 1;
                                                    // Chỉ hiển thị một số trang xung quanh trang hiện tại
                                                    if (
                                                        pageNumber === 1 ||
                                                        pageNumber === totalPages ||
                                                        (pageNumber >= currentPage - 2 && pageNumber <= currentPage + 2)
                                                    ) {
                                                        return (
                                                            <Pagination.Item
                                                                key={pageNumber}
                                                                active={pageNumber === currentPage}
                                                                onClick={() => handlePageChange(pageNumber)}
                                                            >
                                                                {pageNumber}
                                                            </Pagination.Item>
                                                        );
                                                    } else if (
                                                        pageNumber === currentPage - 3 ||
                                                        pageNumber === currentPage + 3
                                                    ) {
                                                        return <Pagination.Ellipsis key={pageNumber} />;
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
                                        </div>
                                    )}
                                </>
                            ) : (
                                <div className="no-courses-message">Chưa có khóa học đã đăng ký</div>
                            )}
                        </Col>

                        {/* Right: Account Upgrade Section */}
                        <Col xs={12} lg={4}>
                            <AccountUpgradeSection
                                selectedPackage={selectedPackage}
                                onPackageHover={handlePackageHover}
                                onPackageLeave={handlePackageLeave}
                                onUpgradeClick={handleUpgradeClick}
                            />
                        </Col>
                    </Row>
                </Container>
            </div>

            {/* Modals */}
            <JoinClassModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onJoin={handleJoinClass}
            />

            <LoginRequiredModal
                isOpen={showLoginModal}
                onClose={() => setShowLoginModal(false)}
            />

            {/* Notification Modals */}
            <SuccessModal
                isOpen={showSuccessModal}
                onClose={() => setShowSuccessModal(false)}
                title="Thành công"
                message={successMessage}
                autoClose={true}
                autoCloseDelay={2000}
            />

            <NotificationModal
                isOpen={showInfoModal}
                onClose={() => setShowInfoModal(false)}
                type="info"
                message={infoMessage}
                autoClose={true}
                autoCloseDelay={3000}
            />

            <NotificationModal
                isOpen={showErrorModal}
                onClose={() => setShowErrorModal(false)}
                type="error"
                message={errorMessage}
                autoClose={true}
                autoCloseDelay={3000}
            />
        </>
    );
}

