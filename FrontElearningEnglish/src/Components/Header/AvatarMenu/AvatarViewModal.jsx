import React from "react";
import { Modal } from "react-bootstrap";
import { FaTimes } from "react-icons/fa";
import "./AvatarViewModal.css";

export default function AvatarViewModal({ show, onClose, avatarUrl, fullName }) {
    const displayAvatarUrl = avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'User')}&background=6366f1&color=fff&size=256`;

    return (
        <Modal
            show={show}
            onHide={onClose}
            centered
            className="avatar-view-modal"
            dialogClassName="avatar-view-modal-dialog"
        >
            <Modal.Body className="avatar-view-modal-body">
                <button
                    type="button"
                    className="avatar-view-close-btn"
                    onClick={onClose}
                    aria-label="Đóng"
                >
                    <FaTimes />
                </button>
                <div className="avatar-view-container">
                    <img
                        src={displayAvatarUrl}
                        alt={`Ảnh đại diện của ${fullName || 'người dùng'}`}
                        className="avatar-view-image"
                        onError={(e) => {
                            e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'User')}&background=6366f1&color=fff&size=256`;
                        }}
                    />
                </div>
            </Modal.Body>
        </Modal>
    );
}
