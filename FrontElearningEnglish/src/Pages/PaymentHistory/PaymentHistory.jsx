import React, { useEffect, useState } from "react";
import { Container, Pagination, Row, Col } from "react-bootstrap";
import "./PaymentHistory.css";
import MainHeader from "../../Components/Header/MainHeader";
import { paymentService } from "../../Services/paymentService";
import PaymentHistoryItem from "../../Components/PaymentHistory/PaymentHistoryItem/PaymentHistoryItem";
import PaymentDetailModal from "../../Components/PaymentHistory/PaymentDetailModal/PaymentDetailModal";

export default function PaymentHistory() {
    const [transactions, setTransactions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [selectedTransaction, setSelectedTransaction] = useState(null);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [loadingDetail, setLoadingDetail] = useState(false);
    
    // Pagination state
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize] = useState(20);
    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(0);

    useEffect(() => {
        const fetchTransactionHistory = async () => {
            try {
                setLoading(true);
                setError("");

                const response = await paymentService.getHistory(currentPage, pageSize);
                // Handle both camelCase and PascalCase responses
                const isSuccess = response.data?.Success !== false && response.data?.success !== false;
                const data = response.data?.data ?? response.data?.Data;
                
                if (isSuccess && data) {
                    // Handle paginated response
                    if (data.items || data.Items) {
                        const items = data.items || data.Items || [];
                        const total = data.totalCount || data.TotalCount || 0;
                        const pages = data.totalPages || data.TotalPages || 1;
                        
                        setTransactions(items);
                        setTotalCount(total);
                        setTotalPages(pages);
                    } else {
                        // Fallback: assume it's a direct array (backward compatibility)
                        const transactionsData = Array.isArray(data) ? data : [];
                        setTransactions(transactionsData);
                        setTotalCount(transactionsData.length);
                        setTotalPages(1);
                    }
                } else {
                    setError(
                        response.data?.Message || response.data?.message || "Không thể tải lịch sử thanh toán"
                    );
                }
            } catch (err) {
                console.error("Error fetching transaction history:", err);
                setError("Không thể tải dữ liệu lịch sử thanh toán");
            } finally {
                setLoading(false);
            }
        };

        fetchTransactionHistory();
    }, [currentPage, pageSize]);

    const handleItemClick = async (paymentId) => {
        try {
            setLoadingDetail(true);
            const response = await paymentService.getTransactionDetail(paymentId);
            if (response.data?.success && response.data?.data) {
                setSelectedTransaction(response.data.data);
                setShowDetailModal(true);
            } else {
                setError(response.data?.message || "Không thể tải chi tiết giao dịch");
            }
        } catch (err) {
            console.error("Error fetching transaction detail:", err);
            setError("Không thể tải chi tiết giao dịch");
        } finally {
            setLoadingDetail(false);
        }
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
        }
    };

    if (loading) {
        return (
            <>
                <MainHeader />
                <div className="payment-history-container">
                    <Container>
                        <div className="loading-message">Đang tải...</div>
                    </Container>
                </div>
            </>
        );
    }

    if (error && transactions.length === 0) {
        return (
            <>
                <MainHeader />
                <div className="payment-history-container">
                    <Container>
                        <div className="error-message">{error}</div>
                    </Container>
                </div>
            </>
        );
    }

    return (
        <>
            <MainHeader />
            <div className="payment-history-container">
                <Container>
                    <div className="payment-history-header">
                        <h1 className="payment-history-title">Lịch sử thanh toán</h1>
                        <p className="payment-history-subtitle">
                            Tổng số giao dịch: <span className="count-badge">{totalCount}</span>
                        </p>
                    </div>

                    {transactions.length === 0 ? (
                        <div className="empty-state">
                            <p>Bạn chưa có giao dịch nào.</p>
                            <p className="empty-state-hint">
                                Các giao dịch thanh toán của bạn sẽ hiển thị tại đây.
                            </p>
                        </div>
                    ) : (
                        <div className="payment-list-container">
                            <Row className="payment-list-header g-0">
                                <Col xs={12} md={5} lg={4} className="payment-header-column product-column d-flex align-items-center">
                                    <span>Sản phẩm</span>
                                </Col>
                                <Col xs={4} md={2} lg={2} className="payment-header-column amount-column d-none d-md-flex align-items-center">
                                    <span>Số tiền</span>
                                </Col>
                                <Col xs={4} md={2} lg={2} className="payment-header-column status-column d-flex align-items-center">
                                    <span>Trạng thái</span>
                                </Col>
                                <Col xs={4} md={3} lg={4} className="payment-header-column date-column d-none d-md-flex align-items-center">
                                    <span>Ngày thanh toán</span>
                                </Col>
                            </Row>

                            <div className="payment-list">
                                {transactions.map((transaction) => {
                                    const statusBadge = getStatusBadge(transaction.status || transaction.Status);
                                    return (
                                        <PaymentHistoryItem
                                            key={transaction.paymentId || transaction.PaymentId}
                                            transaction={transaction}
                                            statusBadge={statusBadge}
                                            formatDate={formatDate}
                                            onClick={() => handleItemClick(transaction.paymentId || transaction.PaymentId)}
                                        />
                                    );
                                })}
                            </div>

                            {/* Pagination */}
                            {totalPages > 1 && (
                                <div className="pagination-wrapper d-flex flex-column flex-md-row align-items-center gap-3">
                                    <Pagination>
                                        <Pagination.First 
                                            onClick={() => setCurrentPage(1)}
                                            disabled={currentPage === 1}
                                        />
                                        <Pagination.Prev 
                                            onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                                            disabled={currentPage === 1}
                                        />
                                        
                                        {/* Show page numbers */}
                                        {Array.from({ length: totalPages }, (_, i) => i + 1)
                                            .filter(page => {
                                                // Show first page, last page, current page, and pages around current
                                                return page === 1 || 
                                                       page === totalPages || 
                                                       (page >= currentPage - 1 && page <= currentPage + 1);
                                            })
                                            .map((page, index, array) => {
                                                // Add ellipsis if needed
                                                const showEllipsisBefore = index > 0 && array[index - 1] !== page - 1;
                                                return (
                                                    <React.Fragment key={page}>
                                                        {showEllipsisBefore && (
                                                            <Pagination.Ellipsis disabled />
                                                        )}
                                                        <Pagination.Item
                                                            active={page === currentPage}
                                                            onClick={() => setCurrentPage(page)}
                                                        >
                                                            {page}
                                                        </Pagination.Item>
                                                    </React.Fragment>
                                                );
                                            })}
                                        
                                        <Pagination.Next 
                                            onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                                            disabled={currentPage === totalPages}
                                        />
                                        <Pagination.Last 
                                            onClick={() => setCurrentPage(totalPages)}
                                            disabled={currentPage === totalPages}
                                        />
                                    </Pagination>
                                    
                                    <div className="pagination-info">
                                        Trang {currentPage} / {totalPages} ({totalCount} giao dịch)
                                    </div>
                                </div>
                            )}
                        </div>
                    )}
                </Container>
            </div>

            <PaymentDetailModal
                isOpen={showDetailModal}
                onClose={() => {
                    setShowDetailModal(false);
                    setSelectedTransaction(null);
                }}
                transaction={selectedTransaction}
                loading={loadingDetail}
            />
        </>
    );
}

