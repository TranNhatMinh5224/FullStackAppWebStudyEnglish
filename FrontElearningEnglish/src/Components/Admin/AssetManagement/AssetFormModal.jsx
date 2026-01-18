import React, { useState, useEffect } from "react";
import { Modal, Button, Form, Row, Col } from "react-bootstrap";
import { assetFrontendService } from "../../../Services/assetFrontendService";
import { useEnums } from "../../../Context/EnumContext";
import FileUpload from "../../Common/FileUpload/FileUpload";
import { toast } from "react-toastify";
import "./AssetFormModal.css";

const ASSET_IMAGE_BUCKET = "assetsfrontend"; // Bucket name for asset images

export default function AssetFormModal({ show, onClose, onSuccess, assetToEdit }) {
    const { assetTypes } = useEnums();
    const [formData, setFormData] = useState({
        nameImage: "",
        assetType: 1, // Default: Logo (1=Logo, 4=DefaultCourse, 5=DefaultLesson)
    });
    const [, setImageUrl] = useState(null);
    const [imageTempKey, setImageTempKey] = useState(null);
    const [imageType, setImageType] = useState(null);
    const [existingImageUrl, setExistingImageUrl] = useState(null);
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});
    const [uploadingImage, setUploadingImage] = useState(false);

    useEffect(() => {
        if (assetToEdit) {
            setFormData({
                nameImage: assetToEdit.nameImage || assetToEdit.NameImage || "",
                assetType: assetToEdit.assetType !== undefined ? assetToEdit.assetType : (assetToEdit.AssetType !== undefined ? assetToEdit.AssetType : 1),
            });
            const existingUrl = assetToEdit.imageUrl || assetToEdit.ImageUrl;
            if (existingUrl) {
                setExistingImageUrl(existingUrl);
                setImageUrl(existingUrl);
            }
        } else {
            setFormData({
                nameImage: "",
                assetType: 1, // Default: Logo
            });
            setExistingImageUrl(null);
            setImageUrl(null);
        }
        setImageTempKey(null);
        setImageType(null);
        setErrors({});
    }, [assetToEdit, show]);

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData({
            ...formData,
            [name]: type === "checkbox" ? checked : (type === "number" ? (value === "" ? 0 : parseInt(value, 10)) : value)
        });
        // Clear error khi user đang gõ
        if (errors[name]) {
            setErrors({
                ...errors,
                [name]: ""
            });
        }
    };

    // FileUpload callbacks
    const handleImageUploadSuccess = (tempKey, fileType, previewUrl) => {
        setImageTempKey(tempKey);
        setImageType(fileType);
        setImageUrl(previewUrl);
        setErrors({ ...errors, image: null });
    };

    const handleImageRemove = () => {
        setImageUrl(null);
        setImageTempKey(null);
        setImageType(null);
        setExistingImageUrl(null);
    };

    const handleImageError = (errorMessage) => {
        setErrors({ ...errors, image: errorMessage });
    };

    const handleImageUploadingChange = (isUploading) => {
        setUploadingImage(isUploading);
    };

    const validateForm = () => {
        const newErrors = {};
        
        if (!formData.nameImage.trim()) {
            newErrors.nameImage = "Tên asset là bắt buộc";
        }

        if (!assetToEdit && !imageTempKey && !existingImageUrl) {
            newErrors.image = "Vui lòng upload ảnh";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        setLoading(true);

        try {
            let submitData;
            let response;
            
            if (assetToEdit) {
                // Update: gửi tất cả field (kể cả không thay đổi)
                const id = assetToEdit.id || assetToEdit.Id;
                submitData = {
                    id: Number(id), // Include Id in body for validation
                    nameImage: formData.nameImage.trim(),
                    assetType: Number(formData.assetType),
                };

                // Chỉ gửi ImageTempKey nếu có ảnh mới upload
                if (imageTempKey) {
                    submitData.imageTempKey = imageTempKey;
                    submitData.imageType = imageType;
                }
                
                response = await assetFrontendService.updateAsset(id, submitData);
            } else {
                // Create: gửi đầy đủ field
                submitData = {
                    nameImage: formData.nameImage.trim(),
                    assetType: Number(formData.assetType),
                };

                // Bắt buộc phải có ảnh khi tạo mới
                if (imageTempKey) {
                    submitData.imageTempKey = imageTempKey;
                    submitData.imageType = imageType;
                }
                
                response = await assetFrontendService.createAsset(submitData);
            }

            if (response.data?.success) {
                const message = assetToEdit ? "Cập nhật asset thành công!" : "Tạo asset mới thành công!";
                onClose();
                onSuccess(message);
            } else {
                toast.error(response.data?.message || "Có lỗi xảy ra.");
            }
        } catch (error) {
            console.error("Error saving asset:", error);
            toast.error(error.response?.data?.message || error.response?.data?.title || "Lỗi kết nối.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            show={show}
            onHide={onClose}
            centered
            size="lg"
            className="modal-modern asset-form-modal"
            dialogClassName="asset-form-modal-dialog"
        >
            <Modal.Header closeButton>
                <Modal.Title>{assetToEdit ? "Cập nhật Asset" : "Thêm Asset Mới"}</Modal.Title>
            </Modal.Header>
            <Form onSubmit={handleSubmit}>
                <Modal.Body>
                    <Row className="mb-3">
                        <Col md={12}>
                            <Form.Group>
                                <Form.Label>Tên asset <span className="text-danger">*</span></Form.Label>
                                <Form.Control
                                    type="text"
                                    name="nameImage"
                                    value={formData.nameImage}
                                    onChange={handleChange}
                                    required
                                    placeholder="VD: Logo Catalunya English"
                                    isInvalid={!!errors.nameImage}
                                />
                                {errors.nameImage && (
                                    <Form.Control.Feedback type="invalid">
                                        {errors.nameImage}
                                    </Form.Control.Feedback>
                                )}
                            </Form.Group>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={12}>
                            <Form.Group>
                                <Form.Label>Loại asset <span className="text-danger">*</span></Form.Label>
                                <Form.Select
                                    name="assetType"
                                    value={formData.assetType}
                                    onChange={handleChange}
                                    required
                                >
                                    {assetTypes && assetTypes.length > 0 ? (
                                        assetTypes.map((type) => (
                                            <option key={type.value} value={type.value}>
                                                {type.name}
                                            </option>
                                        ))
                                    ) : (
                                        <>
                                            <option value={1}>Logo</option>
                                            <option value={4}>DefaultCourse</option>
                                            <option value={5}>DefaultLesson</option>
                                        </>
                                    )}
                                </Form.Select>
                                <Form.Text className="text-muted">
                                    Chỉ quản lý: Logo, DefaultCourse, DefaultLesson
                                </Form.Text>
                            </Form.Group>
                        </Col>
                    </Row>

                    <Row className="mb-3">
                        <Col md={12}>
                            <Form.Group>
                                <Form.Label>
                                    Ảnh {!assetToEdit && <span className="text-danger">*</span>}
                                </Form.Label>
                                <FileUpload
                                    bucket={ASSET_IMAGE_BUCKET}
                                    accept="image/*"
                                    maxSize={5}
                                    existingUrl={existingImageUrl}
                                    onUploadSuccess={handleImageUploadSuccess}
                                    onRemove={handleImageRemove}
                                    onError={handleImageError}
                                    onUploadingChange={handleImageUploadingChange}
                                    label="Chọn ảnh hoặc kéo thả vào đây"
                                    hint="Hỗ trợ JPG, PNG, GIF (tối đa 5MB)"
                                />
                                {errors.image && (
                                    <div className="text-danger small mt-1">{errors.image}</div>
                                )}
                            </Form.Group>
                        </Col>
                    </Row>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={onClose} disabled={loading || uploadingImage}>
                        Hủy
                    </Button>
                    <Button variant="primary" type="submit" disabled={loading || uploadingImage}>
                        {loading ? "Đang lưu..." : uploadingImage ? "Đang upload..." : "Lưu"}
                    </Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}
