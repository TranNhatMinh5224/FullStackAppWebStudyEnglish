import React from "react";
import { Button, ProgressBar, Spinner } from "react-bootstrap";
import { FaPlus, FaTimes, FaImage, FaVideo, FaMusic, FaUpload, FaExclamationTriangle } from "react-icons/fa";
import { useMediaUpload } from "../../../hooks/useMediaUpload";
import "./MediaUploader.css";

/**
 * Shared MediaUploader Component
 * Dùng chung cho QuizGroup, Question, FlashCard, Essay...
 * 
 * @param {string} bucket - Tên bucket (quizgroups, questions, flashcards)
 * @param {string[]} acceptTypes - Loại file được phép ['image', 'video', 'audio']
 * @param {Function} onUpload - Callback khi upload thành công (tempKey, type, duration)
 * @param {Function} onRemove - Callback khi xóa media
 * @param {string} initialPreview - URL preview ban đầu (khi edit)
 * @param {string} initialType - Loại media ban đầu (khi edit)
 * @param {string} label - Label hiển thị
 * @param {string} placeholder - Placeholder text
 * @param {number} maxSize - Giới hạn dung lượng (MB)
 * @param {string} height - Chiều cao container
 * @param {boolean} disabled - Disable upload
 * @param {string} className - Custom class
 */
export default function MediaUploader({
  bucket,
  acceptTypes = ['image', 'video'],
  onUpload,
  onRemove,
  initialPreview = null,
  initialType = null,
  label = "Media",
  placeholder = "Chọn file để upload",
  maxSize = 100,
  height = "200px",
  disabled = false,
  className = "",
}) {
  const {
    preview,
    tempKey,
    mediaType,
    duration,
    uploading,
    error,
    progress,
    fileInputRef,
    handleFileChange,
    removeMedia,
    setExistingMedia,
    triggerUpload,
    getAcceptString,
    hasMedia,
    isImage,
    isVideo,
    isAudio,
  } = useMediaUpload(bucket, {
    maxSize: maxSize * 1024 * 1024,
    acceptTypes,
  });

  // Set initial media on mount
  React.useEffect(() => {
    if (initialPreview) {
      setExistingMedia(initialPreview, initialType);
    }
  }, [initialPreview, initialType, setExistingMedia]);

  // Notify parent when upload completes
  React.useEffect(() => {
    if (tempKey && onUpload) {
      onUpload(tempKey, mediaType, duration);
    }
  }, [tempKey, mediaType, duration, onUpload]);

  const handleRemove = () => {
    removeMedia();
    if (onRemove) {
      onRemove();
    }
  };

  const getIcon = () => {
    if (isVideo) return <FaVideo size={24} className="text-primary" />;
    if (isAudio) return <FaMusic size={24} className="text-success" />;
    return <FaImage size={24} className="text-info" />;
  };

  const getPlaceholderIcon = () => {
    if (acceptTypes.length === 1) {
      if (acceptTypes[0] === 'video') return <FaVideo size={32} />;
      if (acceptTypes[0] === 'audio') return <FaMusic size={32} />;
      return <FaImage size={32} />;
    }
    return <FaUpload size={32} />;
  };

  const getAcceptedTypesText = () => {
    const types = [];
    if (acceptTypes.includes('image')) types.push('Ảnh');
    if (acceptTypes.includes('video')) types.push('Video');
    if (acceptTypes.includes('audio')) types.push('Audio');
    return types.join(', ');
  };

  return (
    <div className={`media-uploader ${className}`}>
      {label && <label className="form-label">{label}</label>}
      
      <div 
        className={`media-uploader-container border rounded bg-light ${disabled ? 'disabled' : ''} ${error ? 'border-danger' : ''}`}
        style={{ minHeight: height }}
      >
        {/* Hidden file input */}
        <input
          type="file"
          ref={fileInputRef}
          onChange={handleFileChange}
          accept={getAcceptString()}
          style={{ display: 'none' }}
          disabled={disabled || uploading}
        />

        {/* Upload state */}
        {uploading && (
          <div className="media-uploader-loading d-flex flex-column align-items-center justify-content-center h-100 p-4">
            <Spinner animation="border" variant="primary" className="mb-2" />
            <span className="text-muted small mb-2">Đang upload...</span>
            <ProgressBar 
              now={progress} 
              className="w-100" 
              style={{ height: '6px' }}
              animated 
              striped
            />
          </div>
        )}

        {/* Preview state */}
        {!uploading && hasMedia && (
          <div className="media-uploader-preview position-relative h-100">
            {isImage && (
              <img 
                src={preview} 
                alt="Preview" 
                className="img-fluid rounded w-100 h-100"
                style={{ objectFit: 'cover', maxHeight: height }}
              />
            )}
            
            {isVideo && (
              <div className="video-preview-wrapper">
                <video 
                  src={preview} 
                  controls 
                  className="w-100 rounded"
                  style={{ maxHeight: height }}
                />
                {duration && (
                  <span className="video-duration badge bg-dark position-absolute bottom-0 end-0 m-2">
                    {Math.floor(duration / 60)}:{String(duration % 60).padStart(2, '0')}
                  </span>
                )}
              </div>
            )}
            
            {isAudio && (
              <div className="audio-preview-wrapper d-flex flex-column align-items-center justify-content-center h-100 p-3">
                <FaMusic size={48} className="text-success mb-3" />
                <audio src={preview} controls className="w-100" />
                {duration && (
                  <span className="text-muted small mt-2">
                    Thời lượng: {Math.floor(duration / 60)}:{String(duration % 60).padStart(2, '0')}
                  </span>
                )}
              </div>
            )}

            {/* Remove button */}
            <Button
              variant="danger"
              size="sm"
              className="media-uploader-remove position-absolute top-0 end-0 m-2 rounded-circle"
              onClick={handleRemove}
              disabled={disabled}
              style={{ width: '32px', height: '32px', padding: 0 }}
            >
              <FaTimes />
            </Button>

            {/* Type badge */}
            <span className="media-type-badge position-absolute top-0 start-0 m-2">
              {getIcon()}
            </span>
          </div>
        )}

        {/* Empty state */}
        {!uploading && !hasMedia && (
          <div 
            className="media-uploader-empty d-flex flex-column align-items-center justify-content-center h-100 p-4 cursor-pointer"
            onClick={() => !disabled && triggerUpload()}
            style={{ cursor: disabled ? 'not-allowed' : 'pointer' }}
          >
            <div className="text-muted mb-2">
              {getPlaceholderIcon()}
            </div>
            <span className="text-muted small text-center">
              {placeholder}
            </span>
            <span className="text-muted small mt-1">
              {getAcceptedTypesText()} (Max: {maxSize}MB)
            </span>
            <Button 
              variant="outline-primary" 
              size="sm" 
              className="mt-2"
              disabled={disabled}
            >
              <FaPlus className="me-1" /> Chọn file
            </Button>
          </div>
        )}
      </div>

      {/* Error message */}
      {error && (
        <div className="media-uploader-error alert alert-danger py-2 mt-2 d-flex align-items-center">
          <FaExclamationTriangle className="me-2" />
          <small>{error}</small>
        </div>
      )}
    </div>
  );
}
