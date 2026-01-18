import React from "react";
import { Modal } from "react-bootstrap";
import { FaTimes } from "react-icons/fa";
import "./AssetImageViewModal.css";

export default function AssetImageViewModal({ show, onClose, imageUrl, assetName }) {
    if (!show || !imageUrl) return null;

    return (
        <Modal
            show={show}
            onHide={onClose}
            centered
            className="asset-image-view-modal"
            dialogClassName="asset-image-view-modal-dialog"
        >
            <Modal.Body className="asset-image-view-modal-body">
                <button
                    type="button"
                    className="asset-image-view-close-btn"
                    onClick={onClose}
                    aria-label="Đóng"
                >
                    <FaTimes />
                </button>
                <div className="asset-image-view-container">
                    <img
                        src={imageUrl}
                        alt={assetName || "Asset preview"}
                        className="asset-image-view-image"
                        onError={(e) => {
                            e.target.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300'%3E%3Crect fill='%23f3f4f6' width='400' height='300'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%239ca3af' font-family='Arial' font-size='16'%3EKhông thể tải ảnh%3C/text%3E%3C/svg%3E";
                        }}
                    />
                    {assetName && (
                        <div className="asset-image-view-name">
                            <p className="mb-0">{assetName}</p>
                        </div>
                    )}
                </div>
            </Modal.Body>
        </Modal>
    );
}
