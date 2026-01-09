import React, { useState, useEffect, useRef } from "react";
import { Modal, Button } from "react-bootstrap";
import { fileService } from "../../../../Services/fileService";
import { adminService } from "../../../../Services/adminService";
import { FaFileUpload, FaTimes } from "react-icons/fa";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { validateFile } from "../../../../Utils/fileValidationConfig";
import "./CourseFormModal.css";

const COURSE_IMAGE_BUCKET = "courses";

export default function CourseFormModal({ show, onClose, onSubmit, initialData }) {
  const fileInputRef = useRef(null);

  // Form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState(0);
  const [maxStudent, setMaxStudent] = useState(0);
  const [isFeatured, setIsFeatured] = useState(false);
  const [type, setType] = useState(1); // 1 = System, 2 = Teacher

  // Image upload state
  const [selectedImage, setSelectedImage] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [imageTempKey, setImageTempKey] = useState(null);
  const [imageType, setImageType] = useState(null);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [existingImageUrl, setExistingImageUrl] = useState(null);
  const [isDragOver, setIsDragOver] = useState(false);

  // Validation errors
  const [errors, setErrors] = useState({});

  // Submit state
  const [submitting, setSubmitting] = useState(false);

  const isUpdateMode = !!initialData;

  // Pre-fill form when in update mode
  useEffect(() => {
    if (show && isUpdateMode && initialData) {
      setTitle(initialData.title || "");
      setDescription(initialData.description || "");
      setPrice(initialData.price || 0);
      setMaxStudent(initialData.maxStudent || 0);
      setIsFeatured(initialData.isFeatured || initialData.IsFeatured || false);
      setType(initialData.type || 1);
      
      const courseImageUrl = initialData.imageUrl || null;
      setExistingImageUrl(courseImageUrl);
      
      if (courseImageUrl) {
        setImagePreview(courseImageUrl);
      } else {
        setImagePreview(null);
      }
      
      setSelectedImage(null);
      setImageTempKey(null);
      setImageType(null);
    }
  }, [show, isUpdateMode, initialData]);

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setTitle("");
      setDescription("");
      setPrice(0);
      setMaxStudent(0);
      setIsFeatured(false);
      setType(1);
      setSelectedImage(null);
      setImagePreview(null);
      setImageTempKey(null);
      setImageType(null);
      setExistingImageUrl(null);
      setErrors({});
      setSubmitting(false);
      setUploadingImage(false);
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

  // Image handling
  const handleImageClick = () => {
    if (!uploadingImage && fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleImageChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Use centralized validation
    const validation = validateFile(file, { bucketName: COURSE_IMAGE_BUCKET });
    if (!validation.isValid) {
      setErrors({ ...errors, image: validation.error });
      return;
    }

    setSelectedImage(file);
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result);
    };
    reader.readAsDataURL(file);

    // Upload immediately
    await uploadImage(file);
  };

  const uploadImage = async (file) => {
    setUploadingImage(true);
    setErrors({ ...errors, image: null });

    try {
      const uploadResponse = await fileService.uploadTempFile(
        file,
        COURSE_IMAGE_BUCKET,
        "temp"
      );

      console.log("Upload response:", uploadResponse.data);

      if (uploadResponse.data?.success && uploadResponse.data?.data) {
        const resultData = uploadResponse.data.data;
        // Backend trả về PascalCase: TempKey, ImageUrl, ImageType
        const tempKey = resultData.TempKey || resultData.tempKey;
        const imageTypeValue = resultData.ImageType || resultData.imageType || file.type;

        console.log("TempKey:", tempKey, "ImageType:", imageTypeValue);

        if (!tempKey) {
          throw new Error("Không nhận được TempKey từ server");
        }

        setImageTempKey(tempKey);
        setImageType(imageTypeValue);
      } else {
        const errorMsg = uploadResponse.data?.message || "Upload ảnh thất bại";
        throw new Error(errorMsg);
      }
    } catch (error) {
      console.error("Error uploading image:", error);
      setErrors({ ...errors, image: error.response?.data?.message || "Có lỗi xảy ra khi upload ảnh" });
      setSelectedImage(null);
      setImagePreview(existingImageUrl || null);
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
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  // Drag and drop handlers
  const handleDragOver = (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (!uploadingImage) {
      setIsDragOver(true);
    }
  };

  const handleDragLeave = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);
  };

  const handleDrop = async (e) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);

    if (uploadingImage) return;

    const files = e.dataTransfer.files;
    if (files && files[0]) {
      const file = files[0];
      // Use centralized validation
      const validation = validateFile(file, { bucketName: COURSE_IMAGE_BUCKET });
      if (!validation.isValid) {
        setErrors({ ...errors, image: validation.error });
        return;
      }

      setSelectedImage(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);

      // Upload immediately
      await uploadImage(file);
    }
  };

  // Paste handler
  const handlePaste = async (e) => {
    if (uploadingImage) return;

    const items = e.clipboardData?.items;
    if (!items) return;

    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      if (item.type.indexOf('image') !== -1) {
        e.preventDefault();
        const file = item.getAsFile();
        if (file) {
          // Use centralized validation
          const validation = validateFile(file, { bucketName: COURSE_IMAGE_BUCKET });
          if (!validation.isValid) {
            setErrors({ ...errors, image: validation.error });
            return;
          }

          setSelectedImage(file);
          const reader = new FileReader();
          reader.onloadend = () => {
            setImagePreview(reader.result);
          };
          reader.readAsDataURL(file);

          // Upload immediately
          await uploadImage(file);
        }
        break;
      }
    }
  };

  // Validation
  const validateForm = () => {
    const newErrors = {};

    if (!title.trim()) {
      newErrors.title = "Tiêu đề là bắt buộc";
    }

    if (!description.trim()) {
      newErrors.description = "Mô tả là bắt buộc";
    }

    // Validate price
    const priceValue = parseFloat(price);
    if (isNaN(priceValue) || priceValue < 0) {
      newErrors.price = "Giá không được là số âm";
    }

    // Validate maxStudent
    const maxStudentValue = parseInt(maxStudent);
    if (isNaN(maxStudentValue) || maxStudentValue < 0) {
      newErrors.maxStudent = "Số học viên không được là số âm";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const validateField = (fieldName, value) => {
    const newErrors = { ...errors };

    if (fieldName === 'price') {
      const priceValue = parseFloat(value);
      if (isNaN(priceValue) || priceValue < 0) {
        newErrors.price = "Giá không được là số âm";
      } else {
        newErrors.price = null;
      }
    } else if (fieldName === 'maxStudent') {
      const maxStudentValue = parseInt(value);
      if (isNaN(maxStudentValue) || maxStudentValue < 0) {
        newErrors.maxStudent = "Số học viên không được là số âm";
      } else {
        newErrors.maxStudent = null;
      }
    }

    setErrors(newErrors);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setSubmitting(true);

    try {
      let submitData;
      
      if (isUpdateMode && initialData) {
        submitData = {
          title: title.trim(),
          description: description.trim(),
          price: parseFloat(price) || 0,
          maxStudent: parseInt(maxStudent) || 0,
          IsFeatured: isFeatured
        };

        if (imageTempKey && imageType) {
          submitData.imageTempKey = imageTempKey;
          submitData.imageType = imageType;
        }
        
        const courseId = initialData.courseId || initialData.CourseId;
        if (!courseId) {
          throw new Error("Không tìm thấy ID khóa học");
        }
        const response = await adminService.updateCourse(courseId, submitData);
        
        if (response.data?.success) {
          // Đóng modal trước
          onClose();
          // Gọi callback để parent hiện SuccessModal
          if (onSubmit) onSubmit(response.data.data);
        } else {
          throw new Error(response.data?.message || "Cập nhật thất bại");
        }
      } else {
        submitData = {
          title: title.trim(),
          description: description.trim(),
          price: parseFloat(price) || 0,
          maxStudent: parseInt(maxStudent) || 0,
          IsFeatured: isFeatured,
          type: parseInt(type)
        };

        if (imageTempKey && imageType) {
          submitData.imageTempKey = imageTempKey;
          submitData.imageType = imageType;
        }

        const response = await adminService.createCourse(submitData);
        
        if (response.data?.success) {
          // Đóng modal trước
          onClose();
          // Gọi callback để parent hiện SuccessModal
          if (onSubmit) onSubmit(response.data.data);
        } else {
          throw new Error(response.data?.message || "Tạo khóa học thất bại");
        }
      }
    } catch (error) {
      console.error("Error submitting course:", error);
      setErrors({ ...errors, submit: error.message || "Có lỗi xảy ra. Vui lòng thử lại" });
    } finally {
      setSubmitting(false);
    }
  };

  const isFormValid = title.trim() && description.trim();

  return (
    <Modal 
      show={show} 
      onHide={onClose}
      centered
      size="xl"
      backdrop="static"
      className="create-course-modal"
      dialogClassName="create-course-modal-dialog"
      style={{ zIndex: 1050 }}
    >
        <Modal.Header className="p-0 border-0">
          <div className="w-100 position-relative">
            <div className="form-top-banner text-white mb-0">
              <h4 className="mb-1 fw-bold">{isUpdateMode ? 'Cập nhật khóa học' : 'Tạo khóa học mới'}</h4>
              
            </div>
            <button type="button" className="btn-close" aria-label="Close" onClick={onClose} />
          </div>
        </Modal.Header>
        <Modal.Body>
          <form onSubmit={handleSubmit} className="p-0">

            {/* Basic Information Card */}
            <div className="card mb-4 shadow-sm">
              <div className="card-body">
                <h6 className="card-title fw-semibold mb-3">Thông tin cơ bản</h6>
                <div className="row g-3">
                  <div className="col-12">
                    <label className="form-label required">Tiêu đề khóa học</label>
                    <input
                      type="text"
                      className={`form-control ${errors.title ? 'is-invalid' : ''}`}
                      value={title}
                      onChange={(e) => { setTitle(e.target.value); setErrors({ ...errors, title: null }); }}
                      placeholder="Nhập tiêu đề khóa học"
                    />
                    {errors.title && <div className="invalid-feedback">{errors.title}</div>}
                    <div className="form-text mt-1">Tiêu đề sẽ hiển thị với học viên. Ngắn gọn, rõ ràng.</div>
                  </div>

                  <div className="col-12">
                    <label className="form-label">Ảnh bìa khóa học</label>

                    {imagePreview ? (
                      <div className="position-relative">
                        <div className="d-flex justify-content-center p-3 bg-white border rounded">
                          <img src={imagePreview} alt="Course preview" className="img-fluid rounded" style={{maxHeight: 320}} />
                        </div>
                        <button type="button" className="btn btn-sm btn-danger position-absolute" style={{right:12, top:12}} onClick={handleRemoveImage} disabled={uploadingImage}>Remove</button>
                        <div className="d-flex justify-content-between align-items-center mt-2">
                          <small className="text-muted">Preview shown above</small>
                          <span className="recommended-badge">1200×600</span>
                        </div>
                      </div>
                    ) : (
                      <div 
                        className={`d-flex flex-column align-items-center justify-content-center border rounded p-4 text-center image-upload-area ${isDragOver ? 'drag-over' : ''} ${uploadingImage ? 'uploading' : ''}`}
                        role="button" 
                        onClick={handleImageClick}
                        onDragOver={handleDragOver}
                        onDragLeave={handleDragLeave}
                        onDrop={handleDrop}
                        style={{
                          minHeight:140, 
                          transition: 'all 0.2s ease',
                          backgroundColor: isDragOver ? '#e3f2fd' : '#f8f9fa',
                          borderColor: isDragOver ? '#2196f3' : '#dee2e6',
                          borderStyle: 'dashed',
                          borderWidth: isDragOver ? '2px' : '1px'
                        }}
                      >
                        <input type="file" ref={fileInputRef} onChange={handleImageChange} accept="image/*" style={{display:'none'}} />
                        <div className="mb-2 text-muted" style={{fontSize:28}}>{isDragOver ? '📥' : '📤'}</div>
                        <div className="fw-semibold text-secondary">
                          {isDragOver ? 'Thả ảnh vào đây' : uploadingImage ? 'Đang upload...' : 'Click, kéo thả hoặc paste ảnh'}
                        </div>
                        <div className="d-flex gap-2 align-items-center mt-2">
                          <small className="text-muted">Recommended:</small>
                          <span className="recommended-badge">1200×600</span>
                          <small className="text-muted">• JPG/PNG • Max 2MB</small>
                        </div>
                      </div>
                    )}

                    {errors.image && <div className="text-danger small mt-2">{errors.image}</div>}
                  </div>
                </div>
              </div>
            </div>

            {/* Settings Card */}
            <div className="card mb-4 shadow-sm">
              <div className="card-body">
                <h6 className="card-title fw-semibold mb-3">Thiết lập khóa học</h6>
                <div className="row g-3">
                  <div className="col-md-4">
                    <label className="form-label">Giá (VND)</label>
                    <input 
                      type="number" 
                      className={`form-control ${errors.price ? 'is-invalid' : ''}`}
                      value={price} 
                      onChange={(e) => { 
                        setPrice(e.target.value); 
                        validateField('price', e.target.value); 
                      }}
                      placeholder="0"
                      min="0"
                    />
                    {errors.price && <div className="invalid-feedback d-block">{errors.price}</div>}
                    <div className="form-text">0 = Miễn phí</div>
                  </div>

                  <div className="col-md-4">
                    <label className="form-label">Số học viên tối đa</label>
                    <input 
                      type="number" 
                      className={`form-control ${errors.maxStudent ? 'is-invalid' : ''}`}
                      value={maxStudent} 
                      onChange={(e) => { 
                        setMaxStudent(e.target.value); 
                        validateField('maxStudent', e.target.value); 
                      }}
                      placeholder="0"
                      min="0"
                    />
                    {errors.maxStudent && <div className="invalid-feedback d-block">{errors.maxStudent}</div>}
                    <div className="form-text">0 = Không giới hạn</div>
                  </div>

                  <div className="col-md-4">
                    <label className="form-label">Loại khóa học</label>
                    <select className="form-select" value={type} onChange={(e)=>setType(e.target.value)}>
                      <option value={1}>System Course</option>
                      <option value={2}>Teacher Course</option>
                    </select>
                  </div>
                </div>
              </div>
            </div>

            {/* Description Card */}
            <div className="card mb-4 shadow-sm">
              <div className="card-body">
                <div className="d-flex justify-content-between align-items-start mb-2">
                  <div>
                    <h6 className="card-title fw-semibold mb-1">Mô tả khóa học</h6>
                    <small className="text-muted">Viết mô tả rõ ràng. Hỗ trợ Markdown. Xem trước bên phải.</small>
                  </div>
                  <div className="text-end text-muted small">Optional — but recommended</div>
                </div>

                <div className="row gx-3">
                  <div className="col-lg-6 mb-3">
                    <div className="mb-2 d-flex align-items-center gap-2">
                      <div className="btn-group" role="group" aria-label="markdown toolbar">
                        <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setDescription(d => `${d}**bold text**`)} title="Bold"><strong>B</strong></button>
                        <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setDescription(d => `${d}\n\n# Heading\n\n`)} title="Heading">H</button>
                        <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setDescription(d => `${d}\n- List item\n`)} title="List">•</button>
                      </div>
                      <small className="text-muted">Toolbar</small>
                    </div>

                    <textarea className={`form-control`} style={{minHeight:340, fontFamily: "Menlo, Monaco, 'Courier New', monospace"}} value={description} onChange={(e)=>{ setDescription(e.target.value); setErrors({...errors, description:null}); }} placeholder={"# Giới thiệu\n\nViết mô tả ngắn gọn...\n\n- Kết quả học tập\n- Ai nên tham gia\n\n**Bắt đầu với mục tiêu học tập.**"} />
                    <div className="form-text mt-2">Gợi ý: Bắt đầu với kết quả, sau đó tóm tắt nội dung khóa học.</div>
                  </div>

                  <div className="col-lg-6 mb-3">
                    <div className="border rounded p-3 h-100 bg-white" style={{minHeight:340, overflowY:'auto'}}>
                      {description?.trim() ? (
                        <ReactMarkdown remarkPlugins={[remarkGfm]}>{description}</ReactMarkdown>
                      ) : (
                        <div className="text-muted">Xem trước mô tả sẽ hiển thị ở đây khi bạn soạn thảo.</div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Featured */}
            <div className="mb-3">
              <div className="form-check form-switch">
                <input className="form-check-input" type="checkbox" id="featuredCheck" checked={isFeatured} onChange={(e)=>setIsFeatured(e.target.checked)} />
                <label className="form-check-label" htmlFor="featuredCheck">Đánh dấu là khóa học nổi bật</label>
              </div>
            </div>

            {/* Submit error */}
            {errors.submit && (<div className="alert alert-danger mt-3">{errors.submit}</div>)}
          </form>
        </Modal.Body>
        <Modal.Footer>
              <Button variant="outline-secondary" onClick={onClose} disabled={submitting}>
                Hủy
              </Button>
              <Button
                variant="primary"
                className="btn-primary"
                onClick={handleSubmit}
                disabled={!isFormValid || submitting || uploadingImage}
              >
                {submitting ? (isUpdateMode ? "Đang cập nhật..." : "Đang tạo...") : (isUpdateMode ? "Lưu & Cập nhật" : "Tạo khóa học")}
              </Button>
            </Modal.Footer>
      </Modal>
  );
}
