import React, { useState } from "react";
import { Table, Button, Badge } from "react-bootstrap";
import { FaEdit, FaTrash, FaEye, FaEyeSlash } from "react-icons/fa";
import AssetImageViewModal from "./AssetImageViewModal";

export default function AssetList({ assets, onEdit, onDelete }) {
    const [viewImage, setViewImage] = useState({ show: false, imageUrl: "", assetName: "" });

    const getAssetTypeBadge = (assetType) => {
        const typeMap = {
            1: { name: "Logo", color: "success" },
            4: { name: "DefaultCourse", color: "primary" },
            5: { name: "DefaultLesson", color: "info" },
        };
        const type = typeMap[assetType] || { name: "Unknown", color: "secondary" };
        return <Badge bg={type.color}>{type.name}</Badge>;
    };

    const handleImageClick = (imageUrl, assetName) => {
        if (imageUrl) {
            setViewImage({ show: true, imageUrl, assetName });
        }
    };

    return (
        <div className="table-responsive">
            <Table hover className="align-middle shadow-sm bg-white rounded">
                <thead className="bg-light text-secondary">
                    <tr>
                        <th className="py-3 ps-4 border-0 rounded-start">ID</th>
                        <th className="py-3 border-0">Preview</th>
                        <th className="py-3 border-0">Tên</th>
                        <th className="py-3 border-0">Loại</th>
                        <th className="py-3 border-0 rounded-end text-center">Hành động</th>
                    </tr>
                </thead>
                <tbody>
                    {assets && assets.length > 0 ? (
                        assets.map((asset) => {
                            const id = asset.id || asset.Id;
                            const nameImage = asset.nameImage || asset.NameImage || "";
                            const imageUrl = asset.imageUrl || asset.ImageUrl || "";
                            const assetType = asset.assetType || asset.AssetType || 1;

                            return (
                                <tr key={id} className="border-bottom">
                                    <td className="ps-4 fw-bold text-muted">#{id}</td>
                                    <td>
                                        {imageUrl ? (
                                            <img
                                                src={imageUrl}
                                                alt={nameImage}
                                                style={{
                                                    width: "60px",
                                                    height: "60px",
                                                    objectFit: "cover",
                                                    borderRadius: "8px",
                                                    border: "1px solid #e5e7eb",
                                                    cursor: "pointer",
                                                    transition: "transform 0.2s ease"
                                                }}
                                                onClick={() => handleImageClick(imageUrl, nameImage)}
                                                onMouseEnter={(e) => {
                                                    e.target.style.transform = "scale(1.05)";
                                                }}
                                                onMouseLeave={(e) => {
                                                    e.target.style.transform = "scale(1)";
                                                }}
                                                onError={(e) => {
                                                    e.target.style.display = "none";
                                                    e.target.nextSibling.style.display = "flex";
                                                }}
                                                title="Click để xem chi tiết"
                                            />
                                        ) : (
                                            <div
                                                style={{
                                                    width: "60px",
                                                    height: "60px",
                                                    backgroundColor: "#f3f4f6",
                                                    borderRadius: "8px",
                                                    display: "flex",
                                                    alignItems: "center",
                                                    justifyContent: "center",
                                                    color: "#9ca3af",
                                                    fontSize: "12px"
                                                }}
                                            >
                                                No Image
                                            </div>
                                        )}
                                    </td>
                                    <td className="fw-medium text-primary">{nameImage}</td>
                                    <td>{getAssetTypeBadge(assetType)}</td>
                                    <td className="text-center">
                                        <Button
                                            variant="outline-primary"
                                            size="sm"
                                            className="me-2 rounded-circle"
                                            style={{ width: '32px', height: '32px', padding: 0 }}
                                            onClick={() => onEdit(asset)}
                                            title="Chỉnh sửa"
                                        >
                                            <FaEdit size={14} />
                                        </Button>
                                        <Button
                                            variant="outline-danger"
                                            size="sm"
                                            className="rounded-circle"
                                            style={{ width: '32px', height: '32px', padding: 0 }}
                                            onClick={() => onDelete(asset)}
                                            title="Xóa"
                                        >
                                            <FaTrash size={14} />
                                        </Button>
                                    </td>
                                </tr>
                            );
                        })
                    ) : (
                        <tr>
                            <td colSpan="5" className="text-center py-5 text-muted">
                                <div className="d-flex flex-column align-items-center">
                                    <i className="fa fa-image fa-3x mb-3 opacity-50"></i>
                                    <p>Chưa có asset nào.</p>
                                </div>
                            </td>
                        </tr>
                    )}
                </tbody>
            </Table>
            
            <AssetImageViewModal
                show={viewImage.show}
                onClose={() => setViewImage({ show: false, imageUrl: "", assetName: "" })}
                imageUrl={viewImage.imageUrl}
                assetName={viewImage.assetName}
            />
        </div>
    );
}
