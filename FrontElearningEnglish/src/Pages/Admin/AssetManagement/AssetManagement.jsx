import React, { useState, useEffect } from "react";
import { Container, Button, Card } from "react-bootstrap";
import { FaPlus } from "react-icons/fa";
import AssetList from "../../../Components/Admin/AssetManagement/AssetList";
import AssetFormModal from "../../../Components/Admin/AssetManagement/AssetFormModal";
import ConfirmModal from "../../../Components/Common/ConfirmModal/ConfirmModal";
import SuccessModal from "../../../Components/Common/SuccessModal/SuccessModal";
import { assetFrontendService } from "../../../Services/assetFrontendService";
import { useAssets } from "../../../Context/AssetContext";
import { toast } from "react-toastify";
import "./AssetManagement.css";

export default function AssetManagement() {
    const [assets, setAssets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showFormModal, setShowFormModal] = useState(false);
    const [assetToEdit, setAssetToEdit] = useState(null);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [assetToDelete, setAssetToDelete] = useState(null);
    const [showSuccessModal, setShowSuccessModal] = useState(false);
    const [successMessage, setSuccessMessage] = useState("");
    const { refreshAssets } = useAssets();

    useEffect(() => {
        fetchAssets();
    }, []);

    const fetchAssets = async () => {
        try {
            setLoading(true);
            const response = await assetFrontendService.getAllAssets();
            if (response.data?.success) {
                setAssets(response.data.data || []);
            } else {
                toast.error("Không thể tải danh sách assets.");
            }
        } catch (error) {
            console.error("Error fetching assets:", error);
            toast.error("Lỗi kết nối.");
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = () => {
        setAssetToEdit(null);
        setShowFormModal(true);
    };

    const handleEdit = (asset) => {
        setAssetToEdit(asset);
        setShowFormModal(true);
    };

    const handleFormSuccess = (message) => {
        fetchAssets();
        refreshAssets(); // Refresh cache trong AssetContext
        if (message) {
            setSuccessMessage(message);
            setShowSuccessModal(true);
        }
    };

    const handleDeleteClick = (asset) => {
        setAssetToDelete(asset);
        setShowDeleteModal(true);
    };

    const confirmDelete = async () => {
        if (!assetToDelete) return;
        try {
            const id = assetToDelete.id || assetToDelete.Id;
            const response = await assetFrontendService.deleteAsset(id);
            if (response.data?.success) {
                setSuccessMessage("Xóa asset thành công!");
                setShowSuccessModal(true);
                fetchAssets();
                refreshAssets(); // Refresh cache trong AssetContext
            } else {
                toast.error(response.data?.message || "Không thể xóa asset.");
            }
        } catch (error) {
            console.error("Error deleting asset:", error);
            toast.error("Lỗi kết nối.");
        } finally {
            setShowDeleteModal(false);
            setAssetToDelete(null);
        }
    };

    return (
        <Container fluid className="py-4">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <div>
                    <h2 className="fw-bold text-primary m-0">Quản lý Assets Frontend</h2>
                    <p className="text-muted m-0">Quản lý logo và ảnh mặc định cho course, lesson, module</p>
                </div>
                <Button variant="primary" onClick={handleCreate} className="shadow-sm">
                    <FaPlus className="me-2" /> Thêm mới
                </Button>
            </div>

            <Card className="border-0 shadow-sm">
                <Card.Body className="p-0">
                    {loading ? (
                        <div className="text-center py-5">
                            <div className="spinner-border text-primary" role="status">
                                <span className="visually-hidden">Loading...</span>
                            </div>
                            <p className="mt-2 text-muted">Đang tải dữ liệu...</p>
                        </div>
                    ) : (
                        <AssetList
                            assets={assets}
                            onEdit={handleEdit}
                            onDelete={handleDeleteClick}
                        />
                    )}
                </Card.Body>
            </Card>

            <AssetFormModal
                show={showFormModal}
                onClose={() => setShowFormModal(false)}
                onSuccess={handleFormSuccess}
                assetToEdit={assetToEdit}
            />

            <ConfirmModal
                isOpen={showDeleteModal}
                onClose={() => setShowDeleteModal(false)}
                onConfirm={confirmDelete}
                title="Xác nhận xóa"
                message={`Bạn có chắc chắn muốn xóa asset "${assetToDelete?.nameImage || assetToDelete?.NameImage}"? Hành động này không thể hoàn tác.`}
                confirmText="Xóa"
                cancelText="Hủy"
                type="delete"
            />

            <SuccessModal
                isOpen={showSuccessModal}
                onClose={() => setShowSuccessModal(false)}
                title="Thành công"
                message={successMessage}
            />
        </Container>
    );
}
