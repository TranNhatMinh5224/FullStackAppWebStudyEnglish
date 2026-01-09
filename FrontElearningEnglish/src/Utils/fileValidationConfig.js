/**
 * Centralized File Validation Configuration
 * 
 * This file contains all file upload validation rules, size limits,
 * and helper functions used across the application.
 * 
 * Benefits:
 * - Single source of truth for file validation
 * - Easy to maintain and update limits
 * - Consistent error messages across all components
 * - Matches backend validation rules
 */

// ========================================
// FILE SIZE LIMITS (in bytes)
// ========================================
// These limits match the backend API configuration
export const FILE_SIZE_LIMITS = {
  // Images
  AVATAR: 2 * 1024 * 1024,        // 2MB - for user avatars
  IMAGE_SMALL: 2 * 1024 * 1024,   // 2MB - for thumbnails, icons
  IMAGE_STANDARD: 5 * 1024 * 1024, // 5MB - for course covers, essays, modules, lessons
  
  // Audio
  AUDIO_STANDARD: 10 * 1024 * 1024, // 10MB - for lectures, flashcards
  
  // Video
  VIDEO_STANDARD: 100 * 1024 * 1024, // 100MB - for lesson videos, lectures
  
  // Documents
  DOCUMENT: 20 * 1024 * 1024,      // 20MB - for PDFs, docs
  
  // Attachments
  ATTACHMENT: 20 * 1024 * 1024,    // 20MB - for essay attachments, general files
};

// ========================================
// BUCKET-SPECIFIC CONFIGURATIONS
// ========================================
export const BUCKET_CONFIGS = {
  'avatars': {
    maxSize: FILE_SIZE_LIMITS.AVATAR,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'],
    displayName: 'Avatar',
    sizeLabel: '2MB'
  },
  'courses': {
    maxSize: FILE_SIZE_LIMITS.IMAGE_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'],
    displayName: 'Ảnh khóa học',
    sizeLabel: '5MB'
  },
  'modules': {
    maxSize: FILE_SIZE_LIMITS.IMAGE_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'],
    displayName: 'Ảnh module',
    sizeLabel: '5MB'
  },
  'lessons': {
    maxSize: FILE_SIZE_LIMITS.IMAGE_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'],
    displayName: 'Ảnh bài học',
    sizeLabel: '5MB'
  },
  'essays': {
    maxSize: FILE_SIZE_LIMITS.IMAGE_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'audio/mpeg', 'audio/mp3', 'audio/wav'],
    displayName: 'File bài luận',
    sizeLabel: '5MB (ảnh), 10MB (audio)'
  },
  'flashcards': {
    maxSize: FILE_SIZE_LIMITS.AUDIO_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'audio/mpeg', 'audio/mp3', 'audio/wav'],
    displayName: 'Flashcard media',
    sizeLabel: '5MB (ảnh), 10MB (audio)'
  },
  'questions': {
    maxSize: FILE_SIZE_LIMITS.VIDEO_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'audio/mpeg', 'audio/mp3', 'audio/wav', 'video/mp4', 'video/webm'],
    displayName: 'Question media',
    sizeLabel: '5MB (ảnh), 10MB (audio), 100MB (video)'
  },
  'quiz-groups': {
    maxSize: FILE_SIZE_LIMITS.VIDEO_STANDARD,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'video/mp4', 'video/webm'],
    displayName: 'Quiz group media',
    sizeLabel: '5MB (ảnh), 100MB (video)'
  },
  'lectures': {
    maxSize: FILE_SIZE_LIMITS.VIDEO_STANDARD,
    allowedTypes: ['video/mp4', 'video/webm', 'audio/mpeg', 'audio/mp3', 'audio/wav'],
    displayName: 'Lecture media',
    sizeLabel: '10MB (audio), 100MB (video)'
  },
  'pronunciations': {
    maxSize: FILE_SIZE_LIMITS.AUDIO_STANDARD,
    allowedTypes: ['audio/webm', 'audio/mpeg', 'audio/mp3', 'audio/wav'],
    displayName: 'Pronunciation audio',
    sizeLabel: '10MB'
  },
  'essay-attachments': {
    maxSize: FILE_SIZE_LIMITS.ATTACHMENT,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'application/pdf', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
    displayName: 'Essay attachment',
    sizeLabel: '20MB'
  }
};

// ========================================
// HELPER FUNCTIONS
// ========================================

/**
 * Get file category (image, audio, video, document) from MIME type
 */
export function getFileCategory(mimeType) {
  if (!mimeType) return 'unknown';
  const type = mimeType.toLowerCase();
  
  if (type.startsWith('image/')) return 'image';
  if (type.startsWith('audio/')) return 'audio';
  if (type.startsWith('video/')) return 'video';
  if (type.startsWith('application/pdf') || 
      type.includes('document') || 
      type.includes('msword') || 
      type.includes('officedocument')) return 'document';
  
  return 'file';
}

