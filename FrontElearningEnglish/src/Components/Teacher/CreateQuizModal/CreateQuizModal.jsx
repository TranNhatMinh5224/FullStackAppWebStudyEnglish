import React, { useState, useEffect } from "react";
import { Modal, Button, Row, Col } from "react-bootstrap";
import { quizService } from "../../../Services/quizService";
import DateTimePicker from "../DateTimePicker/DateTimePicker";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import { useEnums } from "../../../Context/EnumContext";
import "./CreateQuizModal.css";

export default function CreateQuizModal({ show, onClose, onSuccess, assessmentId, quizToUpdate = null, isAdmin = false }) {
  const { getEnumOptions, loading: enumsLoading } = useEnums();
  const isUpdateMode = !!quizToUpdate;
  
  // Get enum options with fallback
  const quizTypeOptions = getEnumOptions('QuizType');
  const quizStatusOptions = getEnumOptions('QuizStatus');
  
  // Form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [instructions, setInstructions] = useState("");
  const [type, setType] = useState(1); // Practice
  const [status, setStatus] = useState(1); // Open
  const [totalQuestions, setTotalQuestions] = useState("");
  const [passingScore, setPassingScore] = useState("");
  const [totalPossibleScore, setTotalPossibleScore] = useState(""); // Tổng điểm tối đa
  const [duration, setDuration] = useState("");
  const [availableFrom, setAvailableFrom] = useState(null);
  const [showAnswersAfterSubmit, setShowAnswersAfterSubmit] = useState(true);
  const [showScoreImmediately, setShowScoreImmediately] = useState(true);
  const [shuffleQuestions, setShuffleQuestions] = useState(true);
  const [shuffleAnswers, setShuffleAnswers] = useState(true);
  const [maxAttempts, setMaxAttempts] = useState("");

  // Validation errors
  const [errors, setErrors] = useState({});

  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [loadingQuiz, setLoadingQuiz] = useState(false);
  const [showConfirmClose, setShowConfirmClose] = useState(false);

  // Load quiz data when in update mode
  useEffect(() => {
    if (show && isUpdateMode && quizToUpdate) {
      loadQuizData();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [show, isUpdateMode, quizToUpdate]);

  const loadQuizData = async () => {
    if (!quizToUpdate) return;
    
    setLoadingQuiz(true);
    try {
      const quizId = quizToUpdate.quizId || quizToUpdate.QuizId;
      const response = isAdmin
        ? await quizService.getAdminQuizById(quizId)
        : await quizService.getTeacherQuizById(quizId);
      
      if (response.data?.success && response.data?.data) {
        const quiz = response.data.data;
        setTitle(quiz.title || quiz.Title || "");
        setDescription(quiz.description || quiz.Description || "");
        setInstructions(quiz.instructions || quiz.Instructions || "");
        setType(quiz.type !== undefined ? quiz.type : (quiz.Type !== undefined ? quiz.Type : 1));
        setStatus(quiz.status !== undefined ? quiz.status : (quiz.Status !== undefined ? quiz.Status : 1));
        setTotalQuestions((quiz.totalQuestions !== undefined ? quiz.totalQuestions : (quiz.TotalQuestions !== undefined ? quiz.TotalQuestions : "")).toString());
        setPassingScore((quiz.passingScore !== undefined ? quiz.passingScore : (quiz.PassingScore !== undefined ? quiz.PassingScore : "")).toString());
        setTotalPossibleScore((quiz.totalPossibleScore !== undefined ? quiz.totalPossibleScore : (quiz.TotalPossibleScore !== undefined ? quiz.TotalPossibleScore : "")).toString());
        setDuration((quiz.duration !== undefined ? quiz.duration : (quiz.Duration !== undefined ? quiz.Duration : "")).toString());
        
        const availableFromValue = quiz.availableFrom || quiz.AvailableFrom;
        setAvailableFrom(availableFromValue ? new Date(availableFromValue) : null);
        
        setShowAnswersAfterSubmit(quiz.showAnswersAfterSubmit !== undefined ? quiz.showAnswersAfterSubmit : (quiz.ShowAnswersAfterSubmit !== undefined ? quiz.ShowAnswersAfterSubmit : true));
        setShowScoreImmediately(quiz.showScoreImmediately !== undefined ? quiz.showScoreImmediately : (quiz.ShowScoreImmediately !== undefined ? quiz.ShowScoreImmediately : true));
        setShuffleQuestions(quiz.shuffleQuestions !== undefined ? quiz.shuffleQuestions : (quiz.ShuffleQuestions !== undefined ? quiz.ShuffleQuestions : true));
        setShuffleAnswers(quiz.shuffleAnswers !== undefined ? quiz.shuffleAnswers : (quiz.ShuffleAnswers !== undefined ? quiz.ShuffleAnswers : true));
        
        const maxAttemptsValue = quiz.maxAttempts !== undefined ? quiz.maxAttempts : (quiz.MaxAttempts !== undefined ? quiz.MaxAttempts : null);
        setMaxAttempts(maxAttemptsValue ? maxAttemptsValue.toString() : "");
      }
    } catch (error) {
      console.error("Error loading quiz data:", error);
      setErrors({ ...errors, submit: "Không thể tải dữ liệu quiz" });
    } finally {
      setLoadingQuiz(false);
    }
  };

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setTitle("");
      setDescription("");
      setInstructions("");
      setType(1);
      setStatus(1);
      setTotalQuestions("");
      setPassingScore("");
      setTotalPossibleScore("");
      setDuration("");
      setAvailableFrom(null);
      setShowAnswersAfterSubmit(true);
      setShowScoreImmediately(true);
      setShuffleQuestions(true);
      setShuffleAnswers(true);
      setMaxAttempts("");
      setErrors({});
      setShowConfirmClose(false);
    }
  }, [show]);

  // Check if form has data or has been modified
  const hasFormData = () => {
    if (isUpdateMode && quizToUpdate) {
      // In update mode, check if data changed from original
      // This is a simplified check - in a real scenario, you'd compare with loaded quiz data
      return (
        title.trim() !== "" ||
        description.trim() !== "" ||
        instructions.trim() !== "" ||
        duration !== "" ||
        totalQuestions !== "" ||
        totalPossibleScore !== "" ||
        passingScore !== ""
      );
    }
    // In create mode, check if any field has data
    return (
      title.trim() !== "" ||
      description.trim() !== "" ||
      instructions.trim() !== "" ||
      duration !== "" ||
      totalQuestions !== "" ||
      passingScore !== ""
    );
  };

  // Handle close with confirmation
  const handleClose = () => {
    if (hasFormData() && !submitting) {
      setShowConfirmClose(true);
    } else {
      onClose();
    }
  };

  // Handle confirm close
  const handleConfirmClose = () => {
    setShowConfirmClose(false);
    onClose();
  };

  const validateForm = () => {
    const newErrors = {};

    // Title: NotEmpty, MaxLength(200) - BẮT BUỘC
    if (!title.trim()) {
      newErrors.title = "Tiêu đề Quiz không được để trống";
    } else if (title.trim().length > 200) {
      newErrors.title = "Tiêu đề Quiz không được vượt quá 200 ký tự";
    }

    // Description: MaxLength(1000) - TÙY CHỌN
    if (description && description.trim().length > 1000) {
      newErrors.description = "Mô tả không được vượt quá 1000 ký tự";
    }

    // Instructions: MaxLength(2000) - TÙY CHỌN
    if (instructions && instructions.trim().length > 2000) {
      newErrors.instructions = "Hướng dẫn không được vượt quá 2000 ký tự";
    }

    // Type: IsInEnum - BẮT BUỘC (đã có default value)
    // Status: IsInEnum - BẮT BUỘC (đã có default value)

    // TotalQuestions: >= 0 - BẮT BUỘC
    if (!totalQuestions || totalQuestions === "" || isNaN(parseInt(totalQuestions))) {
      newErrors.totalQuestions = "Tổng số câu hỏi là bắt buộc";
    } else if (parseInt(totalQuestions) < 0) {
      newErrors.totalQuestions = "Tổng số câu hỏi phải >= 0";
    }

    // TotalPossibleScore: > 0 - BẮT BUỘC
    if (!totalPossibleScore || totalPossibleScore === "" || isNaN(parseFloat(totalPossibleScore))) {
      newErrors.totalPossibleScore = "Thang điểm bài kiểm tra là bắt buộc";
    } else if (parseFloat(totalPossibleScore) <= 0) {
      newErrors.totalPossibleScore = "Thang điểm bài kiểm tra phải lớn hơn 0";
    }

    // PassingScore: > 0 nếu có value - TÙY CHỌN
    if (passingScore !== "" && passingScore !== null) {
      const passingScoreNum = parseInt(passingScore);
      if (isNaN(passingScoreNum) || passingScoreNum <= 0) {
        newErrors.passingScore = "Điểm đạt phải lớn hơn 0";
      }
    }

    // Duration: > 0 nếu có value - TÙY CHỌN (nhưng frontend yêu cầu bắt buộc)
    if (!duration || duration === "" || isNaN(parseInt(duration))) {
      newErrors.duration = "Thời gian làm bài là bắt buộc";
    } else if (parseInt(duration) <= 0) {
      newErrors.duration = "Thời gian làm bài phải lớn hơn 0 phút";
    }

    // AvailableFrom: >= now nếu có value - TÙY CHỌN
    if (availableFrom && availableFrom < now) {
      newErrors.availableFrom = "Thời gian mở Quiz phải trong tương lai hoặc hiện tại";
    }

    // MaxAttempts: tùy chọn, nếu có giá trị thì phải > 0 (null = không giới hạn)
    if (maxAttempts !== "" && maxAttempts !== null) {
      const maxAttemptsNum = parseInt(maxAttempts);
      if (isNaN(maxAttemptsNum) || maxAttemptsNum <= 0) {
        newErrors.maxAttempts = "Số lần làm tối đa phải lớn hơn 0";
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setSubmitting(true);

    try {
      const submitData = {
        assessmentId: parseInt(assessmentId),
        title: title.trim(),
        description: description.trim() || null,
        instructions: instructions.trim() || null,
        type: parseInt(type),
        status: parseInt(status),
        totalQuestions: parseInt(totalQuestions),
        totalPossibleScore: parseFloat(totalPossibleScore),
        // PassingScore: null nếu không có giá trị, hoặc int nếu có
        passingScore: passingScore && passingScore !== "" && !isNaN(parseInt(passingScore)) 
          ? parseInt(passingScore) 
          : null,
        // Duration: null nếu không có giá trị, hoặc int nếu có (nhưng frontend yêu cầu bắt buộc)
        duration: duration && duration !== "" && !isNaN(parseInt(duration)) 
          ? parseInt(duration) 
          : null,
        availableFrom: availableFrom ? availableFrom.toISOString() : null,
        showAnswersAfterSubmit: showAnswersAfterSubmit,
        showScoreImmediately: showScoreImmediately,
        shuffleQuestions: shuffleQuestions,
        shuffleAnswers: shuffleAnswers,
        // MaxAttempts: null nếu không có giá trị (không giới hạn), hoặc int nếu có giới hạn
        maxAttempts: maxAttempts && maxAttempts !== "" && !isNaN(parseInt(maxAttempts)) 
          ? parseInt(maxAttempts) 
          : null,
      };

      let response;
      if (isUpdateMode && quizToUpdate) {
        const quizId = quizToUpdate.quizId || quizToUpdate.QuizId;
        response = isAdmin 
          ? await quizService.updateAdminQuiz(quizId, submitData)
          : await quizService.updateQuiz(quizId, submitData);
      } else {
        response = isAdmin
          ? await quizService.createAdminQuiz(submitData)
          : await quizService.createQuiz(submitData);
      }

      if (response.data?.success) {
        onSuccess?.();
        onClose();
      } else {
        throw new Error(response.data?.message || (isUpdateMode ? "Cập nhật Quiz thất bại" : "Tạo Quiz thất bại"));
      }
    } catch (error) {
      console.error(`Error ${isUpdateMode ? "updating" : "creating"} quiz:`, error);
      const errorMessage = error.response?.data?.message || error.message || (isUpdateMode ? "Có lỗi xảy ra khi cập nhật Quiz" : "Có lỗi xảy ra khi tạo Quiz");
      setErrors({ ...errors, submit: errorMessage });
    } finally {
      setSubmitting(false);
    }
  };

  // Get current date/time for min values
  const now = new Date();
  now.setSeconds(0, 0);

  // Form validation check - khớp với backend validator
  const isFormValid = 
    title.trim() && 
    title.trim().length <= 200 &&
    (!description || description.trim().length <= 1000) &&
    (!instructions || instructions.trim().length <= 2000) &&
    totalQuestions && 
    !isNaN(parseInt(totalQuestions)) &&
    parseInt(totalQuestions) >= 0 &&
    totalPossibleScore && 
    !isNaN(parseFloat(totalPossibleScore)) &&
    parseFloat(totalPossibleScore) > 0 &&
    (passingScore === "" || (passingScore !== "" && !isNaN(parseInt(passingScore)) && parseInt(passingScore) > 0)) &&
    duration && 
    !isNaN(parseInt(duration)) &&
    parseInt(duration) > 0 &&
    (!availableFrom || availableFrom >= now) &&
    (maxAttempts === "" || (maxAttempts && !isNaN(parseInt(maxAttempts)) && parseInt(maxAttempts) > 0));

  return (
    <>
    <Modal 
      show={show} 
      onHide={handleClose}
      backdrop={submitting ? "static" : true}
      keyboard={!submitting}
      centered 
      className="create-quiz-modal modal-modern" 
      dialogClassName="create-quiz-modal-dialog"
    >
      <Modal.Header>
        <Modal.Title>{isUpdateMode ? "Cập nhật Quiz" : "Tạo Quiz mới"}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loadingQuiz ? (
          <div className="text-center py-4">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Đang tải...</span>
            </div>
          </div>
        ) : (
          <form onSubmit={(e) => {
            e.preventDefault();
            handleSubmit(e);
          }}>
          {/* Tiêu đề */}
          <div className="mb-3">
            <label className="form-label required">Tiêu đề</label>
            <input
              type="text"
              className={`form-control ${errors.title ? "is-invalid" : ""}`}
              value={title}
              onChange={(e) => {
                setTitle(e.target.value);
                setErrors({ ...errors, title: null });
              }}
              placeholder="Nhập tiêu đề Quiz"
            />
            {errors.title && <div className="invalid-feedback">{errors.title}</div>}
            <div className="form-text">*Bắt buộc</div>
          </div>

          {/* Mô tả */}
          <div className="mb-3">
            <label className="form-label">Mô tả</label>
            <textarea
              className={`form-control ${errors.description ? "is-invalid" : ""}`}
              value={description}
              onChange={(e) => {
                setDescription(e.target.value);
                setErrors({ ...errors, description: null });
              }}
              placeholder="Nhập mô tả Quiz (không bắt buộc)"
              rows={2}
            />
            {errors.description && <div className="invalid-feedback">{errors.description}</div>}
            <div className="form-text">Không bắt buộc</div>
          </div>

          {/* Hướng dẫn */}
          <div className="mb-3">
            <label className="form-label">Hướng dẫn</label>
            <textarea
              className={`form-control ${errors.instructions ? "is-invalid" : ""}`}
              value={instructions}
              onChange={(e) => {
                setInstructions(e.target.value);
                setErrors({ ...errors, instructions: null });
              }}
              placeholder="Nhập hướng dẫn làm bài (không bắt buộc)"
              rows={2}
            />
            {errors.instructions && <div className="invalid-feedback">{errors.instructions}</div>}
            <div className="form-text">Không bắt buộc</div>
          </div>

          {/* Loại Quiz và Trạng thái */}
          <Row className="g-3 mb-3">
            <Col md={6}>
              <div className="mb-3">
                <label className="form-label required">Loại Quiz</label>
                <select
                  className={`form-select ${errors.type ? "is-invalid" : ""}`}
                  value={type}
                  onChange={(e) => {
                    const newValue = parseInt(e.target.value);
                    setType(newValue);
                    setErrors({ ...errors, type: null });
                  }}
                  disabled={enumsLoading}
                >
                  {enumsLoading ? (
                    <option value={type}>Đang tải...</option>
                  ) : quizTypeOptions.length > 0 ? (
                    quizTypeOptions.map((qt) => (
                      <option key={qt.value} value={qt.value}>
                        {qt.label}
                      </option>
                    ))
                  ) : (
                    <>
                      <option value={1}>Practice</option>
                      <option value={2}>MiniTest</option>
                      <option value={3}>FinalExam</option>
                    </>
                  )}
                </select>
                {errors.type && <div className="invalid-feedback">{errors.type}</div>}
                <div className="form-text">*Bắt buộc</div>
              </div>
            </Col>

            <Col md={6}>
              <div className="mb-3">
                <label className="form-label required">Trạng thái</label>
                <select
                  className={`form-select ${errors.status ? "is-invalid" : ""}`}
                  value={status}
                  onChange={(e) => {
                    const newValue = parseInt(e.target.value);
                    setStatus(newValue);
                    setErrors({ ...errors, status: null });
                  }}
                  disabled={enumsLoading}
                >
                  {enumsLoading ? (
                    <option value={status}>Đang tải...</option>
                  ) : quizStatusOptions.length > 0 ? (
                    quizStatusOptions.map((qs) => (
                      <option key={qs.value} value={qs.value}>
                        {qs.label}
                      </option>
                    ))
                  ) : (
                    <>
                      <option value={0}>Draft</option>
                      <option value={1}>Open</option>
                      <option value={2}>Closed</option>
                      <option value={3}>Archived</option>
                    </>
                  )}
                </select>
                {errors.status && <div className="invalid-feedback">{errors.status}</div>}
                <div className="form-text">*Bắt buộc</div>
              </div>
            </Col>
          </Row>

          {/* Tổng số câu hỏi và Tổng điểm tối đa */}
          <Row className="g-3 mb-3">
            <Col md={6}>
              <div className="mb-3">
                <label className="form-label required">Tổng số câu hỏi</label>
                <input
                  type="number"
                  className={`form-control ${errors.totalQuestions ? "is-invalid" : ""}`}
                  value={totalQuestions}
                  onChange={(e) => {
                    setTotalQuestions(e.target.value);
                    setErrors({ ...errors, totalQuestions: null });
                  }}
                  placeholder="Nhập tổng số câu hỏi"
                  min="1"
                />
                {errors.totalQuestions && <div className="invalid-feedback">{errors.totalQuestions}</div>}
                <div className="form-text">*Bắt buộc</div>
              </div>
            </Col>

            <Col md={6}>
              <div className="mb-3">
                <label className="form-label required">Thang điểm bài kiểm tra</label>
                <input
                  type="text"
                  inputMode="decimal"
                  className={`form-control ${errors.totalPossibleScore ? "is-invalid" : ""}`}
                  value={totalPossibleScore}
                  onChange={(e) => {
                    let value = e.target.value.trim();
                    // Cho phép rỗng để user có thể xóa
                    if (value === '') {
                      setTotalPossibleScore('');
                      setErrors({ ...errors, totalPossibleScore: null });
                      return;
                    }
                    // Chỉ cho phép số và dấu chấm thập phân
                    const numValue = value.replace(/[^\d.]/g, '');
                    // Chỉ cho phép 1 dấu chấm
                    const parts = numValue.split('.');
                    if (parts.length <= 2) {
                      // Giữ nguyên giá trị user nhập
                      setTotalPossibleScore(numValue);
                      setErrors({ ...errors, totalPossibleScore: null });
                    }
                  }}
                  onBlur={(e) => {
                    // Validate khi blur
                    const value = e.target.value.trim();
                    if (value === '') {
                      return;
                    }
                    const num = parseFloat(value);
                    if (isNaN(num) || num <= 0) {
                      return;
                    }
                    // Nếu là số nguyên, giữ nguyên (không thêm .0)
                    if (num % 1 === 0) {
                      setTotalPossibleScore(num.toString());
                    } else {
                      // Nếu có số thập phân, giữ nguyên format user nhập
                      setTotalPossibleScore(value);
                    }
                  }}
                  placeholder="Ví dụ: 10"
                />
                {errors.totalPossibleScore && <div className="invalid-feedback">{errors.totalPossibleScore}</div>}
                <div className="form-text">*Bắt buộc. Tổng điểm tối đa của bài kiểm tra (ví dụ: 10 điểm). Sau đó bạn sẽ phân bổ điểm cho từng câu hỏi.</div>
              </div>
            </Col>
          </Row>

          {/* Điểm đạt yêu cầu */}
          <div className="mb-3">
            <label className="form-label">Điểm đạt yêu cầu</label>
            <input
              type="number"
              className={`form-control ${errors.passingScore ? "is-invalid" : ""}`}
              value={passingScore}
              onChange={(e) => {
                setPassingScore(e.target.value);
                setErrors({ ...errors, passingScore: null });
              }}
              placeholder="Nhập điểm đạt yêu cầu"
              min="0"
            />
            {errors.passingScore && <div className="invalid-feedback">{errors.passingScore}</div>}
            <div className="form-text">Không bắt buộc. Điểm tối thiểu để đạt yêu cầu</div>
          </div>

          {/* Thời gian làm bài */}
          <div className="mb-4">
            <label className="form-label required mb-3">Thời gian làm bài</label>
            <div className="duration-presets mb-3">
              <div className="preset-buttons d-flex flex-wrap gap-2">
                {[15, 30, 45, 60, 90, 120].map((minutes) => (
                  <button
                    key={minutes}
                    type="button"
                    className={`preset-btn ${duration === minutes.toString() ? "active" : ""}`}
                    onClick={() => {
                      setDuration(minutes.toString());
                      setErrors({ ...errors, duration: null });
                    }}
                  >
                    {minutes} phút
                  </button>
                ))}
              </div>
            </div>
            <div className="duration-input-wrapper">
              <div className="input-group">
                <input
                  type="number"
                  className={`form-control ${errors.duration ? "is-invalid" : ""}`}
                  value={duration}
                  onChange={(e) => {
                    setDuration(e.target.value);
                    setErrors({ ...errors, duration: null });
                  }}
                  placeholder="Hoặc nhập số phút tùy chỉnh"
                  min="1"
                />
                <span className="input-group-text">phút</span>
              </div>
              {errors.duration && <div className="invalid-feedback d-block">{errors.duration}</div>}
              <div className="form-text mt-2">
                *Bắt buộc. Thời gian tối đa học sinh có thể làm bài quiz này
              </div>
            </div>
          </div>

          {/* Thời gian có thể làm */}
          <div className="mb-4">
            <DateTimePicker
              value={availableFrom}
              onChange={(date) => {
                setAvailableFrom(date);
                setErrors({ ...errors, availableFrom: null });
              }}
              min={now}
              placeholder="dd/mm/yyyy"
              hasError={!!errors.availableFrom}
              label="Thời gian có thể làm bài (tùy chọn)"
              required={false}
              dateOnly={true}
            />
            {errors.availableFrom && <div className="invalid-feedback d-block" style={{ marginTop: "4px" }}>{errors.availableFrom}</div>}
            <div className="form-text mt-2">
              Không bắt buộc. Nếu không chọn, học sinh có thể làm bài ngay sau khi quiz được tạo
            </div>
          </div>

          {/* Cài đặt hiển thị */}
          <div className="form-section-card mb-4">
            <div className="form-section-title">Cài đặt hiển thị</div>
            <div className="d-flex flex-column gap-3">
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="showAnswersAfterSubmit"
                  checked={showAnswersAfterSubmit}
                  onChange={(e) => setShowAnswersAfterSubmit(e.target.checked)}
                />
                <label className="form-check-label" htmlFor="showAnswersAfterSubmit">
                  Hiển thị đáp án sau khi nộp bài
                </label>
              </div>
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="showScoreImmediately"
                  checked={showScoreImmediately}
                  onChange={(e) => setShowScoreImmediately(e.target.checked)}
                />
                <label className="form-check-label" htmlFor="showScoreImmediately">
                  Hiển thị điểm ngay lập tức
                </label>
              </div>
            </div>
          </div>

          {/* Cài đặt xáo trộn */}
          <div className="form-section-card mb-4">
            <div className="form-section-title">Cài đặt xáo trộn</div>
            <div className="d-flex flex-column gap-3">
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="shuffleQuestions"
                  checked={shuffleQuestions}
                  onChange={(e) => setShuffleQuestions(e.target.checked)}
                />
                <label className="form-check-label" htmlFor="shuffleQuestions">
                  Xáo trộn câu hỏi
                </label>
              </div>
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="shuffleAnswers"
                  checked={shuffleAnswers}
                  onChange={(e) => setShuffleAnswers(e.target.checked)}
                />
                <label className="form-check-label" htmlFor="shuffleAnswers">
                  Xáo trộn đáp án
                </label>
              </div>
            </div>
          </div>

          {/* Cài đặt số lần làm */}
          <div className="form-section-card mb-4">
            <div className="form-section-title">Cài đặt số lần làm</div>
            <div className="mb-3">
              <label className="form-label">Số lần làm tối đa</label>
              <input
                type="number"
                className={`form-control ${errors.maxAttempts ? "is-invalid" : ""}`}
                value={maxAttempts}
                onChange={(e) => {
                  setMaxAttempts(e.target.value);
                  setErrors({ ...errors, maxAttempts: null });
                }}
                placeholder="Để trống = không giới hạn"
                min="1"
              />
              {errors.maxAttempts && <div className="invalid-feedback">{errors.maxAttempts}</div>}
              <div className="form-text">
                Tùy chọn. Để trống nếu muốn cho phép làm lại không giới hạn. Nếu nhập số, học sinh chỉ được làm tối đa số lần đó.
              </div>
            </div>
          </div>

          {/* Submit error */}
          {errors.submit && (
            <div className="alert alert-danger mt-3">{errors.submit}</div>
          )}
        </form>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button 
          variant="secondary" 
          onClick={handleClose} 
          disabled={submitting}
          type="button"
        >
          Huỷ
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={!isFormValid || submitting || loadingQuiz}
          type="button"
        >
          {submitting ? (isUpdateMode ? "Đang cập nhật..." : "Đang tạo...") : (isUpdateMode ? "Cập nhật" : "Tạo")}
        </Button>
      </Modal.Footer>
    </Modal>

    {/* Confirm Close Modal */}
    <ConfirmModal
      isOpen={showConfirmClose}
      onClose={() => setShowConfirmClose(false)}
      onConfirm={handleConfirmClose}
      title="Xác nhận đóng"
      message={`Bạn có dữ liệu chưa được lưu. Bạn có chắc chắn muốn ${isUpdateMode ? "hủy cập nhật" : "hủy tạo"} Quiz không?`}
      confirmText="Đóng"
      cancelText="Tiếp tục"
      type="warning"
    />
    </>
  );
}

