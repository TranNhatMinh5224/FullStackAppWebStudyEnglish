import React from "react";
import "./EnrollmentModal.css";

export default function EnrollmentModal({ 
    isOpen, 
    onClose, 
    course, 
    onStartNow, 
    onPayment,
    isProcessing = false
}) {
    if (!isOpen) return null;

    const isFree = course?.price === 0 || course?.price === null || course?.price === undefined;

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content enrollment-modal-content" onClick={(e) => e.stopPropagation()}>
                <h2 className="modal-title">Xác nhận đăng ký khóa học</h2>
                
                <div className="enrollment-modal-body">
                    <p className="enrollment-question">
                        Bạn có muốn đăng ký khóa học <strong>"{course?.title}"</strong> không?
                    </p>
                    
                    {!isFree && (
                        <div className="enrollment-price-info">
                            <span className="price-label">Giá khóa học:</span>
                            <span className="price-value">
                                {course?.price?.toLocaleString("vi-VN")}đ
                            </span>
                        </div>
                    )}
                </div>

                <div className="modal-buttons">
                    <button
                        type="button"
                        className="modal-btn modal-btn-cancel"
                        onClick={onClose}
                        disabled={isProcessing}
                    >
                        Hủy
                    </button>
                    {isFree ? (
                        <button
                            type="button"
                            className="modal-btn modal-btn-submit"
                            onClick={onStartNow}
                            disabled={isProcessing}
                        >
                            {isProcessing ? "Đang xử lý..." : "Bắt đầu ngay"}
                        </button>
                    ) : (
                        <button
                            type="button"
                            className="modal-btn modal-btn-submit"
                            onClick={onPayment}
                            disabled={isProcessing}
                        >
                            {isProcessing ? "Đang xử lý..." : "Thanh toán"}
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
}

