import React from "react";
import { Modal, Row, Col, Badge } from "react-bootstrap";
import "./PaymentDetailModal.css";

export default function PaymentDetailModal({ isOpen, onClose, transaction, loading }) {
    if (!transaction && !loading) return null;

    const formatAmount = (amount) => {
        if (!amount) return "0 đ";
        return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND",
        }).format(amount);
    };

    const formatDate = (dateString) => {
        if (!dateString) return "-";
        const date = new Date(dateString);
        return date.toLocaleDateString("vi-VN", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
        });
    };

    const getStatusBadge = (status) => {
        switch (status) {
            case 1: // Pending
                return { text: "Đang chờ", variant: "warning" };
            case 2: // Completed
                return { text: "Hoàn thành", variant: "info", customClass: "status-completed" };
            case 3: // Failed
                return { text: "Thất bại", variant: "danger" };
            default:
                return { text: "Không xác định", variant: "secondary" };
        };
    };

    const getProductTypeName = (productType) => {
        // Hỗ trợ cả số và string từ API
        const type = typeof productType === 'string' ? productType : productType?.toString();
        switch (type) {
            case "1":
            case "Course":
                return "Khóa học";
            case "2":
            case "TeacherPackage":
                return "Gói giáo viên";
            default:
                return "Không xác định";
        }
    };

    if (loading) {
        return (
            <Modal show={isOpen} onHide={onClose} size="lg" centered className="payment-detail-modal modal-modern" dialogClassName="payment-detail-modal-dialog">
                <Modal.Body>
                    <div className="text-center py-4">
                        <div className="spinner-border text-primary" role="status">
                            <span className="visually-hidden">Đang tải...</span>
                        </div>
                        <p className="mt-3">Đang tải chi tiết giao dịch...</p>
                    </div>
                </Modal.Body>
            </Modal>
        );
    }

    const statusBadge = getStatusBadge(transaction?.status || transaction?.Status);

    return (
        <Modal show={isOpen} onHide={onClose} size="lg" centered className="payment-detail-modal modal-modern" dialogClassName="payment-detail-modal-dialog">
            <Modal.Header closeButton>
                <Modal.Title>Chi tiết giao dịch</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div className="payment-detail-content">
                    <Row className="mb-3">
                        <Col md={4}>
                            <div className="detail-label">Trạng thái</div>
                        </Col>
                        <Col md={8}>
                            <Badge bg={statusBadge.variant} className={`status-badge-large ${statusBadge.customClass || ""}`}>
                                {statusBadge.text}
                            </Badge>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={4}>
                            <div className="detail-label">Sản phẩm</div>
                        </Col>
                        <Col md={8}>
                            <div className="detail-value">
                                {transaction?.productName || transaction?.ProductName || "N/A"}
                            </div>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={4}>
                            <div className="detail-label">Loại sản phẩm</div>
                        </Col>
                        <Col md={8}>
                            <div className="detail-value">
                                {getProductTypeName(transaction?.productType || transaction?.ProductType)}
                            </div>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={4}>
                            <div className="detail-label">Số tiền</div>
                        </Col>
                        <Col md={8}>
                            <div className="detail-value amount-value">
                                {formatAmount(transaction?.amount || transaction?.Amount)}
                            </div>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={4}>
                            <div className="detail-label">Phương thức thanh toán</div>
                        </Col>
                        <Col md={8}>
                            <div className="detail-value">
                                {transaction?.paymentMethod || transaction?.PaymentMethod || "N/A"}
                            </div>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={4}>
                            <div className="detail-label">Ngày tạo</div>
                        </Col>
                        <Col md={8}>
                            <div className="detail-value">
                                {formatDate(transaction?.createdAt || transaction?.CreatedAt)}
                            </div>
                        </Col>
                    </Row>

                    {transaction?.paidAt || transaction?.PaidAt ? (
                        <Row className="mb-3">
                            <Col md={4}>
                                <div className="detail-label">Ngày thanh toán</div>
                            </Col>
                            <Col md={8}>
                                <div className="detail-value">
                                    {formatDate(transaction?.paidAt || transaction?.PaidAt)}
                                </div>
                            </Col>
                        </Row>
                    ) : null}

                    {transaction?.userName || transaction?.UserName ? (
                        <>
                            <hr className="my-4" />
                            <Row className="mb-3">
                                <Col md={4}>
                                    <div className="detail-label">Người thanh toán</div>
                                </Col>
                                <Col md={8}>
                                    <div className="detail-value">
                                        {transaction?.userName || transaction?.UserName}
                                    </div>
                                </Col>
                            </Row>

                            <Row className="mb-3">
                                <Col md={4}>
                                    <div className="detail-label">Email</div>
                                </Col>
                                <Col md={8}>
                                    <div className="detail-value">
                                        {transaction?.userEmail || transaction?.UserEmail || "N/A"}
                                    </div>
                                </Col>
                            </Row>
                        </>
                    ) : null}
                </div>
            </Modal.Body>
            <Modal.Footer>
                <button className="btn btn-secondary" onClick={onClose}>
                    Đóng
                </button>
            </Modal.Footer>
        </Modal>
    );
}

