import React, { useState, useEffect, useRef } from "react";
import { Modal, Button } from "react-bootstrap";
import { fileService } from "../../Services/fileService";
import { validateFile } from "../../Utils/fileValidationConfig";
import { FaFileUpload, FaTimes } from "react-icons/fa";
import "./CreateWithImageModal.css";

const CreateWithImageModal = ({
  show,
  onClose,
  onSuccess,
  title,
  isUpdateMode = false,
  bucketName,
  children,
  onSubmit,
  isFormValid,
  submitting,
  uploadingImage,
  setUploadingImage,
  modalClass = "create-with-image-modal",
  dialogClass = "create-with-image-modal-dialog"
}) => {
  const fileInputRef = useRef(null);

  // Image upload state
  const [selectedImage, setSelectedImage] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [imageTempKey, setImageTempKey] = useState(null);
  const [imageType, setImageType] = useState(null);
  const [existingImageUrl, setExistingImageUrl] = useState(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const [errors, setErrors] = useState({});

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setSelectedImage(null);
      setImagePreview(null);
      setImageTempKey(null);
      setImageType(null);
      setExistingImageUrl(null);
      setErrors({});
      setIsDragOver(false);
    }
  }, [show]);

  // Add paste event listener
  useEffect(() => {
    if (show) {
      document.addEventListener('paste', handlePaste);
      return () => {
        document.removeEventListener('paste', handlePaste);
      };
    }
  }, [show, uploadingImage]);

  const handleImageClick = () => {
    fileInputRef.current?.click();
  };

  const handleImageChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Use centralized validation
    const validation = validateFile(file, { bucketName });
    if (!validation.isValid) {
      setErrors({ ...errors, image: validation.error });
      return;
    }

    setUploadingImage(true);
    setErrors({ ...errors, image: null });

    try {
      const response = await fileService.uploadTempImage(file, bucketName);
      if (response.data?.success) {
        setImageTempKey(response.data.data.tempKey);
        setImageType(response.data.data.imageType);
        setImagePreview(response.data.data.url);
        setSelectedImage(file);
      } else {
        throw new Error(response.data?.message || "Upload failed");
      }
    } catch (error) {
      console.error("Image upload error:", error);
      setErrors({ ...errors, image: error.response?.data?.message || error.message || "Upload ảnh thất bại" });
    } finally {
      setUploadingImage(false);
    }
  };

  const handleRemoveImage = () => {
    setSelectedImage(null);
    setImagePreview(null);
    setImageTempKey(null);
    setImageType(null);
    setExistingImageUrl(null);
    setErrors({ ...errors, image: null });
  };

  const handleDragOver = (e) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = (e) => {
    e.preventDefault();
    setIsDragOver(false);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragOver(false);

    const files = e.dataTransfer.files;
    if (files.length > 0) {
      const file = files[0];
      if (file.type.startsWith('image/')) {
        handleImageChange({ target: { files: [file] } });
      }
    }
  };

  const handlePaste = (e) => {
    if (uploadingImage) return;

    const items = e.clipboardData?.items;
    if (!items) return;

    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      if (item.type.indexOf('image') !== -1) {
        e.preventDefault();
        const file = item.getAsFile();
        if (file) {
          handleImageChange({ target: { files: [file] } });
        }
        break;
      }
    }
  };

  const handleFormSubmit = async (e) => {
    e.preventDefault();

    // Call the custom submit handler with image data
    await onSubmit({
      imageTempKey,
      imageType,
      imagePreview,
      existingImageUrl
    });
  };

  return (
    <Modal
      show={show}
      onHide={onClose}
      centered
      size="xl"
      backdrop="static"
      className={modalClass}
      dialogClassName={dialogClass}
      style={{ zIndex: 1050 }}
    >
      <Modal.Header className="p-0 border-0">
        <div className="w-100 position-relative">
          <div className="form-top-banner text-white mb-0">
            <h4 className="mb-1 fw-bold">{title}</h4>
          </div>
          <button type="button" className="btn-close" aria-label="Close" onClick={onClose} />
        </div>
      </Modal.Header>

      <Modal.Body>
        <form onSubmit={handleFormSubmit} className="p-0">
          {children({
            imagePreview,
            handleImageClick,
            handleRemoveImage,
            handleDragOver,
            handleDragLeave,
            handleDrop,
            fileInputRef,
            handleImageChange,
            isDragOver,
            uploadingImage,
            errors,
            setErrors
          })}
        </form>
      </Modal.Body>

      <Modal.Footer>
        <Button variant="outline-secondary" onClick={onClose} disabled={submitting}>
          Hủy
        </Button>
        <Button
          variant="primary"
          className="btn-primary"
          onClick={handleFormSubmit}
          disabled={!isFormValid || submitting || uploadingImage}
        >
          {submitting ? (isUpdateMode ? "Đang cập nhật..." : "Đang tạo...") : (isUpdateMode ? "Lưu & Cập nhật" : "Tạo")}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default CreateWithImageModal;