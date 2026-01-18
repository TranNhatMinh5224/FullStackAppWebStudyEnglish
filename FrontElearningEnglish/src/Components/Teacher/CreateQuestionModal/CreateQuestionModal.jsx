import React, { useState, useEffect, useRef, useCallback } from "react";
import { Modal, Button, Form, Row, Col, Tab, Tabs, InputGroup, Badge } from "react-bootstrap";
import { FaPlus, FaTrash, FaArrowUp, FaArrowDown, FaTimes, FaLayerGroup, FaQuestionCircle, FaEdit } from "react-icons/fa";
import { questionService } from "../../../Services/questionService";
import { fileService } from "../../../Services/fileService";
import { quizService } from "../../../Services/quizService";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import { useAuth } from "../../../Context/AuthContext";
import "./CreateQuestionModal.css";

// QuestionType enum từ backend - phù hợp với QuestionType.cs
const QUESTION_TYPES = {
  MultipleChoice: 1,    // chọn 1 đáp án đúng
  MultipleAnswers: 2,   // chọn nhiều đáp án đúng
  TrueFalse: 3,         // đúng/sai
  FillBlank: 4,         // điền vào chỗ trống
  Matching: 5,         // nối từ/cụm từ
  Ordering: 6,          // sắp xếp thứ tự
};

const QUESTION_BUCKET = "questions";
const QUIZ_GROUP_BUCKET = "quizgroups";

