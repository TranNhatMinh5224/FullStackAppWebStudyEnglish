import React from "react";
import { Badge } from "react-bootstrap";
import "./PaymentHistoryItem.css";

export default function PaymentHistoryItem({ transaction, statusBadge, formatDate, onClick }) {
    const productName = transaction.productName || transaction.ProductName || "N/A";
    const amount = transaction.amount || transaction.Amount || 0;
    const paidAt = transaction.paidAt || transaction.PaidAt || transaction.createdAt || transaction.CreatedAt;

    const formatAmount = (amount) => {
        return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND",
        }).format(amount);
    };

    return (
        <div className="payment-item" onClick={onClick}>
            <div className="payment-column product-column">
                <span className="payment-product">{productName}</span>
            </div>
            <div className="payment-column amount-column">
                <span className="payment-amount">{formatAmount(amount)}</span>
            </div>
            <div className="payment-column status-column">
                <Badge bg={statusBadge.variant} className={`status-badge ${statusBadge.customClass || ""}`}>
                    {statusBadge.text}
                </Badge>
            </div>
            <div className="payment-column date-column">
                <span className="payment-date">{formatDate(paidAt)}</span>
            </div>
        </div>
    );
}

