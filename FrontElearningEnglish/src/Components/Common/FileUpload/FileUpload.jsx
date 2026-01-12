import React, { useState, useRef, useEffect, useCallback } from "react";
import { Button } from "react-bootstrap";
import { FaFileUpload, FaTimes, FaImage, FaMusic, FaVideo } from "react-icons/fa";
import { fileService } from "../../../Services/fileService";
import "./FileUpload.css";

/**
 * FileUpload Component - Reusable file upload component
 * 
 * @param {Object} props
 * @param {string} props.bucket - S3 bucket name (required)
 * @param {string} props.accept - Accept file types (e.g., "image/*", "audio/*", "video/*")
 * @param {number} props.maxSize - Max file size in MB (default: 5)
 * @param {string} props.existingUrl - Existing file URL to display
 * @param {Function} props.onUploadSuccess - Callback when upload succeeds: (tempKey, fileType, previewUrl, fileSize, duration) => void
 * @param {Function} props.onRemove - Callback when file is removed: () => void
 * @param {Function} props.onError - Callback when error occurs: (errorMessage) => void
 * @param {string} props.label - Label text (default: "Chọn file hoặc kéo thả vào đây")
 * @param {string} props.hint - Hint text (default: "Hỗ trợ Paste (Ctrl+V) từ Clipboard")
 * @param {boolean} props.enablePaste - Enable paste from clipboard (default: true)
 * @param {string} props.previewClassName - Custom class for preview container
 * @param {boolean} props.showPreview - Show preview (default: true)
 */
