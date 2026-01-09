import { useState, useCallback, useEffect } from "react";
import { quizService } from "../Services/quizService";

/**
 * Custom hook để quản lý form QuizGroup
 * Tách logic khỏi component để dễ maintain và test
 * 
 * @param {number|null} groupId - ID của group (null nếu tạo mới)
 * @param {number} quizSectionId - ID của section chứa group
 * @param {boolean} isAdmin - Có phải admin không
 */
export function useQuizGroupForm(groupId, quizSectionId, isAdmin = false) {
  const isUpdateMode = !!groupId;

  // Form data
  const [formData, setFormData] = useState({
    name: "",
    title: "",
    description: "",
    sumScore: 0,
    displayOrder: 0,
  });

  // Media data
  const [mediaData, setMediaData] = useState({
    imgTempKey: null,
    imgType: null,
    imgUrl: null,
    videoTempKey: null,
    videoType: null,
    videoUrl: null,
    videoDuration: null,
  });

  // State
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(false);
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState(null);

  /**
   * Load group data khi edit
   */
  const loadGroupData = useCallback(async () => {
    if (!groupId) return;

    setLoadingData(true);
    try {
      const response = await quizService.getQuizGroupById(groupId);
      
      if (response.data?.success && response.data?.data) {
        const group = response.data.data;
        
        setFormData({
          name: group.name || group.Name || "",
          title: group.title || group.Title || "",
          description: group.description || group.Description || "",
          sumScore: group.sumScore ?? group.SumScore ?? 0,
          displayOrder: group.displayOrder ?? group.DisplayOrder ?? 0,
        });

        setMediaData({
          imgTempKey: null, // Không có tempKey khi edit
          imgType: group.imgType || group.ImgType || null,
          imgUrl: group.imgUrl || group.ImgUrl || null,
          videoTempKey: null,
          videoType: group.videoType || group.VideoType || null,
          videoUrl: group.videoUrl || group.VideoUrl || null,
          videoDuration: group.videoDuration || group.VideoDuration || null,
        });
      }
    } catch (error) {
      console.error("Error loading group data:", error);
      setSubmitError("Không thể tải dữ liệu group");
    } finally {
      setLoadingData(false);
    }
  }, [groupId]);

  /**
   * Load data khi mount (nếu edit mode)
   */
  useEffect(() => {
    if (isUpdateMode) {
      loadGroupData();
    }
  }, [isUpdateMode, loadGroupData]);

  /**
   * Update form field
   */
  const updateField = useCallback((field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error khi user sửa
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: null }));
    }
  }, [errors]);

  /**
   * Update image media
   */
  const setImageMedia = useCallback((tempKey, type) => {
    setMediaData(prev => ({
      ...prev,
      imgTempKey: tempKey,
      imgType: type,
    }));
  }, []);

  /**
   * Update video media
   */
  const setVideoMedia = useCallback((tempKey, type, duration) => {
    setMediaData(prev => ({
      ...prev,
      videoTempKey: tempKey,
      videoType: type,
      videoDuration: duration,
    }));
  }, []);

  /**
   * Clear image
   */
  const clearImage = useCallback(() => {
    setMediaData(prev => ({
      ...prev,
      imgTempKey: null,
      imgType: null,
      imgUrl: null,
    }));
  }, []);

  /**
   * Clear video
   */
  const clearVideo = useCallback(() => {
    setMediaData(prev => ({
      ...prev,
      videoTempKey: null,
      videoType: null,
      videoUrl: null,
      videoDuration: null,
    }));
  }, []);

  /**
   * Validate form
   */
  const validate = useCallback(() => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = "Tên nhóm là bắt buộc";
    }

    if (!formData.title.trim()) {
      newErrors.title = "Tiêu đề là bắt buộc";
    }

    if (formData.sumScore < 0) {
      newErrors.sumScore = "Tổng điểm phải >= 0";
    }

    if (formData.displayOrder < 0) {
      newErrors.displayOrder = "Thứ tự phải >= 0";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, [formData]);

  /**
   * Build submit payload
   */
  const buildPayload = useCallback(() => {
    const payload = {
      name: formData.name.trim(),
      title: formData.title.trim(),
      description: formData.description.trim() || null,
      sumScore: parseFloat(formData.sumScore) || 0,
      displayOrder: parseInt(formData.displayOrder) || 0,
    };

    // Chỉ thêm quizSectionId khi tạo mới
    if (!isUpdateMode) {
      payload.quizSectionId = parseInt(quizSectionId);
    }

    // Image
    if (mediaData.imgTempKey) {
      payload.imgTempKey = mediaData.imgTempKey;
      payload.imgType = mediaData.imgType;
    }

    // Video
    if (mediaData.videoTempKey) {
      payload.videoTempKey = mediaData.videoTempKey;
      payload.videoType = mediaData.videoType;
      payload.videoDuration = mediaData.videoDuration;
    }

    return payload;
  }, [formData, mediaData, isUpdateMode, quizSectionId]);

  /**
   * Submit form
   */
  const submit = useCallback(async () => {
    if (!validate()) {
      return { success: false, error: "Validation failed" };
    }

    setLoading(true);
    setSubmitError(null);

    try {
      const payload = buildPayload();
      let response;

      if (isUpdateMode) {
        response = isAdmin
          ? await quizService.updateAdminQuizGroup(groupId, payload)
          : await quizService.updateQuizGroup(groupId, payload);
      } else {
        response = isAdmin
          ? await quizService.createAdminQuizGroup(payload)
          : await quizService.createQuizGroup(payload);
      }

      if (response.data?.success) {
        return { success: true, data: response.data.data };
      } else {
        throw new Error(response.data?.message || "Thao tác thất bại");
      }
    } catch (error) {
      console.error("Submit error:", error);
      const errorMessage = error.response?.data?.message || error.message || "Có lỗi xảy ra";
      setSubmitError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setLoading(false);
    }
  }, [validate, buildPayload, isUpdateMode, isAdmin, groupId]);

  /**
   * Reset form
   */
  const reset = useCallback(() => {
    setFormData({
      name: "",
      title: "",
      description: "",
      sumScore: 0,
      displayOrder: 0,
    });
    setMediaData({
      imgTempKey: null,
      imgType: null,
      imgUrl: null,
      videoTempKey: null,
      videoType: null,
      videoUrl: null,
      videoDuration: null,
    });
    setErrors({});
    setSubmitError(null);
  }, []);

  /**
   * Check if form is valid for submit
   */
  const isValid = formData.name.trim() && formData.title.trim() && formData.sumScore >= 0;

  return {
    // Form data
    formData,
    mediaData,
    
    // State
    loading,
    loadingData,
    errors,
    submitError,
    isUpdateMode,
    isValid,
    
    // Actions
    updateField,
    setImageMedia,
    setVideoMedia,
    clearImage,
    clearVideo,
    validate,
    submit,
    reset,
    loadGroupData,
  };
}

export default useQuizGroupForm;
