// Components/Header/ProfileDropdown.jsx
import React from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Dropdown } from "react-bootstrap";
import { FaUserCircle } from "react-icons/fa";
import { useAuth } from "../../Context/AuthContext";
import { ROUTE_PATHS } from "../../Routes/Paths";

export default function ProfileDropdown() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user: authUser, roles, isGuest, logout } = useAuth();

  const isTeacher = roles.includes("Teacher") || authUser?.teacherSubscription?.isTeacher === true;
  // Check for any admin role: SuperAdmin, ContentAdmin, FinanceAdmin, or Admin
  const isAdmin = roles.some(role => {
    const roleName = typeof role === 'string' ? role : role?.name || role;
    return roleName === "SuperAdmin" || 
           roleName === "ContentAdmin" || 
           roleName === "FinanceAdmin" ||
           roleName === "Admin";
  });
  const isPremium = authUser?.teacherSubscription?.subscriptionType === "Premium" || authUser?.teacherSubscription?.packageLevel === "Premium";
  
  // Check if currently on teacher interface
  const isOnTeacherInterface = location.pathname.startsWith("/teacher");

  // Use user from auth context (includes avatarUrl)
  const user = authUser;

  return (
    <Dropdown className="profile-wrapper" align="end">
      <Dropdown.Toggle
        as="div"
        className="profile-trigger d-flex align-items-center"
        id="profile-dropdown"
      >
        <div className="avatar d-flex align-items-center justify-content-center">
          {!isGuest && user?.avatarUrl ? (
            <img src={user.avatarUrl} alt={`Avatar của ${user.fullName || 'người dùng'}`} className="avatar-img" />
          ) : (
            <FaUserCircle className="avatar-default-icon" />
          )}
        </div>
        {!isGuest && (
          <div className="user-info d-flex flex-column">
            <span className="name">
              {user?.fullName && user.fullName.length > 12
                ? `${user.fullName.substring(0, 12)}...`
                : user?.fullName}
            </span>
            <span className="role">
              {isOnTeacherInterface ? "Giáo viên" : "Học sinh"}
            </span>
          </div>
        )}
      </Dropdown.Toggle>

      <Dropdown.Menu className="profile-dropdown">
        {/* ===== GUEST ===== */}
        {isGuest && (
          <>
            <Dropdown.Item onClick={() => navigate("/login")}>
              Đăng nhập
            </Dropdown.Item>
            <Dropdown.Item onClick={() => navigate("/register")}>
              Đăng ký
            </Dropdown.Item>
          </>
        )}

        {/* ===== USER / TEACHER ===== */}
        {!isGuest && !isAdmin && (
          <>
            <Dropdown.Item onClick={() => navigate("/profile")}>
              Thông tin cá nhân
            </Dropdown.Item>

            <Dropdown.Item onClick={() => navigate(ROUTE_PATHS.PAYMENT_HISTORY)}>
              Lịch sử thanh toán
            </Dropdown.Item>

            {isTeacher && user?.teacherSubscription && (
              <>
                <Dropdown.Divider />
                <div className="teacher-package-info">
                  <div className="teacher-package-header d-flex justify-content-between align-items-center">
                    <span className="teacher-package-label">Gói giáo viên</span>
                    <span className={`teacher-package-badge ${isPremium ? "premium" : "basic"}`}>
                      {isPremium ? "Premium" : "Cơ bản"}
                    </span>
                  </div>
                  {user?.teacherSubscription?.expiresAt && (
                    <div className="teacher-package-expiry">
                      Hết hạn: {new Date(user.teacherSubscription.expiresAt).toLocaleDateString("vi-VN")}
                    </div>
                  )}
                </div>
                <Dropdown.Item
                  className="teacher-switch"
                  onClick={() => {
                    if (isOnTeacherInterface) {
                      navigate(ROUTE_PATHS.HOME);
                    } else {
                      navigate(ROUTE_PATHS.TEACHER_COURSE_MANAGEMENT);
                    }
                  }}
                >
                  {isOnTeacherInterface ? "Chuyển giao diện học sinh" : "Chuyển giao diện giáo viên"}
                </Dropdown.Item>
              </>
            )}

            <Dropdown.Divider />

            <Dropdown.Item
              className="logout"
              onClick={() => logout(navigate)}
            >
              Đăng xuất
            </Dropdown.Item>
          </>
        )}

        {/* ===== ADMIN ===== */}
        {isAdmin && (
          <>
            <Dropdown.Item onClick={() => navigate("/admin")}>
              Trang quản trị
            </Dropdown.Item>
            <Dropdown.Item
              className="logout"
              onClick={() => logout(navigate)}
            >
              Đăng xuất
            </Dropdown.Item>
          </>
        )}
      </Dropdown.Menu>
    </Dropdown>
  );
}
