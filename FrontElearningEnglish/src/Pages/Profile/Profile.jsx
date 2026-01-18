import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Row, Col } from "react-bootstrap";
import { FaUserCircle } from "react-icons/fa";
import "./Profile.css";
import MainHeader from "../../Components/Header/MainHeader";
import { useAuth } from "../../Context/AuthContext";
import { authService } from "../../Services/authService";
import AvatarMenu from "../../Components/Header/AvatarMenu/AvatarMenu";

export default function Profile() {
    const navigate = useNavigate();
    const { refreshUser } = useAuth();
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const response = await authService.getProfile();
                const userData = response.data.data;
                userData.fullName = userData.displayName || userData.fullName || `${userData.firstName} ${userData.lastName}`.trim();
                setUser(userData);
            } catch (error) {
                console.error("Error fetching profile:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchProfile();
    }, []);

    const handleEditProfile = () => {
        navigate("/profile/edit");
    };

    const handleChangePassword = () => {
        navigate("/profile/change-password");
    };

    // Refresh user data after avatar update
    const handleAvatarUpdate = async () => {
        try {
            const profileResponse = await authService.getProfile();
            const userData = profileResponse.data.data;
            userData.fullName = userData.displayName || userData.fullName || `${userData.firstName} ${userData.lastName}`.trim();
            setUser(userData);
            await refreshUser();
        } catch (error) {
            console.error("Error refreshing profile:", error);
        }
    };

    if (loading) {
        return (
            <>
                <MainHeader />
                <div className="profile-container">
                    <div className="profile-loading">Đang tải...</div>
                </div>
            </>
        );
    }

    if (!user) {
        return (
            <>
                <MainHeader />
                <div className="profile-container">
                    <div className="profile-error">Không thể tải thông tin người dùng</div>
                </div>
            </>
        );
    }

    return (
        <>
            <MainHeader />
            <div className="profile-container">
                    <div className="profile-header">
                        <h1>Thông tin User</h1>
                    </div>

                <div className="profile-card">
                    {/* Avatar Section */}
                    <div className="avatar-section d-flex justify-content-center">
                        <div className="avatar-wrapper">
                            <AvatarMenu
                                avatarUrl={user.avatarUrl}
                                fullName={user.fullName}
                                onAvatarUpdate={handleAvatarUpdate}
                                showAvatarOptions={true}
                            >
                                <div className="avatar-inner">
                                    {user.avatarUrl ? (
                                        <img src={user.avatarUrl} alt="Avatar" className="avatar-image" />
                                    ) : (
                                        <div className="avatar-placeholder d-flex align-items-center justify-content-center">
                                            <FaUserCircle className="avatar-default-icon-large" />
                                        </div>
                                    )}
                                </div>
                            </AvatarMenu>
                        </div>
                    </div>

                    {/* User Information */}
                    <div className="profile-info">
                        <Row className="info-row g-0">
                            <Col xs={12} md={4} className="mb-2 mb-md-0">
                                <label className="d-flex align-items-center">Last name:</label>
                            </Col>
                            <Col xs={12} md={8}>
                                <div className="info-value d-flex align-items-center">{user.lastName || "-"}</div>
                            </Col>
                        </Row>

                        <Row className="info-row g-0">
                            <Col xs={12} md={4} className="mb-2 mb-md-0">
                                <label>First name:</label>
                            </Col>
                            <Col xs={12} md={8}>
                                <div className="info-value">{user.firstName || "-"}</div>
                            </Col>
                        </Row>

                        <Row className="info-row g-0">
                            <Col xs={12} md={4} className="mb-2 mb-md-0">
                                <label>Email:</label>
                            </Col>
                            <Col xs={12} md={8}>
                                <div className="info-value">{user.email || "-"}</div>
                            </Col>
                        </Row>

                        <Row className="info-row g-0">
                            <Col xs={12} md={4} className="mb-2 mb-md-0">
                                <label>Số điện thoại:</label>
                            </Col>
                            <Col xs={12} md={8}>
                                <div className="info-value">{user.phoneNumber || "-"}</div>
                            </Col>
                        </Row>
                    </div>

                    {/* Action Buttons */}
                    <div className="profile-actions d-flex justify-content-end flex-column flex-md-row">
                        <button className="btn-change-password" onClick={handleChangePassword}>
                            Đổi mật khẩu
                        </button>
                        <button className="btn-edit-profile" onClick={handleEditProfile}>
                            Thay đổi thông tin
                        </button>
                    </div>
                </div>
            </div>
        </>
    );
}

