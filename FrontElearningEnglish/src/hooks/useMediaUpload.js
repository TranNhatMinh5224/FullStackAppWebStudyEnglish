import { useState, useCallback, useRef } from "react";
import { fileService } from "../Services/fileService";

/**
 * Custom hook để xử lý upload media (image/video/audio)
 * Dùng chung cho QuizGroup, Question, FlashCard, Essay...
 * 
 * @param {string} bucket - Tên bucket trên MinIO (quizgroups, questions, flashcards...)
 * @param {Object} options - Cấu hình thêm
 * @param {number} options.maxSize - Giới hạn dung lượng file (bytes), mặc định 100MB
 * @param {string[]} options.acceptTypes - Loại file được phép ['image', 'video', 'audio']
 * @returns {Object} - State và handlers cho media upload
 */
export function useMediaUpload(bucket, options = {}) {
  const { 
    maxSize = 100 * 1024 * 1024, // 100MB default
    acceptTypes = ['image', 'video', 'audio']
  } = options;

  // State
  const [preview, setPreview] = useState(null);
  const [tempKey, setTempKey] = useState(null);
  const [mediaType, setMediaType] = useState(null);     // Full MIME type: image/jpeg, video/mp4
  const [mediaCategory, setMediaCategory] = useState(null); // Category: image, video, audio
  const [duration, setDuration] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState(null);
  const [progress, setProgress] = useState(0);

  // Ref cho input file
  const fileInputRef = useRef(null);

  /**
   * Xác định loại media từ file
   */
  const getMediaType = useCallback((file) => {
    if (file.type.startsWith('image/')) return 'image';
    if (file.type.startsWith('video/')) return 'video';
    if (file.type.startsWith('audio/')) return 'audio';
    return null;
  }, []);

  /**
   * Lấy duration của video/audio
   */
  const getMediaDuration = useCallback((file, previewUrl) => {
    return new Promise((resolve) => {
      if (file.type.startsWith('video/')) {
        const video = document.createElement('video');
        video.src = previewUrl;
        video.onloadedmetadata = () => {
          resolve(Math.round(video.duration));
        };
        video.onerror = () => resolve(null);
      } else if (file.type.startsWith('audio/')) {
        const audio = document.createElement('audio');
        audio.src = previewUrl;
        audio.onloadedmetadata = () => {
          resolve(Math.round(audio.duration));
        };
        audio.onerror = () => resolve(null);
      } else {
        resolve(null);
      }
    });
  }, []);

  /**
   * Validate file trước khi upload
   */
  const validateFile = useCallback((file) => {
    // Check size
    if (file.size > maxSize) {
      const maxSizeMB = Math.round(maxSize / (1024 * 1024));
      return `File quá lớn. Giới hạn ${maxSizeMB}MB`;
    }

    // Check type
    const type = getMediaType(file);
    if (!type || !acceptTypes.includes(type)) {
      return `Loại file không được hỗ trợ. Chấp nhận: ${acceptTypes.join(', ')}`;
    }

    return null;
  }, [maxSize, acceptTypes, getMediaType]);

  /**
   * Handle file change từ input
   */
  const handleFileChange = useCallback(async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Reset error
    setError(null);

    // Validate
    const validationError = validateFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setUploading(true);
    setProgress(0);

    try {
      // Create preview
      const previewUrl = URL.createObjectURL(file);
      setPreview(previewUrl);

      // Get media category (image, video, audio)
      const category = getMediaType(file);
      setMediaCategory(category);
      
      // Store FULL MIME type for backend (image/jpeg, video/mp4, audio/mpeg)
      setMediaType(file.type);

      // Get duration for video/audio
      const mediaDuration = await getMediaDuration(file, previewUrl);
      setDuration(mediaDuration);

      // Upload to server
      setProgress(30);
      const response = await fileService.uploadTempFile(file, bucket, "temp");
      setProgress(90);

      if (response.data?.success && response.data?.data) {
        const key = response.data.data.tempKey || response.data.data.TempKey;
        setTempKey(key);
        setProgress(100);
      } else {
        throw new Error(response.data?.message || "Upload thất bại");
      }
    } catch (err) {
      console.error("Media upload error:", err);
      setError(err.message || "Lỗi khi upload file");
      // Reset on error
      setPreview(null);
      setTempKey(null);
      setMediaType(null);
      setMediaCategory(null);
      setDuration(null);
    } finally {
      setUploading(false);
    }
  }, [bucket, validateFile, getMediaType, getMediaDuration]);

  /**
   * Xóa media đã upload
   */
  const removeMedia = useCallback(() => {
    if (preview) {
      URL.revokeObjectURL(preview);
    }
    setPreview(null);
    setTempKey(null);
    setMediaType(null);
    setMediaCategory(null);
    setDuration(null);
    setError(null);
    setProgress(0);
    
    // Reset input
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  }, [preview]);

  /**
   * Set media từ URL có sẵn (khi edit)
   * @param {string} url - URL của media
   * @param {string} type - Full MIME type (image/jpeg, video/mp4) hoặc category (image, video)
   */
  const setExistingMedia = useCallback((url, type = null) => {
    if (!url) return;
    
    setPreview(url);
    
    // Auto detect category from URL
    let category = null;
    const lowerUrl = url.toLowerCase();
    if (lowerUrl.match(/\.(mp4|webm|mov|avi)(\?|$)/)) {
      category = 'video';
    } else if (lowerUrl.match(/\.(mp3|wav|ogg|m4a)(\?|$)/)) {
      category = 'audio';
    } else {
      category = 'image';
    }
    
    // Set category for display logic
    setMediaCategory(category);
    
    // Set full MIME type if provided, else store category
    // Backend returns full MIME type like 'image/jpeg', 'video/mp4'
    if (type && type.includes('/')) {
      setMediaType(type);
    } else {
      setMediaType(type || category);
    }
  }, []);

  /**
   * Reset toàn bộ state
   */
  const reset = useCallback(() => {
    removeMedia();
  }, [removeMedia]);

  /**
   * Trigger file input click
   */
  const triggerUpload = useCallback(() => {
    fileInputRef.current?.click();
  }, []);

  /**
   * Get accept string cho input file
   */
  const getAcceptString = useCallback(() => {
    const accepts = [];
    if (acceptTypes.includes('image')) accepts.push('image/*');
    if (acceptTypes.includes('video')) accepts.push('video/*');
    if (acceptTypes.includes('audio')) accepts.push('audio/*');
    return accepts.join(',');
  }, [acceptTypes]);

  return {
    // State
    preview,
    tempKey,
    mediaType,      // Full MIME type: image/jpeg, video/mp4
    mediaCategory,  // Category: image, video, audio
    duration,
    uploading,
    error,
    progress,
    
    // Refs
    fileInputRef,
    
    // Actions
    handleFileChange,
    removeMedia,
    setExistingMedia,
    reset,
    triggerUpload,
    
    // Helpers
    getAcceptString,
    
    // Computed - use mediaCategory for display logic
    hasMedia: !!preview,
    isImage: mediaCategory === 'image' || mediaType?.startsWith('image'),
    isVideo: mediaCategory === 'video' || mediaType?.startsWith('video'),
    isAudio: mediaCategory === 'audio' || mediaType?.startsWith('audio'),
  };
}

export default useMediaUpload;
