import React, { useState, useRef } from "react";
import { Dropdown } from "react-bootstrap";
import { FaUser, FaImage } from "react-icons/fa";
import { useAuth } from "../../../Context/AuthContext";
import { authService } from "../../../Services/authService";
import { fileService } from "../../../Services/fileService";
import AvatarViewModal from "./AvatarViewModal";
import "./AvatarMenu.css";

export default function AvatarMenu({ avatarUrl, fullName, children, onAvatarUpdate, showAvatarOptions = false }) {
    const [showViewModal, setShowViewModal] = useState(false);
    const [showMenu, setShowMenu] = useState(false);
    const [uploading, setUploading] = useState(false);
    const fileInputRef = useRef(null);
    const { refreshUser } = useAuth();

    const AVATAR_BUCKET = "avatars";

    const handleSeeProfilePicture = () => {
        setShowViewModal(true);
    };

    const handleChooseProfilePicture = () => {
        fileInputRef.current?.click();
    };

    const handleFileChange = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;

        // Validate file type
        if (!file.type.startsWith("image/")) {
            alert("Vui lòng chọn file ảnh");
            return;
        }

        // Validate file size (max 2MB)
        if (file.size > 2 * 1024 * 1024) {
            alert("Kích thước ảnh không được vượt quá 2MB");
            return;
        }

        setUploading(true);

        try {
            // 1. Upload file to temp
            const uploadResponse = await fileService.uploadTempFile(file, AVATAR_BUCKET, "temp");
            const tempKey = uploadResponse.data.data.tempKey;

            if (!tempKey) {
                throw new Error("Upload file thất bại");
            }

            // 2. Update avatar with temp key
            const updateResponse = await authService.updateAvatar({
                avatarTempKey: tempKey,
            });

            if (updateResponse.data.success) {
                // Refresh user in context (to update avatar in header)
                await refreshUser();
                // Call callback if provided (for Profile page to refresh user data)
                if (onAvatarUpdate) {
                    await onAvatarUpdate();
                }
                alert("Cập nhật ảnh đại diện thành công!");
            }
        } catch (error) {
            console.error("Error uploading avatar:", error);
            alert(error.response?.data?.message || "Có lỗi xảy ra khi upload avatar");
        } finally {
            setUploading(false);
            // Reset file input
            if (fileInputRef.current) {
                fileInputRef.current.value = "";
            }
        }
    };

    // Nếu không hiển thị menu avatar options, chỉ render children (dùng ở header)
    if (!showAvatarOptions) {
        return <>{children}</>;
    }

    // Hiển thị menu avatar options (dùng ở trang Profile)
    return (
        <>
            <Dropdown 
                className="avatar-menu-wrapper" 
                align="end"
                show={showMenu}
                onToggle={(isOpen) => setShowMenu(isOpen)}
            >
                <Dropdown.Toggle
                    as="div"
                    className="avatar-menu-trigger"
                    id="avatar-menu-dropdown"
                    disabled={uploading}
                    onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        setShowMenu(!showMenu);
                    }}
                >
                    {children}
</Dropdown.Toggle>

                <Dropdown.Menu 
                    className="avatar-menu"
                    onClick={(e) => {
                        e.stopPropagation();
                    }}
                >
                    <Dropdown.Item 
                        onClick={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            setShowMenu(false);
                            handleSeeProfilePicture();
                        }}
                    >
                        <FaUser className="avatar-menu-icon" />
                        <span>Xem ảnh đại diện</span>
                    </Dropdown.Item>
                    <Dropdown.Item 
                        onClick={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            setShowMenu(false);
                            handleChooseProfilePicture();
                        }} 
                        disabled={uploading}
                    >
                        <FaImage className="avatar-menu-icon" />
                        <span>{uploading ? "Đang tải..." : "Chọn ảnh đại diện"}</span>
                    </Dropdown.Item>
                </Dropdown.Menu>
            </Dropdown>

            <input
                type="file"
                ref={fileInputRef}
                accept="image/*"
                style={{ display: "none" }}
                onChange={handleFileChange}
            />

            <AvatarViewModal
                show={showViewModal}
                onClose={() => setShowViewModal(false)}
                avatarUrl={avatarUrl}
                fullName={fullName}
            />
        </>
    );
}