export default function CreateQuestionModal({
  show,
  onClose,
  onSuccess,
  sectionId,
  groupId,
  questionToUpdate,
  isBulkMode = false,
  onSaveDraft,
  isAdmin: propIsAdmin = false
}) {  
  const { roles } = useAuth();
  
  // Auto-detect admin role from AuthContext if not explicitly provided
  const isAdmin = propIsAdmin || (roles && roles.some(role => {
    const roleName = typeof role === 'string' ? role : (role?.name || '');
    return roleName === "SuperAdmin" || 
           roleName === "ContentAdmin" || 
           roleName === "FinanceAdmin";
  }));
  
  const [activeTab, setActiveTab] = useState("question");
  const [internalGroupId, setInternalGroupId] = useState(groupId); 
  const [groupInfo, setGroupInfo] = useState(null);
  const [sectionInfo, setSectionInfo] = useState(null);
  const [createdGroupName, setCreatedGroupName] = useState(""); 

  const fetchGroupDetails = useCallback(async (id) => {
      try {
          const res = isAdmin
            ? await quizService.getAdminQuizGroupById(id)
            : await quizService.getQuizGroupById(id);
          if (res.data?.success) setGroupInfo(res.data.data);
      } catch (e) { console.error(e); }
  }, [isAdmin]);

  const fetchSectionDetails = useCallback(async (id) => {
      try {
          const res = isAdmin
            ? await quizService.getAdminQuizSectionById(id)
            : await quizService.getQuizSectionById(id);
          if (res.data?.success) setSectionInfo(res.data.data);
      } catch (e) { console.error(e); }
  }, [isAdmin]);

  useEffect(() => {
    if (show) {
      const targetGId = questionToUpdate ? (questionToUpdate.quizGroupId || questionToUpdate.QuizGroupId) : groupId;
      setInternalGroupId(targetGId);
      setActiveTab("question");
      if (targetGId) fetchGroupDetails(targetGId);
      if (sectionId) fetchSectionDetails(sectionId);
      // resetQuestionForm will be called after it's defined
    }
  }, [show, groupId, sectionId, questionToUpdate, fetchGroupDetails, fetchSectionDetails]);

  const [qFormData, setQFormData] = useState({
    stemText: "",
    explanation: "",
    points: 10,
    type: null, // Không có giá trị mặc định
    options: [],
    matchingPairs: [], 
  });
  
  const [qMediaPreview, setQMediaPreview] = useState(null);
  const [qMediaTempKey, setQMediaTempKey] = useState(null);
  const [qMediaType, setQMediaType] = useState(null);
  const [qUploadingMedia, setQUploadingMedia] = useState(false);
  const qFileInputRef = useRef(null);
  const [qLoading, setQLoading] = useState(false);
  const [qError, setQError] = useState("");
  const [showConfirmClose, setShowConfirmClose] = useState(false);
  
  // Bulk mode: Danh sách các câu hỏi đã tạo chờ bulk create
  const [pendingQuestions, setPendingQuestions] = useState([]);
  const [bulkLoading, setBulkLoading] = useState(false);
  
  // Danh sách các câu hỏi đã được tạo thành công (có ID từ server)
  const [createdQuestions, setCreatedQuestions] = useState([]);

  useEffect(() => {
    if (show && questionToUpdate) {
        // Chuẩn hóa options từ backend: Chuyển 'Text' thành 'text' để UI hoạt động
        let initialOptions = (questionToUpdate.options || []).map(opt => ({
            ...opt,
            text: opt.text || opt.Text || "",
            isCorrect: opt.isCorrect !== undefined ? opt.isCorrect : (opt.IsCorrect || false)
        }));

        let initialPairs = [];
        if (questionToUpdate.type === QUESTION_TYPES.Matching && questionToUpdate.correctAnswersJson) {
           try {
             const parsed = typeof questionToUpdate.correctAnswersJson === 'string' ? JSON.parse(questionToUpdate.correctAnswersJson) : questionToUpdate.correctAnswersJson; 
             if (Array.isArray(parsed)) initialPairs = parsed;
             else if (typeof parsed === 'object') initialPairs = Object.entries(parsed).map(([k, v]) => ({ key: k, value: v }));
           } catch (e) { initialPairs = [{ key: "", value: "" }]; }
        } else if (questionToUpdate.matchingPairs) {
            initialPairs = questionToUpdate.matchingPairs;
        }

        setQFormData({
          stemText: questionToUpdate.stemText || "",
          explanation: questionToUpdate.explanation || "",
          points: questionToUpdate.points || 10,
          type: questionToUpdate.type || QUESTION_TYPES.MultipleChoice,
          options: initialOptions,
          matchingPairs: initialPairs.length > 0 ? initialPairs : [{ key: "", value: "" }],
        });

        const url = questionToUpdate.mediaUrl || questionToUpdate.MediaUrl || questionToUpdate.mediaPreview;
        if (url) {
            setQMediaPreview(url);
            const lowerUrl = url.toLowerCase();
            if (lowerUrl.match(/\.(mp4|webm|mov)$/)) setQMediaType('video');
            else if (lowerUrl.match(/\.(mp3|wav|ogg)$/)) setQMediaType('audio');
            else setQMediaType('image');
        }
    }
  }, [show, questionToUpdate]);

  const resetQuestionForm = useCallback((type) => {
    let defaultOptions = [];
    let defaultPairs = [];
    if (type === QUESTION_TYPES.MultipleChoice || type === QUESTION_TYPES.MultipleAnswers) {
      defaultOptions = [{ text: "", isCorrect: false }, { text: "", isCorrect: false }, { text: "", isCorrect: false }, { text: "", isCorrect: false }];
    } else if (type === QUESTION_TYPES.TrueFalse) {
      defaultOptions = [{ text: "True", isCorrect: true }, { text: "False", isCorrect: false }];
    } else if (type === QUESTION_TYPES.Ordering) {
      defaultOptions = [{ text: "", isCorrect: true }, { text: "", isCorrect: true }, { text: "", isCorrect: true }];
    } else if (type === QUESTION_TYPES.Matching) {
      defaultPairs = [{ key: "", value: "" }, { key: "", value: "" }];
    }
    setQFormData({ 
      stemText: "", 
      explanation: "", 
      points: 10, 
      type: type, // Quan trọng: phải set type
      options: defaultOptions, 
      matchingPairs: defaultPairs 
    });
    setQMediaPreview(null); setQMediaTempKey(null); setQMediaType(null);
  }, []);

  // Reset form when show changes and no questionToUpdate
  useEffect(() => {
    if (show && !questionToUpdate) {
      // Reset về trạng thái ban đầu, không có loại câu hỏi
      setQFormData({
        stemText: "",
        explanation: "",
        points: 10,
        type: null,
        options: [],
        matchingPairs: [],
      });
      setQMediaPreview(null);
      setQMediaTempKey(null);
      setQMediaType(null);
      setQError("");
    }
  }, [show, questionToUpdate]);

  const handleQTypeChange = (e) => {
    const value = e.target.value;

    if (!value || value === '') {
      // Nếu chọn "Chọn loại câu hỏi", reset về trạng thái ban đầu
      setQFormData(prev => ({
        ...prev,
        stemText: "",
        explanation: "",
        points: 10,
        type: null,
        options: [],
        matchingPairs: [],
      }));
      return;
    }

    // HTML select luôn trả về string → ta ép về number
    const selectedType = Number(value);
    
    if (isNaN(selectedType)) {
      return;
    }
    
    resetQuestionForm(selectedType);
  };

  const handleOptionChange = (index, field, value) => {
    const newOptions = [...qFormData.options];
    if (field === "isCorrect" && (qFormData.type === QUESTION_TYPES.MultipleChoice || qFormData.type === QUESTION_TYPES.TrueFalse)) {
      newOptions.forEach((opt, i) => { opt.isCorrect = i === index; });
    } else { newOptions[index][field] = value; }
    setQFormData({ ...qFormData, options: newOptions });
  };

  const addOption = () => setQFormData({ ...qFormData, options: [...qFormData.options, { text: "", isCorrect: qFormData.type === QUESTION_TYPES.Ordering }] });
  const removeOption = (index) => setQFormData({ ...qFormData, options: qFormData.options.filter((_, i) => i !== index) });
  const moveOption = (index, direction) => {
    if ((direction === 'up' && index === 0) || (direction === 'down' && index === qFormData.options.length - 1)) return;
    const newOptions = [...qFormData.options];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    [newOptions[index], newOptions[targetIndex]] = [newOptions[targetIndex], newOptions[index]];
    setQFormData({ ...qFormData, options: newOptions });
  };

  const handlePairChange = (index, field, value) => {
    const newPairs = [...qFormData.matchingPairs];
    newPairs[index][field] = value;
    setQFormData({ ...qFormData, matchingPairs: newPairs });
  };
  const addPair = () => setQFormData({ ...qFormData, matchingPairs: [...qFormData.matchingPairs, { key: "", value: "" }] });
  const removePair = (index) => setQFormData({ ...qFormData, matchingPairs: qFormData.matchingPairs.filter((_, i) => i !== index) });

  const handleQMediaChange = async (e) => {
      const file = e.target.files?.[0];
      if (!file) return;
      if (file.size > 100 * 1024 * 1024) { setQError("File quá lớn (giới hạn 100MB)."); return; }
      setQUploadingMedia(true); setQError("");
      try {
          const previewUrl = URL.createObjectURL(file);
          setQMediaPreview(previewUrl);
          let type = file.type.startsWith('video/') ? 'video' : file.type.startsWith('audio/') ? 'audio' : 'image';
          setQMediaType(type);
          const response = await fileService.uploadTempFile(file, QUESTION_BUCKET, "temp");
          if (response.data?.success && response.data?.data) setQMediaTempKey(response.data.data.tempKey || response.data.data.TempKey);
          else setQError("Upload thất bại.");
      } catch (err) { setQError("Lỗi khi upload file."); } finally { setQUploadingMedia(false); }
  };

  const handleRemoveQMedia = () => {
      setQMediaPreview(null);
      setQMediaTempKey(null);
      setQMediaType(null);
      if (qFileInputRef.current) qFileInputRef.current.value = "";
  };

  // Check if form has data
  const hasFormData = () => {
    return (
      qFormData.stemText.trim() !== "" ||
      qFormData.explanation.trim() !== "" ||
      qFormData.points !== 10 ||
      qMediaPreview !== null ||
      gFormData.name.trim() !== "" ||
      gFormData.title.trim() !== "" ||
      gFormData.description.trim() !== "" ||
      pendingQuestions.length > 0
    );
  };

  // Handle close with confirmation
  const handleClose = () => {
    // Always allow closing if loading
    if (qLoading || gLoading || bulkLoading) {
      return; // Don't close if loading
    }
    
    // Check if form has data
    if (hasFormData()) {
      setShowConfirmClose(true);
    } else {
      setPendingQuestions([]); // Reset pending questions khi đóng
      setCreatedQuestions([]); // Reset created questions khi đóng
      onClose();
    }
  };

  // Handle confirm close
  const handleConfirmClose = () => {
    setShowConfirmClose(false);
    setPendingQuestions([]); // Reset pending questions khi đóng
    setCreatedQuestions([]); // Reset created questions khi đóng
    onClose();
  };

  // Helper function để build payload từ form data
  const buildQuestionPayload = (formData, mediaTempKey, mediaType) => {
    const pointsValue = typeof formData.points === 'string' 
      ? (formData.points.trim() === '' ? 10 : parseFloat(formData.points))
      : formData.points;
    
    const payload = {
      stemText: formData.stemText.trim(),
      explanation: formData.explanation || "",
      points: pointsValue || 10,
      type: formData.type,
      quizSectionId: sectionId || null,
      quizGroupId: internalGroupId || null,
      mediaTempKey: mediaTempKey || null,
      mediaType: mediaType || null,
      options: []
    };

    if (formData.type === QUESTION_TYPES.Matching) {
      const correctMatchesMap = {};
      const leftTexts = []; const rightTexts = [];
      formData.matchingPairs.forEach(p => {
          const leftText = (p.key || "").trim();
          const rightText = (p.value || "").trim();
          if (leftText && rightText) {
              correctMatchesMap[leftText] = rightText;
              leftTexts.push(leftText); rightTexts.push(rightText);
          }
      });
      payload.correctAnswersJson = JSON.stringify(correctMatchesMap);
      payload.metadataJson = JSON.stringify({ left: leftTexts, right: rightTexts });
      payload.options = [
          ...leftTexts.map(t => ({ text: t, isCorrect: true })),
          ...rightTexts.map(t => ({ text: t, isCorrect: false }))
      ];

    } else if (formData.type === QUESTION_TYPES.FillBlank) {
       const matches = [...formData.stemText.matchAll(/\[(.*?)\]/g)];
       const extractedAnswers = matches.map(m => m[1].trim());
       payload.correctAnswersJson = JSON.stringify(extractedAnswers);
       payload.options = extractedAnswers.map(ans => ({ text: ans, isCorrect: true }));

    } else if (formData.type === QUESTION_TYPES.Ordering) {
       payload.options = formData.options.map((opt, index) => ({
          text: (opt.text || opt.Text || "").trim(),
          displayOrder: index,
          isCorrect: true
       }));
       payload.correctAnswersJson = JSON.stringify(payload.options.map(o => o.text));
    } else {
      payload.options = formData.options.map(opt => ({
          text: (opt.text || opt.Text || "").trim(),
          isCorrect: !!opt.isCorrect,
          feedback: opt.feedback || null
      }));
    }

    return payload;
  };

  // Thêm câu hỏi vào danh sách chờ bulk (từ "Lưu và Thêm câu hỏi khác")
  const addToPendingList = () => {
    if (!qFormData.stemText.trim()) { 
      setQError("Vui lòng nhập nội dung câu hỏi"); 
      return; 
    }
    
    if (!qFormData.type) {
      setQError("Vui lòng chọn loại câu hỏi");
      return;
    }
    
    // Validate và parse points
    const pointsValue = typeof qFormData.points === 'string' 
      ? (qFormData.points.trim() === '' ? 10 : parseFloat(qFormData.points))
      : qFormData.points;
    
    if (isNaN(pointsValue) || pointsValue <= 0) {
      setQError("Điểm số phải lớn hơn 0");
      return;
    }

    const payload = buildQuestionPayload(qFormData, qMediaTempKey, qMediaType);
    
    // Thêm vào danh sách với preview data
    const questionPreview = {
      id: Date.now(), // Temporary ID
      payload: payload,
      preview: {
        stemText: qFormData.stemText.trim(),
        type: qFormData.type,
        points: pointsValue,
        optionsCount: qFormData.options.length,
        hasMedia: !!qMediaTempKey,
        matchingPairsCount: qFormData.matchingPairs?.length || 0
      }
    };
    
    setPendingQuestions(prev => [...prev, questionPreview]);
    setQError("");
    
    // Reset form để tạo câu hỏi tiếp theo
    resetQuestionForm(qFormData.type);
  };

  // Xóa câu hỏi khỏi danh sách
  const removeFromPendingList = (id) => {
    setPendingQuestions(prev => prev.filter(q => q.id !== id));
  };

  // Bulk create tất cả câu hỏi trong danh sách
  const handleBulkCreate = async () => {
    if (pendingQuestions.length === 0) {
      setQError("Danh sách câu hỏi trống");
      return;
    }

    setBulkLoading(true);
    setQError("");
    
    try {
      const bulkPayload = {
        questions: pendingQuestions.map(q => q.payload)
      };

      const response = isAdmin 
        ? await questionService.bulkCreateAdminQuestions(bulkPayload)
        : await questionService.bulkCreateQuestions(bulkPayload);
      
      if (response.data?.success) {
        // Lưu danh sách câu hỏi đã tạo thành công (có ID từ server)
        const createdIds = response.data.data?.createdQuestionIds || [];
        const newCreatedQuestions = pendingQuestions.map((q, idx) => ({
          ...q,
          questionId: createdIds[idx] || null, // ID từ server
          createdAt: new Date().toISOString()
        }));
        
        setCreatedQuestions(prev => [...prev, ...newCreatedQuestions]);
        
        // Xóa các câu đã tạo thành công khỏi danh sách chờ
        setPendingQuestions([]);
        
        // Gọi callback để refresh danh sách câu hỏi
        if (onSuccess) {
          onSuccess(response.data.data);
        }
        
        setQError("");
      } else {
        setQError(response.data?.message || "Có lỗi xảy ra khi tạo hàng loạt");
      }
    } catch (err) {
      setQError(err.response?.data?.message || "Lỗi kết nối khi tạo hàng loạt");
    } finally {
      setBulkLoading(false);
    }
  };
  
  // Xóa câu hỏi khỏi danh sách đã tạo (draft)
  const removeFromCreatedList = (id) => {
    setCreatedQuestions(prev => prev.filter(q => q.id !== id));
  };
  
  // Sửa câu hỏi từ danh sách đã tạo
  const editCreatedQuestion = (question) => {
    // Parse lại options từ payload
    let options = [];
    let matchingPairs = [];
    
    if (question.preview.type === QUESTION_TYPES.Matching) {
      // Parse matching pairs từ metadataJson
      try {
        if (question.payload.metadataJson) {
          const metadata = typeof question.payload.metadataJson === 'string' 
            ? JSON.parse(question.payload.metadataJson) 
            : question.payload.metadataJson;
          if (metadata.left && metadata.right) {
            matchingPairs = metadata.left.map((left, idx) => ({
              key: left,
              value: metadata.right[idx] || ""
            }));
          }
        }
      } catch (e) {
        console.error("Error parsing matching pairs:", e);
        matchingPairs = [{ key: "", value: "" }];
      }
    } else {
      // Parse options từ payload
      options = (question.payload.options || []).map(opt => ({
        text: opt.text || opt.Text || "",
        isCorrect: opt.isCorrect !== undefined ? opt.isCorrect : (opt.IsCorrect || false),
        feedback: opt.feedback || null
      }));
      
      // Nếu là TrueFalse và chưa có options, tạo mặc định
      if (question.preview.type === QUESTION_TYPES.TrueFalse && options.length === 0) {
        options = [{ text: "True", isCorrect: true }, { text: "False", isCorrect: false }];
      }
    }
    
    // Load câu hỏi vào form để sửa
    setQFormData({
      stemText: question.preview.stemText,
      explanation: question.payload.explanation || "",
      points: question.preview.points,
      type: question.preview.type,
      options: options,
      matchingPairs: matchingPairs.length > 0 ? matchingPairs : (question.preview.type === QUESTION_TYPES.Matching ? [{ key: "", value: "" }] : [])
    });
    
    // Load media nếu có (cần lưu lại mediaTempKey trong preview)
    // Note: Media đã được commit, nên không thể edit trực tiếp, cần upload lại nếu muốn thay đổi
    setQMediaPreview(null);
    setQMediaTempKey(null);
    setQMediaType(null);
    
    // Xóa câu hỏi khỏi danh sách đã tạo
    removeFromCreatedList(question.id);
  };

  const handleQuestionSubmit = async (isAddMore = false) => {
    if (!qFormData.stemText.trim()) { setQError("Vui lòng nhập nội dung câu hỏi"); return; }
    
    // Validate và parse points
    const pointsValue = typeof qFormData.points === 'string' 
      ? (qFormData.points.trim() === '' ? 10 : parseFloat(qFormData.points))
      : qFormData.points;
    
    if (isNaN(pointsValue) || pointsValue <= 0) {
      setQError("Điểm số phải lớn hơn 0");
      return;
    }

    // Nếu là "Lưu và Thêm câu hỏi khác" (isAddMore = true) và không phải update mode
    if (isAddMore && !questionToUpdate) {
      // Thêm vào danh sách chờ bulk thay vì gọi API ngay
      addToPendingList();
      return;
    }

    // Nếu là update hoặc tạo đơn lẻ (không bulk)
    setQLoading(true); setQError("");
    try {
      const payload = buildQuestionPayload(qFormData, qMediaTempKey, qMediaType);
      payload.questionId = questionToUpdate?.questionId;

      let response = questionToUpdate 
        ? (isAdmin ? await questionService.updateAdminQuestion(questionToUpdate.questionId, payload) : await questionService.updateQuestion(questionToUpdate.questionId, payload))
        : (isAdmin ? await questionService.createAdminQuestion(payload) : await questionService.createQuestion(payload));
      if (response.data?.success) {
        onSuccess(response.data.data);
        if (isAddMore) { resetQuestionForm(qFormData.type); setQError(""); } else onClose();
      } else { setQError(response.data?.message || "Có lỗi xảy ra"); }
    } catch (err) { setQError(err.response?.data?.message || "Lỗi kết nối"); } finally { setQLoading(false); }
  };

  const [gFormData, setGFormData] = useState({ name: "", title: "", description: "", sumScore: 0 });
  const [gMedia, setGMedia] = useState({ preview: null, tempKey: null, type: null, duration: null });
  const [gLoading, setGLoading] = useState(false);
  const [gError, setGError] = useState("");
  const gFileInputRef = useRef(null);

  const handleGMediaChange = async (e) => {
      const file = e.target.files?.[0];
      if (!file) return;
      let type = file.type.startsWith('image/') ? 'image' : file.type.startsWith('video/') ? 'video' : null;
      if (!type) { setGError("Chỉ hỗ trợ ảnh hoặc video cho Group"); return; }
      setGLoading(true);
      try {
          const preview = URL.createObjectURL(file);
          let duration = null;
          if (type === 'video') {
             const vid = document.createElement('video'); vid.src = preview;
             await new Promise(r => vid.onloadedmetadata = r); duration = Math.round(vid.duration);
          }
          const res = await fileService.uploadTempFile(file, QUIZ_GROUP_BUCKET, "temp");
          if (res.data?.success) { setGMedia({ preview, tempKey: res.data.data.tempKey || res.data.data.TempKey, type, duration }); setGError(""); }
      } catch (err) { setGError("Lỗi upload media group"); } finally { setGLoading(false); }
  };

  const handleGroupSubmit = async () => {
      if (!gFormData.name.trim() || !gFormData.title.trim()) { setGError("Tên và tiêu đề là bắt buộc"); return; }
      setGLoading(true); setGError("");
      try {
          const payload = { quizSectionId: parseInt(sectionId), name: gFormData.name.trim(), title: gFormData.title.trim(), description: gFormData.description, sumScore: parseFloat(gFormData.sumScore), imageTempKey: gMedia.type === 'image' ? gMedia.tempKey : null, imageType: gMedia.type === 'image' ? gMedia.type : null, videoTempKey: gMedia.type === 'video' ? gMedia.tempKey : null, videoType: gMedia.type === 'video' ? gMedia.type : null, videoDuration: gMedia.duration };
          const res = isAdmin
            ? await quizService.createAdminQuizGroup(payload)
            : await quizService.createQuizGroup(payload);
          if (res.data?.success) { setInternalGroupId(res.data.data.quizGroupId); setCreatedGroupName(res.data.data.name); setActiveTab("question"); setGFormData({ name: "", title: "", description: "", sumScore: 0 }); setGMedia({ preview: null, tempKey: null, type: null, duration: null }); }
      } catch (err) { setGError("Lỗi kết nối khi tạo group"); } finally { setGLoading(false); }
  };

  const renderMCQOptions = () => (
    <div className="options-grid"><Row>{qFormData.options.map((option, index) => (
            <Col md={6} key={index} className="mb-3"><div className="option-item d-flex align-items-center gap-2">
                    <Form.Check type={qFormData.type === QUESTION_TYPES.MultipleAnswers ? "checkbox" : "radio"} name="correctAnswer" checked={option.isCorrect} onChange={(e) => handleOptionChange(index, "isCorrect", e.target.checked)} className="scale-125" />
                    <Form.Control type="text" value={option.text || option.Text || ""} onChange={(e) => handleOptionChange(index, "text", e.target.value)} placeholder={`Đáp án ${index + 1}`} className="border-0 shadow-none fw-medium" />
                    {qFormData.type !== QUESTION_TYPES.TrueFalse && <Button variant="outline-danger" size="sm" className="border-0" onClick={() => removeOption(index)}><FaTrash /></Button>}
                </div></Col>))}</Row>
      {qFormData.type !== QUESTION_TYPES.TrueFalse && <Button variant="outline-primary" size="sm" onClick={addOption} className="mt-2"><FaPlus className="me-1" /> Thêm đáp án</Button>}
    </div>
  );

  const renderMatchingOptions = () => (
    <><div className="alert alert-info py-2 small mb-3"><FaQuestionCircle className="me-2"/>Nhập các cặp tương ứng. Backend sẽ dùng Text để đối chiếu ID.</div>
        {qFormData.matchingPairs.map((pair, index) => (
            <div key={index} className="option-item mb-2"><Row className="align-items-center g-2">
                    <Col md={5}><InputGroup><InputGroup.Text className="bg-light text-muted small fw-bold" style={{width: '40px'}}>{index + 1}A</InputGroup.Text><Form.Control type="text" value={pair.key} onChange={(e) => handlePairChange(index, "key", e.target.value)} placeholder="Vế trái" className="border-0 shadow-none" /></InputGroup></Col>
                    <Col md={1} className="text-center text-primary matching-arrow"><FaArrowDown style={{transform: 'rotate(-90deg)'}} /></Col>
                    <Col md={5}><InputGroup><InputGroup.Text className="bg-light text-muted small fw-bold" style={{width: '40px'}}>{index + 1}B</InputGroup.Text><Form.Control type="text" value={pair.value} onChange={(e) => handlePairChange(index, "value", e.target.value)} placeholder="Vế phải" className="border-0 shadow-none" /></InputGroup></Col>
                    <Col md={1} className="text-end"><Button variant="outline-danger" size="sm" className="border-0" onClick={() => removePair(index)}><FaTimes /></Button></Col>
                </Row></div>))}
        <Button variant="outline-primary" size="sm" onClick={addPair} className="mt-2"><FaPlus className="me-1" /> Thêm cặp nối</Button>
    </>
  );

  const renderOrderingOptions = () => (
      <><div className="alert alert-info py-2 small mb-3"><FaQuestionCircle className="me-2"/>Sắp xếp theo đúng thứ tự logic.</div>
        {qFormData.options.map((option, index) => (
            <div key={index} className="option-item mb-2 d-flex align-items-center gap-3"><span className="badge bg-secondary rounded-circle p-2 d-flex align-items-center justify-content-center" style={{width: '30px', height: '30px'}}>{index + 1}</span>
                <Form.Control type="text" value={option.text || option.Text || ""} onChange={(e) => handleOptionChange(index, "text", e.target.value)} placeholder={`Bước ${index + 1}`} className="border-0 shadow-none fw-medium flex-grow-1" />
                <div className="d-flex gap-1 order-controls bg-white border rounded p-1 shadow-sm">
                    <Button type="button" variant="outline-secondary" size="sm" disabled={index === 0} onClick={() => moveOption(index, 'up')}><FaArrowUp /></Button>
                    <Button type="button" variant="outline-secondary" size="sm" disabled={index === qFormData.options.length - 1} onClick={() => moveOption(index, 'down')}><FaArrowDown /></Button>
                </div>
                <Button variant="outline-danger" size="sm" className="border-0 ms-2" onClick={() => removeOption(index)}><FaTrash /></Button>
            </div>))}
        <Button variant="outline-primary" size="sm" onClick={addOption} className="mt-2"><FaPlus className="me-1" /> Thêm bước</Button>
      </>
  );

  // Renderer cho FillBlank
  const renderFillBlankInfo = () => (
    <div className="alert alert-info">
      <strong>Hướng dẫn:</strong> Dùng <code className="bg-light px-2 py-1 rounded">[từ đúng]</code> trong nội dung câu hỏi để tạo chỗ trống.
      <br />
      <small className="text-muted">Ví dụ: Hanoi is the [capital] of Vietnam.</small>
    </div>
  );

  // Map: type → hàm render
  const QUESTION_RENDERERS = {
    [QUESTION_TYPES.MultipleChoice]: () => (
      <div className="form-section-card">
        <div className="form-section-title">
          <FaQuestionCircle className="me-2" />
          Trắc nghiệm (1 đáp án đúng)
        </div>
        <div className="mt-3">
          {renderMCQOptions()}
        </div>
      </div>
    ),
    [QUESTION_TYPES.MultipleAnswers]: () => (
      <div className="form-section-card">
        <div className="form-section-title">
          <FaQuestionCircle className="me-2" />
          Trắc nghiệm (Nhiều đáp án đúng)
        </div>
        <div className="mt-3">
          {renderMCQOptions()}
        </div>
      </div>
    ),
    [QUESTION_TYPES.TrueFalse]: () => (
      <div className="form-section-card">
        <div className="form-section-title">
          <FaQuestionCircle className="me-2" />
          Đúng / Sai
        </div>
        <div className="mt-3">
          {renderMCQOptions()}
        </div>
      </div>
    ),
    [QUESTION_TYPES.Matching]: () => (
      <div className="form-section-card">
        <div className="form-section-title">
          <FaQuestionCircle className="me-2" />
          Nối từ (Matching)
        </div>
        <div className="mt-3">
          {renderMatchingOptions()}
        </div>
      </div>
    ),
    [QUESTION_TYPES.Ordering]: () => (
      <div className="form-section-card">
        <div className="form-section-title">
          <FaQuestionCircle className="me-2" />
          Sắp xếp (Ordering)
        </div>
        <div className="mt-3">
          {renderOrderingOptions()}
        </div>
      </div>
    ),
    [QUESTION_TYPES.FillBlank]: () => (
      <div className="form-section-card">
        <div className="form-section-title">
          <FaQuestionCircle className="me-2" />
          Điền từ (Fill in blanks)
        </div>
        <div className="mt-3">
          {renderFillBlankInfo()}
        </div>
      </div>
    ),
  };

  // Hàm render động theo type
  const renderQuestionFormat = () => {
    if (!qFormData.type) {
      return (
        <div className="alert alert-warning d-flex align-items-center">
          <FaQuestionCircle className="me-2" size={20} />
          <div>
            <strong>Vui lòng chọn loại câu hỏi</strong>
            <br />
            <small>Chọn loại câu hỏi ở trên để hiển thị form xây dựng câu hỏi phù hợp.</small>
          </div>
        </div>
      );
    }

    const renderer = QUESTION_RENDERERS[qFormData.type] || QUESTION_RENDERERS[Number(qFormData.type)];

    if (!renderer) {
      return (
        <div className="alert alert-danger">
          Loại câu hỏi không được hỗ trợ: {qFormData.type}
        </div>
      );
    }

    return renderer();
  };

  return (
    <>
    <Modal show={show} onHide={handleClose} backdrop={true} keyboard={true} centered className="modal-modern" dialogClassName="create-question-modal-xl">
      <Modal.Header><Modal.Title>{questionToUpdate ? "Cập nhật câu hỏi" : "Tạo mới nội dung"}</Modal.Title></Modal.Header>
      <Modal.Body className="p-0">
        <Tabs activeKey={activeTab} onSelect={(k) => setActiveTab(k)} className="px-3 pt-2 border-bottom-0 custom-tabs">
            <Tab eventKey="question" title={<span><FaQuestionCircle className="me-2"/>Câu hỏi</span>}>
                <div className="p-3 border-top"><div className="mb-3">
                        {internalGroupId ? (
                             <div className="alert alert-info py-2 small border-0 shadow-sm"><div className="d-flex justify-content-between align-items-center w-100">
                                    <div><FaLayerGroup className="me-2"/>Đang thêm vào nhóm: <strong>{groupInfo?.title || groupInfo?.name || createdGroupName || `Group #${internalGroupId}`}</strong></div>
                                    <div className="d-flex align-items-center gap-3">
                                        {groupInfo?.sumScore !== undefined && <Badge bg="secondary">Tổng điểm nhóm: {groupInfo.sumScore}</Badge>}
                                        {sectionInfo && <span className="text-muted fw-bold">| Section: {sectionInfo.title}</span>}
                                        <Button variant="link" size="sm" className="p-0 text-decoration-none ms-2 text-danger" onClick={() => {setInternalGroupId(null); setCreatedGroupName(""); setGroupInfo(null);}}><FaTimes className="me-1"/>Thoát nhóm</Button>
                                    </div>
                                 </div></div>
                        ) : (sectionInfo && <div className="alert alert-light py-2 small border shadow-sm"><FaQuestionCircle className="me-2 text-primary"/>Thêm câu hỏi lẻ vào Section: <strong>{sectionInfo.title}</strong></div>)}
                    </div>
                    {qError && <div className="alert alert-danger">{qError}</div>}
                    <Form><Row className="mb-3">
                            <Col md={5}><Form.Group><Form.Label>Loại câu hỏi</Form.Label><Form.Select 
                                    value={qFormData.type != null ? String(qFormData.type) : ''} 
                                    onChange={handleQTypeChange} 
                                    disabled={!!questionToUpdate}
                                >
                                    <option value="">-- Chọn loại câu hỏi muốn tạo --</option>
                                    <option value={String(QUESTION_TYPES.MultipleChoice)}>Trắc nghiệm (1 đáp án)</option>
                                    <option value={String(QUESTION_TYPES.MultipleAnswers)}>Trắc nghiệm (Nhiều đáp án)</option>
                                    <option value={String(QUESTION_TYPES.TrueFalse)}>Đúng / Sai</option>
                                    <option value={String(QUESTION_TYPES.FillBlank)}>Điền từ (Fill in blanks)</option>
                                    <option value={String(QUESTION_TYPES.Matching)}>Nối từ (Matching)</option>
                                    <option value={String(QUESTION_TYPES.Ordering)}>Sắp xếp (Ordering)</option>
                                </Form.Select></Form.Group></Col>
                            <Col md={4}><Form.Group><Form.Label>Điểm số</Form.Label><InputGroup><Form.Control 
                                        type="text" 
                                        inputMode="decimal"
                                        value={qFormData.points || ''} 
                                        onChange={(e) => {
                                          const value = e.target.value;
                                          if (value === '') {
                                            setQFormData(prev => ({ ...prev, points: '' }));
                                            return;
                                          }
                                          const numValue = value.replace(/[^\d.]/g, '');
                                          const parts = numValue.split('.');
                                          if (parts.length <= 2) {
                                            setQFormData(prev => ({ ...prev, points: numValue }));
                                          }
                                        }}
                                        onBlur={(e) => {
                                          const value = e.target.value.trim();
                                          if (value === '') {
                                            setQFormData(prev => ({ ...prev, points: 10 }));
                                            return;
                                          }
                                          const num = parseFloat(value);
                                          if (isNaN(num) || num <= 0) {
                                            setQFormData(prev => ({ ...prev, points: 10 }));
                                            return;
                                          }
                                          setQFormData(prev => ({ ...prev, points: num }));
                                        }}
                                        placeholder="Nhập điểm số"
                                      />
                                        {internalGroupId && groupInfo && <InputGroup.Text className="bg-light text-muted small">/ {groupInfo.sumScore} (Nhóm)</InputGroup.Text>}
                                    </InputGroup></Form.Group></Col>
                        </Row><Row>
                            <Col md={7}><Form.Group className="mb-3"><Form.Label>Nội dung câu hỏi</Form.Label><Form.Control as="textarea" rows={4} value={qFormData.stemText} onChange={(e) => setQFormData({ ...qFormData, stemText: e.target.value })} placeholder={qFormData.type === QUESTION_TYPES.FillBlank ? "Ví dụ: Hanoi is the [capital] of Vietnam." : "Nhập câu hỏi..."} /></Form.Group><Form.Group className="mb-3"><Form.Label>Giải thích (Optional)</Form.Label><Form.Control as="textarea" rows={2} value={qFormData.explanation} onChange={(e) => setQFormData({ ...qFormData, explanation: e.target.value })} /></Form.Group></Col>
                            <Col md={5}><Form.Group className="mb-3"><Form.Label>Media đính kèm</Form.Label><div className="border p-2 rounded bg-light text-center" style={{minHeight: '150px'}}>
                                        {!qMediaPreview ? (<div className="py-4 cursor-pointer" onClick={() => qFileInputRef.current?.click()}><FaPlus size={20} className="text-muted mb-2 d-block mx-auto"/><span className="text-muted small">{qUploadingMedia ? "Đang tải..." : "Chọn Ảnh/Video/Audio"}</span></div>) : (
                                            <div className="position-relative">
                                                {qMediaType === 'image' && <img src={qMediaPreview} alt="Preview" className="img-fluid rounded" style={{maxHeight:'180px'}}/>}
                                                {qMediaType === 'video' && <video src={qMediaPreview} controls style={{maxHeight:'180px', width:'100%'}}/>}
                                                {qMediaType === 'audio' && <audio src={qMediaPreview} controls style={{width:'100%'}}/>}
                                                <Button variant="danger" size="sm" className="position-absolute top-0 end-0 m-1" onClick={handleRemoveQMedia}><FaTimes/></Button>
                                            </div>)}
                                        <input type="file" ref={qFileInputRef} onChange={handleQMediaChange} style={{display:'none'}} accept="image/*,video/*,audio/*"/>
                                    </div></Form.Group></Col>
                        </Row>
                        
                        {/* Hiển thị format xây dựng câu hỏi dựa trên loại đã chọn */}
                        <div className="mt-4">
                          {renderQuestionFormat()}
                        </div>
                        
                        </Form>
                        
                        {/* Danh sách câu hỏi chờ bulk */}
                        {pendingQuestions.length > 0 && (
                          <div className="mt-4 p-3 bg-light rounded border">
                            <div className="d-flex justify-content-between align-items-center mb-3">
                              <h6 className="mb-0">
                                <Badge bg="warning" className="me-2">{pendingQuestions.length}</Badge>
                                Câu hỏi đã thêm (chờ tạo hàng loạt)
                              </h6>
                              <Button 
                                variant="primary" 
                                size="sm" 
                                onClick={handleBulkCreate} 
                                disabled={bulkLoading || qLoading || qUploadingMedia}
                              >
                                {bulkLoading ? "Đang tạo..." : `Tạo câu hỏi hàng loạt (${pendingQuestions.length} câu)`}
                              </Button>
                            </div>
                            <div className="pending-questions-list" style={{maxHeight: '200px', overflowY: 'auto'}}>
                              {pendingQuestions.map((q, idx) => (
                                <div key={q.id} className="d-flex justify-content-between align-items-start p-2 mb-2 bg-white rounded border">
                                  <div className="flex-grow-1">
                                    <div className="d-flex align-items-center gap-2 mb-1">
                                      <Badge bg="secondary">{idx + 1}</Badge>
                                      <strong className="small">
                                        {q.preview.type === QUESTION_TYPES.MultipleChoice && "Trắc nghiệm (1 đáp án)"}
                                        {q.preview.type === QUESTION_TYPES.MultipleAnswers && "Trắc nghiệm (Nhiều đáp án)"}
                                        {q.preview.type === QUESTION_TYPES.TrueFalse && "Đúng / Sai"}
                                        {q.preview.type === QUESTION_TYPES.FillBlank && "Điền từ"}
                                        {q.preview.type === QUESTION_TYPES.Matching && "Nối từ"}
                                        {q.preview.type === QUESTION_TYPES.Ordering && "Sắp xếp"}
                                      </strong>
                                      <Badge bg="success">{q.preview.points} điểm</Badge>
                                      {q.preview.hasMedia && <Badge bg="warning">Có media</Badge>}
                                    </div>
                                    <div className="text-muted small" style={{
                                      overflow: 'hidden',
                                      textOverflow: 'ellipsis',
                                      display: '-webkit-box',
                                      WebkitLineClamp: 2,
                                      WebkitBoxOrient: 'vertical'
                                    }}>
                                      {q.preview.stemText}
                                    </div>
                                    <div className="text-muted small mt-1">
                                      {q.preview.optionsCount > 0 ? `${q.preview.optionsCount} đáp án` : 
                                       q.preview.matchingPairsCount > 0 ? `${q.preview.matchingPairsCount} cặp nối` : 
                                       "N/A"}
                                    </div>
                                  </div>
                                  <Button 
                                    variant="outline-danger" 
                                    size="sm" 
                                    className="ms-2"
                                    onClick={() => removeFromPendingList(q.id)}
                                  >
                                    <FaTrash />
                                  </Button>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                        
                        {/* Bảng draft - Danh sách câu hỏi đã được tạo thành công */}
                        {createdQuestions.length > 0 && (
                          <div className="mt-4 p-3 bg-success bg-opacity-10 rounded border border-success">
                            <div className="d-flex justify-content-between align-items-center mb-3">
                              <h6 className="mb-0 text-success">
                                <Badge bg="success" className="me-2">{createdQuestions.length}</Badge>
                                Câu hỏi đã tạo thành công
                              </h6>
                            </div>
                            <div className="table-responsive">
                              <table className="table table-sm table-hover bg-white">
                                <thead>
                                  <tr>
                                    <th style={{width: '5%'}}>#</th>
                                    <th style={{width: '10%'}}>Loại</th>
                                    <th style={{width: '40%'}}>Nội dung</th>
                                    <th style={{width: '8%'}}>Điểm</th>
                                    <th style={{width: '10%'}}>Đáp án</th>
                                    <th style={{width: '12%'}}>ID</th>
                                    <th style={{width: '15%'}}>Thao tác</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {createdQuestions.map((q, idx) => (
                                    <tr key={q.id}>
                                      <td><Badge bg="secondary">{idx + 1}</Badge></td>
                                      <td>
                                        <small>
                                          {q.preview.type === QUESTION_TYPES.MultipleChoice && "Trắc nghiệm (1)"}
                                          {q.preview.type === QUESTION_TYPES.MultipleAnswers && "Trắc nghiệm (N)"}
                                          {q.preview.type === QUESTION_TYPES.TrueFalse && "Đúng/Sai"}
                                          {q.preview.type === QUESTION_TYPES.FillBlank && "Điền từ"}
                                          {q.preview.type === QUESTION_TYPES.Matching && "Nối từ"}
                                          {q.preview.type === QUESTION_TYPES.Ordering && "Sắp xếp"}
                                        </small>
                                      </td>
                                      <td>
                                        <div style={{
                                          overflow: 'hidden',
                                          textOverflow: 'ellipsis',
                                          display: '-webkit-box',
                                          WebkitLineClamp: 2,
                                          WebkitBoxOrient: 'vertical',
                                          maxWidth: '300px'
                                        }}>
                                          {q.preview.stemText}
                                        </div>
                                      </td>
                                      <td><Badge bg="info">{q.preview.points}</Badge></td>
                                      <td>
                                        <small className="text-muted">
                                          {q.preview.optionsCount > 0 ? `${q.preview.optionsCount} đáp án` : 
                                           q.preview.matchingPairsCount > 0 ? `${q.preview.matchingPairsCount} cặp` : 
                                           "-"}
                                        </small>
                                      </td>
                                      <td>
                                        {q.questionId ? (
                                          <Badge bg="primary">#{q.questionId}</Badge>
                                        ) : (
                                          <Badge bg="secondary">Chưa có</Badge>
                                        )}
                                      </td>
                                      <td>
                                        <div className="d-flex gap-1">
                                          <Button 
                                            variant="outline-primary" 
                                            size="sm" 
                                            onClick={() => editCreatedQuestion(q)}
                                            title="Sửa"
                                          >
                                            <FaEdit />
                                          </Button>
                                          <Button 
                                            variant="outline-danger" 
                                            size="sm" 
                                            onClick={() => removeFromCreatedList(q.id)}
                                            title="Xóa"
                                          >
                                            <FaTrash />
                                          </Button>
                                        </div>
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                          </div>
                        )}
                        
                    <div className="d-flex justify-content-end gap-2 mt-4">
                      <Button variant="secondary" onClick={handleClose}>Đóng</Button>
                      {!questionToUpdate && (
                        <Button 
                          variant="success" 
                          onClick={() => handleQuestionSubmit(true)} 
                          disabled={qLoading || qUploadingMedia || bulkLoading}
                        >
                          {qLoading ? "Đang lưu..." : "Lưu và Thêm câu hỏi khác"}
                        </Button>
                      )}
                      <Button 
                        variant="primary" 
                        onClick={() => handleQuestionSubmit(false)} 
                        disabled={qLoading || qUploadingMedia || bulkLoading}
                      >
                        {qLoading ? "Đang xử lý..." : questionToUpdate ? "Lưu thay đổi" : "Tạo câu hỏi"}
                      </Button>
                    </div></div></Tab>
            {sectionId && !groupId && !questionToUpdate && (
                <Tab eventKey="group" title={<span><FaLayerGroup className="me-2"/>Tạo nhóm câu hỏi</span>}>
                     <div className="p-4 border-top"><div className="alert alert-light border shadow-sm mb-4"><h6 className="alert-heading fw-bold"><FaLayerGroup className="me-2"/>Tạo Group mới</h6><p className="mb-0 small text-muted">Tạo một nhóm câu hỏi (ví dụ: Bài đọc hiểu, Bài nghe) sau đó bạn có thể thêm nhiều câu hỏi con vào nhóm này.</p></div>
                        {gError && <div className="alert alert-danger">{gError}</div>}
                        <Form><Row><Col md={8}><Form.Group className="mb-3"><Form.Label className="required">Tên nhóm (Mã định danh)</Form.Label><Form.Control type="text" value={gFormData.name} onChange={e => setGFormData({...gFormData, name: e.target.value})} placeholder="VD: Reading Passage 1"/></Form.Group><Form.Group className="mb-3"><Form.Label className="required">Tiêu đề hiển thị</Form.Label><Form.Control type="text" value={gFormData.title} onChange={e => setGFormData({...gFormData, title: e.target.value})} placeholder="VD: Đọc đoạn văn sau và trả lời câu hỏi..."/></Form.Group><Form.Group className="mb-3"><Form.Label>Nội dung / Đoạn văn / Mô tả</Form.Label><Form.Control as="textarea" rows={5} value={gFormData.description} onChange={e => setGFormData({...gFormData, description: e.target.value})}/></Form.Group></Col>
                                <Col md={4}><Form.Group className="mb-3"><Form.Label>Tổng điểm nhóm</Form.Label><Form.Control type="number" value={gFormData.sumScore} onChange={e => setGFormData({...gFormData, sumScore: e.target.value})}/></Form.Group>
                                    <Form.Group className="mb-3"><Form.Label>Media nhóm (Ảnh/Video)</Form.Label><div className="border p-2 rounded bg-light text-center" style={{minHeight: '200px'}}>
                                            {!gMedia.preview ? (<div className="py-5 cursor-pointer" onClick={() => gFileInputRef.current?.click()}><FaPlus size={24} className="text-muted mb-2 d-block mx-auto"/><span className="text-muted">{gLoading ? "Đang tải..." : "Upload Media"}</span></div>) : (
                                                <div className="position-relative">{gMedia.type === 'image' && <img src={gMedia.preview} alt="Preview" className="img-fluid rounded" />}
                                                    {gMedia.type === 'video' && <video src={gMedia.preview} controls style={{width:'100%'}} />}
                                                    <Button variant="danger" size="sm" className="position-absolute top-0 end-0 m-1" onClick={() => setGMedia({preview: null, tempKey: null, type: null, duration: null})}><FaTimes/></Button></div>)}
                                            <input type="file" ref={gFileInputRef} onChange={handleGMediaChange} style={{display:'none'}} accept="image/*,video/*"/>
                                        </div></Form.Group></Col></Row></Form>
                        <div className="d-flex justify-content-end gap-2 mt-4"><Button variant="secondary" onClick={handleClose}>Hủy</Button><Button variant="success" onClick={handleGroupSubmit} disabled={gLoading}>{gLoading ? "Đang tạo..." : "Tạo Group và Thêm câu hỏi"}</Button></div></div></Tab>)}
        </Tabs></Modal.Body></Modal>

    <ConfirmModal
      show={showConfirmClose}
      onHide={() => setShowConfirmClose(false)}
      onConfirm={handleConfirmClose}
      title="Xác nhận đóng"
      message="Bạn có dữ liệu chưa lưu. Bạn có chắc chắn muốn đóng không?"
      confirmText="Đóng"
      cancelText="Tiếp tục chỉnh sửa"
      variant="warning"
    />
    </>
  );
}