export default function FileUpload({
    bucket,
    accept = "image/*",
    maxSize = 5,
    existingUrl = null,
    onUploadSuccess,
    onRemove,
    onError,
    label = "Chọn file hoặc kéo thả vào đây",
    hint = "Hỗ trợ Paste (Ctrl+V) từ Clipboard",
    enablePaste = true,
    previewClassName = "",
    showPreview = true,
}) {
    const fileInputRef = useRef(null);
    const [preview, setPreview] = useState(existingUrl || null);
    const [uploading, setUploading] = useState(false);
    const [uploadProgress, setUploadProgress] = useState(0);
    const [error, setError] = useState(null);
    const [isDragging, setIsDragging] = useState(false);

    // Determine file type icon
    const getFileTypeIcon = () => {
        if (accept.includes("image")) return <FaImage />;
        if (accept.includes("audio")) return <FaMusic />;
        if (accept.includes("video")) return <FaVideo />;
        return <FaFileUpload />;
    };

    // Validate file
    const validateFile = useCallback((file) => {
        if (!file) return "Không có file được chọn";

        // Check file type
        if (accept !== "*" && accept !== "*/*") {
            const acceptedTypes = accept.split(",").map(t => t.trim());
            const isValidType = acceptedTypes.some(type => {
                if (type.endsWith("/*")) {
                    const baseType = type.split("/")[0];
                    return file.type.startsWith(`${baseType}/`);
                }
                return file.type === type;
            });

            if (!isValidType) {
                const typeName = accept.includes("image") ? "ảnh" : accept.includes("audio") ? "âm thanh" : accept.includes("video") ? "video" : "file";
                return `Vui lòng chọn file ${typeName}`;
            }
        }

        // Check file size
        const maxSizeBytes = maxSize * 1024 * 1024;
        if (file.size > maxSizeBytes) {
            return `Kích thước file tối đa ${maxSize}MB`;
        }

        return null;
    }, [accept, maxSize]);

    // Extract duration from video/audio file
    const extractDuration = useCallback(async (file) => {
        if (!file.type.startsWith("video/") && !file.type.startsWith("audio/")) {
            return null;
        }

        try {
            return new Promise((resolve) => {
                const media = document.createElement(file.type.startsWith("video/") ? "video" : "audio");
                media.src = URL.createObjectURL(file);
                
                media.onloadedmetadata = () => {
                    const duration = Math.round(media.duration); // Round to nearest second
                    URL.revokeObjectURL(media.src);
                    resolve(duration);
                };

                media.onerror = () => {
                    URL.revokeObjectURL(media.src);
                    resolve(null); // Return null if can't extract duration
                };

                // Timeout after 5 seconds
                setTimeout(() => {
                    URL.revokeObjectURL(media.src);
                    resolve(null);
                }, 5000);
            });
        } catch (error) {
            console.warn("Error extracting duration:", error);
            return null;
        }
    }, []);

    // Upload file
    const uploadFile = useCallback(async (file) => {
        const validationError = validateFile(file);
        if (validationError) {
            setError(validationError);
            if (onError) onError(validationError);
            return;
        }

        setUploading(true);
        setUploadProgress(10);
        setError(null);

        try {
            // Create preview
            let previewUrl = null;
            if (showPreview && file.type.startsWith("image/")) {
                previewUrl = URL.createObjectURL(file);
                setPreview(previewUrl);
            } else if (showPreview && file.type.startsWith("audio/")) {
                previewUrl = URL.createObjectURL(file);
                setPreview(previewUrl);
            } else if (showPreview && file.type.startsWith("video/")) {
                previewUrl = URL.createObjectURL(file);
                setPreview(previewUrl);
            }

            // Extract duration for video/audio files
            setUploadProgress(30);
            let duration = null;
            if (file.type.startsWith("video/") || file.type.startsWith("audio/")) {
                duration = await extractDuration(file);
            }

            // Upload to temp storage
            setUploadProgress(50);
            const uploadResponse = await fileService.uploadTempFile(file, bucket, "temp");
            setUploadProgress(100);

            if (uploadResponse.data?.success && uploadResponse.data?.data) {
                const resultData = uploadResponse.data.data;
                const tempKey = resultData.TempKey || resultData.tempKey;
                const fileType = resultData.ImageType || resultData.imageType || resultData.FileType || resultData.fileType || file.type;

                if (!tempKey) {
                    throw new Error("Không nhận được TempKey từ server");
                }

                // Call success callback with file size and duration
                if (onUploadSuccess) {
                    onUploadSuccess(tempKey, fileType, previewUrl, file.size, duration);
                }
            } else {
                throw new Error(uploadResponse.data?.message || "Upload thất bại");
            }
        } catch (error) {
            // Handle backend validation errors (file size, etc.)
            let errorMessage = error.message || "Lỗi upload file";
            
            if (error.response?.data) {
                // Backend returns: { success: false, message: "...", maxSize: "100MB" }
                if (error.response.data.message) {
                    errorMessage = error.response.data.message;
                } else if (error.response.data.error) {
                    errorMessage = error.response.data.error;
                }
            }
            
            setError(errorMessage);
            setPreview(existingUrl || null);
            if (onError) onError(errorMessage);
        } finally {
            setTimeout(() => {
                setUploading(false);
                setUploadProgress(0);
            }, 600);
        }
    }, [bucket, showPreview, existingUrl, onUploadSuccess, onError, validateFile, extractDuration]);

    // Process file
    const processFile = useCallback(async (file) => {
        if (!file) return;
        await uploadFile(file);
    }, [uploadFile]);

    // Handle file input change
    const handleFileChange = (e) => {
        const file = e.target.files?.[0];
        if (file) {
            processFile(file);
        }
    };

    // Handle drag and drop
    const handleDragOver = (e) => {
        e.preventDefault();
        setIsDragging(true);
    };

    const handleDragLeave = (e) => {
        e.preventDefault();
        setIsDragging(false);
    };

    const handleDrop = (e) => {
        e.preventDefault();
        setIsDragging(false);
        const file = e.dataTransfer.files?.[0];
        if (file) {
            processFile(file);
        }
    };

    // Handle paste from clipboard
    useEffect(() => {
        if (!enablePaste) return;

        const handlePaste = async (e) => {
            if (uploading) return;
            const items = e.clipboardData?.items;
            if (!items) return;

            for (let i = 0; i < items.length; i++) {
                const item = items[i];
                if (accept.includes("image") && item.type.startsWith("image/")) {
                    e.preventDefault();
                    const file = item.getAsFile();
                    if (file) {
                        processFile(file);
                    }
                    break;
                }
            }
        };

        document.addEventListener("paste", handlePaste);
        return () => document.removeEventListener("paste", handlePaste);
    }, [enablePaste, uploading, accept, processFile]);

    // Handle remove
    const handleRemove = () => {
        if (preview && preview.startsWith("blob:")) {
            URL.revokeObjectURL(preview);
        }
        setPreview(null);
        setError(null);
        if (fileInputRef.current) {
            fileInputRef.current.value = "";
        }
        if (onRemove) {
            onRemove();
        }
    };

    // Update preview when existingUrl changes
    useEffect(() => {
        if (existingUrl && !preview) {
            setPreview(existingUrl);
        }
    }, [existingUrl, preview]);

    // Cleanup preview URL on unmount
    useEffect(() => {
        return () => {
            if (preview && preview.startsWith("blob:")) {
                URL.revokeObjectURL(preview);
            }
        };
    }, [preview]);

    return (
        <div className="file-upload-container">
            {preview && showPreview ? (
                <div className={`file-preview-wrapper ${previewClassName}`}>
                    {preview.startsWith("blob:") || preview.startsWith("http") ? (
                        <>
                            {accept.includes("image") && (
                                <img src={preview} alt="Preview" className="file-preview-image" />
                            )}
                            {accept.includes("audio") && (
                                <audio src={preview} controls className="file-preview-audio" />
                            )}
                            {accept.includes("video") && (
                                <video src={preview} controls className="file-preview-video" />
                            )}
                        </>
                    ) : (
                        <div className="file-preview-placeholder">
                            {getFileTypeIcon()}
                            <span>File đã tải lên</span>
                        </div>
                    )}
                    <Button
                        variant="danger"
                        size="sm"
                        className="file-remove-btn"
                        onClick={handleRemove}
                        disabled={uploading}
                    >
                        <FaTimes />
                    </Button>
                </div>
            ) : (
                <div
                    className={`file-upload-area ${isDragging ? "dragging" : ""} ${uploading ? "uploading" : ""}`}
                    onClick={() => fileInputRef.current?.click()}
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                    onDrop={handleDrop}
                >
                    <input
                        type="file"
                        ref={fileInputRef}
                        hidden
                        accept={accept}
                        onChange={handleFileChange}
                        disabled={uploading}
                    />
                    <div className="file-upload-icon">{getFileTypeIcon()}</div>
                    <div className="file-upload-label">{label}</div>
                    {hint && <div className="file-upload-hint">{hint}</div>}
                </div>
            )}

            {uploading && (
                <div className="file-upload-progress">
                    <div className="file-upload-progress-fill" style={{ width: `${uploadProgress}%` }}></div>
                </div>
            )}

            {error && (
                <div className="file-upload-error">{error}</div>
            )}
        </div>
    );
}
