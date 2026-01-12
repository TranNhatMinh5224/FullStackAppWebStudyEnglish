import React, { useState, useEffect, useRef, useCallback } from "react";
import { Modal, Button, Form, InputGroup } from "react-bootstrap";
import { 
    FaLayerGroup, FaMarkdown, 
    FaBold, FaItalic, FaHeading, FaListUl, FaCode 
} from "react-icons/fa";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { adminService } from "../../../../Services/adminService";
import FileUpload from "../../../Common/FileUpload/FileUpload";
import ConfirmModal from "../../../Common/ConfirmModal/ConfirmModal";
import "./CourseFormModal.css";

const COURSE_IMAGE_BUCKET = "courses";

export default function CourseFormModal({ show, onClose, onSubmit, initialData }) {
    const textAreaRef = useRef(null);

    // --- TOÀN BỘ LOGIC STATE GỐC ---
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState(0);
    const [maxStudent, setMaxStudent] = useState(0);
    const [isFeatured, setIsFeatured] = useState(false);
    const [type, setType] = useState(1);
    const [imageTempKey, setImageTempKey] = useState(null);
    const [imageType, setImageType] = useState(null);
    const [existingImageUrl, setExistingImageUrl] = useState(null);
    const [errors, setErrors] = useState({});
    const [submitting, setSubmitting] = useState(false);
    const [showConfirmCancel, setShowConfirmCancel] = useState(false);

    const isUpdateMode = !!initialData;

    // --- LOGIC: MARKDOWN TOOLBAR ---
    const insertMarkdown = (tag) => {
        const area = textAreaRef.current;
        const start = area.selectionStart;
        const end = area.selectionEnd;
        const text = area.value;
        const selected = text.substring(start, end) || "văn bản";
        let inserted = "";
        
        switch(tag) {
            case 'bold': inserted = `**${selected}**`; break;
            case 'italic': inserted = `_${selected}_`; break;
            case 'heading': inserted = `### ${selected}`; break;
            case 'list': inserted = `\n- ${selected}`; break;
            case 'code': inserted = `\`${selected}\``; break;
            default: inserted = selected;
        }

        const newVal = text.substring(0, start) + inserted + text.substring(end);
        setDescription(newVal);
        // Focus lại sau khi chèn
        setTimeout(() => {
            area.focus();
            area.setSelectionRange(start + inserted.length, start + inserted.length);
        }, 0);
    };

    // --- LOGIC: INITIAL SYNC (GỐC) ---
    useEffect(() => {
        if (show && isUpdateMode && initialData) {
            setTitle(initialData.title || "");
            setDescription(initialData.description || "");
            setPrice(initialData.price || 0);
            setMaxStudent(initialData.maxStudent || 0);
            setIsFeatured(initialData.isFeatured || false);
            setType(initialData.type || 1);
            setExistingImageUrl(initialData.imageUrl || null);
        }
    }, [show, isUpdateMode, initialData]);

    // --- LOGIC: KIỂM TRA FORM CÓ NỘI DUNG ---
    const hasFormContent = () => {
        return !!(
            title.trim() ||
            description.trim() ||
            price > 0 ||
            maxStudent > 0 ||
            isFeatured ||
            type !== 1 ||
            imageTempKey ||
            existingImageUrl
        );
    };

    // --- LOGIC: XỬ LÝ HỦY BỎ VỚI XÁC NHẬN ---
    const handleCancel = () => {
        if (hasFormContent()) {
            setShowConfirmCancel(true);
        } else {
            onClose();
        }
    };

    const handleConfirmCancel = () => {
        setShowConfirmCancel(false);
        onClose();
    };

    // --- LOGIC: RESET (GỐC) ---
    useEffect(() => {
        if (!show) {
            setTitle(""); setDescription(""); setPrice(0); setMaxStudent(0);
            setIsFeatured(false); setType(1);
            setImageTempKey(null); setImageType(null); setExistingImageUrl(null);
            setErrors({}); setSubmitting(false);
            setShowConfirmCancel(false); // Reset confirm modal state
        }
    }, [show]);

    // --- LOGIC: HANDLE IMAGE UPLOAD SUCCESS ---
    const handleImageUploadSuccess = useCallback((tempKey, fileType, previewUrl) => {
        setImageTempKey(tempKey);
        setImageType(fileType);
        setErrors(prev => ({ ...prev, image: null }));
    }, []);

    // --- LOGIC: HANDLE IMAGE REMOVE ---
    const handleImageRemove = useCallback(() => {
        setImageTempKey(null);
        setImageType(null);
        setExistingImageUrl(null);
        setErrors(prev => ({ ...prev, image: null }));
    }, []);

    // --- LOGIC: HANDLE IMAGE UPLOAD ERROR ---
    const handleImageUploadError = useCallback((errorMessage) => {
        setErrors(prev => ({ ...prev, image: errorMessage }));
    }, []);

    // --- LOGIC: VALIDATION - Khớp với Backend Validator ---
    const validateForm = () => {
        const newErrors = {};

        // Title validation: NotEmpty, MaxLength(200)
        if (!title.trim()) {
            newErrors.title = "Tiêu đề là bắt buộc";
        } else if (title.trim().length > 200) {
            newErrors.title = "Tiêu đề không được vượt quá 200 ký tự";
        }

        // Description validation: NotEmpty, MaxLength(1000000)
        if (!description.trim()) {
            newErrors.description = "Mô tả là bắt buộc";
        } else if (description.trim().length > 1000000) {
            newErrors.description = "Mô tả không được vượt quá 1,000,000 ký tự";
        }

        // Price validation: GreaterThanOrEqualTo(0) when has value
        const priceValue = parseFloat(price);
        if (price !== "" && price !== null && !isNaN(priceValue) && priceValue < 0) {
            newErrors.price = "Giá phải lớn hơn hoặc bằng 0";
        }

        // MaxStudent validation: GreaterThanOrEqualTo(0)
        const maxStudentValue = parseInt(maxStudent);
        if (maxStudent !== "" && maxStudent !== null && !isNaN(maxStudentValue) && maxStudentValue < 0) {
            newErrors.maxStudent = "Số học viên tối đa phải lớn hơn hoặc bằng 0 (0 = không giới hạn)";
        }

        // Type validation: IsInEnum (1 = System, 2 = Teacher)
        const typeValue = parseInt(type);
        if (typeValue !== 1 && typeValue !== 2) {
            newErrors.type = "Loại khóa học không hợp lệ";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    // Real-time validation for individual fields
    const validateField = (fieldName, value) => {
        const newErrors = { ...errors };
        
        switch (fieldName) {
            case "title":
                if (!value.trim()) {
                    newErrors.title = "Tiêu đề là bắt buộc";
                } else if (value.trim().length > 200) {
                    newErrors.title = "Tiêu đề không được vượt quá 200 ký tự";
                } else {
                    delete newErrors.title;
                }
                break;
            case "description":
                if (!value.trim()) {
                    newErrors.description = "Mô tả là bắt buộc";
                } else if (value.trim().length > 1000000) {
                    newErrors.description = "Mô tả không được vượt quá 1,000,000 ký tự";
                } else {
                    delete newErrors.description;
                }
                break;
            case "price":
                const priceValue = parseFloat(value);
                if (value !== "" && value !== null && !isNaN(priceValue) && priceValue < 0) {
                    newErrors.price = "Giá phải lớn hơn hoặc bằng 0";
                } else {
                    delete newErrors.price;
                }
                break;
            case "maxStudent":
                const maxStudentValue = parseInt(value);
                if (value !== "" && value !== null && !isNaN(maxStudentValue) && maxStudentValue < 0) {
                    newErrors.maxStudent = "Số học viên tối đa phải lớn hơn hoặc bằng 0 (0 = không giới hạn)";
                } else {
                    delete newErrors.maxStudent;
                }
                break;
            case "type":
                const typeValue = parseInt(value);
                if (typeValue !== 1 && typeValue !== 2) {
                    newErrors.type = "Loại khóa học không hợp lệ";
                } else {
                    delete newErrors.type;
                }
                break;
            default:
                break;
        }
        
        setErrors(newErrors);
    };

    // --- LOGIC: SUBMIT (GỐC) ---
    const handleSubmit = async (e) => {
        if (e) e.preventDefault();
        
        // Validate form trước khi submit
        if (!validateForm()) {
            return;
        }

        setSubmitting(true);
        try {
            const submitData = {
                title: title.trim(),
                description: description.trim(),
                price: parseFloat(price) || 0,
                maxStudent: parseInt(maxStudent) || 0,
                isFeatured,
                type: parseInt(type),
                ...(imageTempKey && { imageTempKey, imageType })
            };
            let response = isUpdateMode 
                ? await adminService.updateCourse(initialData.courseId || initialData.CourseId, submitData)
                : await adminService.createCourse(submitData);

            if (response.data?.success) {
                onClose();
                if (onSubmit) onSubmit(response.data.data);
            } else {
                throw new Error(response.data?.message || "Thao tác thất bại");
            }
        } catch (error) {
            console.error("Error submitting course:", error);
            
            // Xử lý validation errors từ backend
            const backendErrors = {};
            let submitError = "";
            
            // Kiểm tra nếu có validation errors từ backend (ModelState errors)
            if (error.response?.data?.errors) {
                const validationErrors = error.response.data.errors;
                // Map backend field names to frontend field names
                if (validationErrors.Title) {
                    backendErrors.title = Array.isArray(validationErrors.Title) 
                        ? validationErrors.Title[0] 
                        : validationErrors.Title;
                }
                if (validationErrors.Description) {
                    backendErrors.description = Array.isArray(validationErrors.Description) 
                        ? validationErrors.Description[0] 
                        : validationErrors.Description;
                }
                if (validationErrors.Price) {
                    backendErrors.price = Array.isArray(validationErrors.Price) 
                        ? validationErrors.Price[0] 
                        : validationErrors.Price;
                }
                if (validationErrors.MaxStudent) {
                    backendErrors.maxStudent = Array.isArray(validationErrors.MaxStudent) 
                        ? validationErrors.MaxStudent[0] 
                        : validationErrors.MaxStudent;
                }
                if (validationErrors.Type) {
                    backendErrors.type = Array.isArray(validationErrors.Type) 
                        ? validationErrors.Type[0] 
                        : validationErrors.Type;
                }
            }
            
            // Nếu có message từ backend
            if (error.response?.data?.message) {
                submitError = error.response.data.message;
            } else if (error.message) {
                submitError = error.message;
            } else {
                submitError = "Có lỗi xảy ra. Vui lòng thử lại";
            }
            
            // Merge backend errors với submit error
            setErrors({ ...backendErrors, submit: submitError });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <>
        <Modal show={show} onHide={handleCancel} centered size="xl" backdrop="static" className="modal-modern create-course-modal" dialogClassName="create-course-modal-dialog">
            <Modal.Header>
                <Modal.Title className="modal-title-custom">
                    {isUpdateMode ? "Cập nhật khóa học" : "Tạo khóa học mới"}
                </Modal.Title>
            </Modal.Header>

            <Modal.Body>
                <Form onSubmit={handleSubmit}>
                    {/* SECTION 1: CƠ BẢN */}
                    <div className="form-section">
                        <div className="section-title"><FaLayerGroup /> Thông tin chung</div>
                        <div className="row g-3">
                            <div className="col-12">
                                <Form.Label className="fw-bold">Tiêu đề khóa học <span className="text-danger">*</span></Form.Label>
                                <Form.Control
                                    type="text"
                                    isInvalid={!!errors.title}
                                    value={title}
                                    onChange={(e) => { setTitle(e.target.value); validateField("title", e.target.value); }}
                                    placeholder="Nhập tiêu đề..."
                                    maxLength={200}
                                />
                                <div className="d-flex justify-content-between align-items-center mt-2">
                                    {errors.title && (
                                        <Form.Control.Feedback type="invalid" className="d-block mb-0">
                                            {errors.title}
                                        </Form.Control.Feedback>
                                    )}
                                    <div className={`char-count ms-auto ${title.length > 180 ? 'text-warning' : title.length > 195 ? 'text-danger' : ''}`}>
                                        {title.length.toLocaleString('vi-VN')} / 200 ký tự
                                    </div>
                                </div>
                            </div>
                            <div className="col-md-4">
                                <Form.Label>Giá (VND)</Form.Label>
                                <InputGroup>
                                    <Form.Control 
                                        type="number" 
                                        value={price} 
                                        onChange={e => {
                                            setPrice(e.target.value);
                                            validateField("price", e.target.value);
                                        }}
                                        isInvalid={!!errors.price}
                                        min="0"
                                        step="0.01"
                                    />
                                    <InputGroup.Text>VND</InputGroup.Text>
                                </InputGroup>
                                {errors.price && <Form.Control.Feedback type="invalid" className="d-block">{errors.price}</Form.Control.Feedback>}
                            </div>
                            <div className="col-md-4">
                                <Form.Label>Học viên tối đa</Form.Label>
                                <Form.Control 
                                    type="number" 
                                    value={maxStudent} 
                                    onChange={e => {
                                        setMaxStudent(e.target.value);
                                        validateField("maxStudent", e.target.value);
                                    }}
                                    isInvalid={!!errors.maxStudent}
                                    min="0"
                                />
                                {errors.maxStudent && <Form.Control.Feedback type="invalid" className="d-block">{errors.maxStudent}</Form.Control.Feedback>}
                                <small className="text-muted">0 = Không giới hạn</small>
                            </div>
                            <div className="col-md-4">
                                <Form.Label>Loại khóa học</Form.Label>
                                <Form.Select 
                                    value={type} 
                                    onChange={e => {
                                        setType(e.target.value);
                                        validateField("type", e.target.value);
                                    }}
                                    isInvalid={!!errors.type}
                                >
                                    <option value="1">System Course</option>
                                    <option value="2">Teacher Course</option>
                                </Form.Select>
                                {errors.type && <Form.Control.Feedback type="invalid" className="d-block">{errors.type}</Form.Control.Feedback>}
                            </div>
                        </div>
                    </div>

                    {/* SECTION 2: ẢNH */}
                    <div className="form-section">
                        <div className="section-title">Hình ảnh đại diện</div>
                        <FileUpload
                            bucket={COURSE_IMAGE_BUCKET}
                            accept="image/*"
                            maxSize={5}
                            existingUrl={existingImageUrl}
                            onUploadSuccess={handleImageUploadSuccess}
                            onRemove={handleImageRemove}
                            onError={handleImageUploadError}
                            label="Chọn ảnh hoặc kéo thả vào đây"
                            hint="Hỗ trợ Paste (Ctrl+V) từ Clipboard"
                            enablePaste={true}
                        />
                    </div>

                    {/* SECTION 3: MÔ TẢ */}
                    <div className="form-section">
                        <div className="section-title"><FaMarkdown /> Nội dung mô tả</div>
                        <div className="markdown-toolbar">
                            <button type="button" className="toolbar-btn" onClick={() => insertMarkdown('bold')} title="In đậm"><FaBold /></button>
                            <button type="button" className="toolbar-btn" onClick={() => insertMarkdown('italic')} title="In nghiêng"><FaItalic /></button>
                            <button type="button" className="toolbar-btn" onClick={() => insertMarkdown('heading')} title="Tiêu đề"><FaHeading /></button>
                            <button type="button" className="toolbar-btn" onClick={() => insertMarkdown('list')} title="Danh sách"><FaListUl /></button>
                            <button type="button" className="toolbar-btn" onClick={() => insertMarkdown('code')} title="Mã code"><FaCode /></button>
                        </div>
                        <div className="markdown-editor-container">
                            <textarea
                                ref={textAreaRef}
                                className={`markdown-textarea ${errors.description ? "border-danger" : ""}`}
                                value={description}
                                onChange={e => { 
                                    setDescription(e.target.value); 
                                    validateField("description", e.target.value); 
                                }}
                                placeholder="Viết mô tả bằng Markdown..."
                                maxLength={1000000}
                            />
                            <div className="markdown-preview">
                                {description ? <ReactMarkdown remarkPlugins={[remarkGfm]}>{description}</ReactMarkdown> : <div className="text-muted italic h-100 d-flex align-items-center justify-content-center">Xem trước nội dung...</div>}
                            </div>
                        </div>
                        <div className="d-flex justify-content-between align-items-center mt-2">
                            {errors.description && (
                                <Form.Control.Feedback type="invalid" className="d-block text-danger small mb-0">
                                    {errors.description}
                                </Form.Control.Feedback>
                            )}
                            <div className={`char-count ms-auto ${description.length > 900000 ? 'text-warning' : description.length > 950000 ? 'text-danger' : ''}`}>
                                {description.length.toLocaleString('vi-VN')} / 1,000,000 ký tự
                            </div>
                        </div>
                    </div>

                    {/* SECTION 4: NỔI BẬT */}
                    <div className="form-section bg-light border-start border-4 border-info">
                        <div className="d-flex justify-content-between align-items-center">
                            <div><div className="fw-bold">Khóa học nổi bật</div><small className="text-muted">Ưu tiên hiển thị tại trang chủ</small></div>
                            <Form.Check type="switch" checked={isFeatured} onChange={e => setIsFeatured(e.target.checked)} />
                        </div>
                    </div>

                    {errors.submit && <div className="alert alert-danger mt-3">{errors.submit}</div>}
                </Form>
            </Modal.Body>

            <Modal.Footer>
                <Button variant="link" className="text-muted text-decoration-none fw-bold" onClick={handleCancel} disabled={submitting}>Hủy bỏ</Button>
                <Button className="btn-primary-custom" onClick={handleSubmit} disabled={submitting || !title || !description}>
                    {submitting ? "Đang lưu..." : (isUpdateMode ? "Cập nhật khóa học" : "Tạo khóa học")}
                </Button>
            </Modal.Footer>
        </Modal>

        <ConfirmModal
            isOpen={showConfirmCancel}
            onClose={() => setShowConfirmCancel(false)}
            onConfirm={handleConfirmCancel}
            title="Xác nhận hủy bỏ"
            message="Bạn có nội dung chưa lưu. Bạn có chắc chắn muốn hủy bỏ không?"
            confirmText="Hủy bỏ"
            cancelText="Tiếp tục chỉnh sửa"
            type="warning"
        />
    </>
    );
}