/**
 * Get max size for a file based on its MIME type
 */
export function getMaxSizeByType(mimeType) {
  const category = getFileCategory(mimeType);
  
  switch (category) {
    case 'image':
      return FILE_SIZE_LIMITS.IMAGE_STANDARD;
    case 'audio':
      return FILE_SIZE_LIMITS.AUDIO_STANDARD;
    case 'video':
      return FILE_SIZE_LIMITS.VIDEO_STANDARD;
    case 'document':
      return FILE_SIZE_LIMITS.DOCUMENT;
    default:
      return FILE_SIZE_LIMITS.ATTACHMENT;
  }
}

/**
 * Get max size for a specific bucket
 */
export function getMaxSizeByBucket(bucketName) {
  const config = BUCKET_CONFIGS[bucketName];
  return config ? config.maxSize : FILE_SIZE_LIMITS.ATTACHMENT;
}

/**
 * Format bytes to human-readable string
 */
export function formatFileSize(bytes) {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
}

/**
 * Validate file against rules
 * @param {File} file - The file to validate
 * @param {Object} options - Validation options
 * @param {string} options.bucketName - The bucket name (optional, for bucket-specific rules)
 * @param {number} options.maxSize - Custom max size in bytes (optional)
 * @param {string[]} options.allowedTypes - Array of allowed MIME types (optional)
 * @returns {Object} { isValid: boolean, error: string|null }
 */
export function validateFile(file, options = {}) {
  if (!file) {
    return { isValid: false, error: 'Vui lòng chọn file' };
  }

  // Get validation rules
  let maxSize = options.maxSize;
  let allowedTypes = options.allowedTypes;
  let displayName = 'File';
  let sizeLabel = '';

  // Use bucket-specific config if available
  if (options.bucketName && BUCKET_CONFIGS[options.bucketName]) {
    const config = BUCKET_CONFIGS[options.bucketName];
    maxSize = maxSize || config.maxSize;
    allowedTypes = allowedTypes || config.allowedTypes;
    displayName = config.displayName;
    sizeLabel = config.sizeLabel;
  }

  // Fallback to type-based size if no custom size provided
  if (!maxSize) {
    maxSize = getMaxSizeByType(file.type);
  }

  // Validate file type
  if (allowedTypes && allowedTypes.length > 0) {
    const isTypeAllowed = allowedTypes.some(type => 
      file.type.toLowerCase() === type.toLowerCase()
    );
    
    if (!isTypeAllowed) {
      const category = getFileCategory(file.type);
      let typeMessage = 'file';
      if (category === 'image') typeMessage = 'ảnh';
      else if (category === 'audio') typeMessage = 'audio';
      else if (category === 'video') typeMessage = 'video';
      else if (category === 'document') typeMessage = 'tài liệu';
      
      return { 
        isValid: false, 
        error: `Loại file không được hỗ trợ. Vui lòng chọn file ${typeMessage} hợp lệ.` 
      };
    }
  }

  // Validate file size
  if (file.size > maxSize) {
    const currentSize = formatFileSize(file.size);
    const maxSizeFormatted = sizeLabel || formatFileSize(maxSize);
    
    return {
      isValid: false,
      error: `Kích thước ${displayName.toLowerCase()} (${currentSize}) vượt quá giới hạn ${maxSizeFormatted}`
    };
  }

  return { isValid: true, error: null };
}

/**
 * Validate image file (shorthand for common image validation)
 */
export function validateImage(file, maxSize = FILE_SIZE_LIMITS.IMAGE_STANDARD) {
  return validateFile(file, {
    maxSize,
    allowedTypes: ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif']
  });
}

/**
 * Validate audio file (shorthand for common audio validation)
 */
export function validateAudio(file, maxSize = FILE_SIZE_LIMITS.AUDIO_STANDARD) {
  return validateFile(file, {
    maxSize,
    allowedTypes: ['audio/mpeg', 'audio/mp3', 'audio/wav', 'audio/webm', 'audio/ogg']
  });
}

/**
 * Validate video file (shorthand for common video validation)
 */
export function validateVideo(file, maxSize = FILE_SIZE_LIMITS.VIDEO_STANDARD) {
  return validateFile(file, {
    maxSize,
    allowedTypes: ['video/mp4', 'video/webm', 'video/ogg']
  });
}

// ========================================
// EXPORT DEFAULT CONFIG
// ========================================
export default {
  FILE_SIZE_LIMITS,
  BUCKET_CONFIGS,
  validateFile,
  validateImage,
  validateAudio,
  validateVideo,
  getFileCategory,
  getMaxSizeByType,
  getMaxSizeByBucket,
  formatFileSize
};
