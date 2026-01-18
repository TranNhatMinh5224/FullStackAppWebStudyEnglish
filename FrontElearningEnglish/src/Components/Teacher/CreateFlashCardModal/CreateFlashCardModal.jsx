import React, { useState, useEffect, useRef } from "react";
import { Modal, Button, Row, Col, Form } from "react-bootstrap";
import { FaImage, FaMusic, FaSearch, FaMagic, FaTimes, FaBook, FaTags, FaFileAlt } from "react-icons/fa";
import { flashcardService } from "../../../Services/flashcardService";
import { fileService } from "../../../Services/fileService";
import GenerateFlashcardModal from "../GenerateFlashcardModal/GenerateFlashcardModal";
import LookupWordModal from "../LookupWordModal/LookupWordModal";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import "./CreateFlashCardModal.css";

const FLASHCARD_IMAGE_BUCKET = "flashcards";
const FLASHCARD_AUDIO_BUCKET = "flashcard-audio";

export default function CreateFlashCardModal({ show, onClose, onSuccess, moduleId, flashcardToUpdate, isAdmin = false }) {
  const isEditMode = !!flashcardToUpdate;

  // Form state
  const [word, setWord] = useState("");
  const [meaning, setMeaning] = useState("");
  const [pronunciation, setPronunciation] = useState("");
  const [partOfSpeech, setPartOfSpeech] = useState("");
  const [example, setExample] = useState("");
  const [exampleTranslation, setExampleTranslation] = useState("");
  const [synonyms, setSynonyms] = useState("");
  const [antonyms, setAntonyms] = useState("");
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  // Modals state
  const [showGenerateModal, setShowGenerateModal] = useState(false);
  const [showLookupModal, setShowLookupModal] = useState(false);
  const [showConfirmClose, setShowConfirmClose] = useState(false);

  // Media state
  const [imagePreview, setImagePreview] = useState(null);
  const [imageTempKey, setImageTempKey] = useState(null);
  const [imageType, setImageType] = useState(null);
  const [uploadingImage, setUploadingImage] = useState(false);
  const imageInputRef = useRef(null);

  const [audioPreview, setAudioPreview] = useState(null);
  const [audioTempKey, setAudioTempKey] = useState(null);
  const [audioType, setAudioType] = useState(null);
  const [uploadingAudio, setUploadingAudio] = useState(false);
  const audioInputRef = useRef(null);

  // Refs for auto-resize textareas
  const exampleTextareaRef = useRef(null);
  const exampleTranslationTextareaRef = useRef(null);

  // Auto-resize textarea function
  const autoResizeTextarea = (textareaRef) => {
    if (textareaRef?.current) {
      textareaRef.current.style.height = 'auto';
      const maxHeight = 120; // Limit max height to 120px
      const newHeight = Math.min(textareaRef.current.scrollHeight, maxHeight);
      textareaRef.current.style.height = `${newHeight}px`;
    }
  };

  // Auto-resize when example or exampleTranslation changes
  useEffect(() => {
    // Small delay to ensure DOM is updated
    setTimeout(() => {
      autoResizeTextarea(exampleTextareaRef);
    }, 0);
  }, [example]);

  useEffect(() => {
    // Small delay to ensure DOM is updated
    setTimeout(() => {
      autoResizeTextarea(exampleTranslationTextareaRef);
    }, 0);
  }, [exampleTranslation]);

  useEffect(() => {
    if (show) {
      if (flashcardToUpdate) {
        setWord(flashcardToUpdate.word || flashcardToUpdate.Word || "");
        setMeaning(flashcardToUpdate.meaning || flashcardToUpdate.Meaning || "");
        setPronunciation(flashcardToUpdate.pronunciation || flashcardToUpdate.Pronunciation || "");
        setPartOfSpeech(flashcardToUpdate.partOfSpeech || flashcardToUpdate.PartOfSpeech || "");
        setExample(flashcardToUpdate.example || flashcardToUpdate.Example || "");
        setExampleTranslation(flashcardToUpdate.exampleTranslation || flashcardToUpdate.ExampleTranslation || "");
        setSynonyms(flashcardToUpdate.synonyms || flashcardToUpdate.Synonyms || "");
        setAntonyms(flashcardToUpdate.antonyms || flashcardToUpdate.Antonyms || "");
        
        setImagePreview(flashcardToUpdate.imageUrl || flashcardToUpdate.ImageUrl || null);
        setAudioPreview(flashcardToUpdate.audioUrl || flashcardToUpdate.AudioUrl || null);
        setImageTempKey(null);
        setAudioTempKey(null);
      } else {
        resetForm();
      }
      setErrors({});
    }
  }, [show, flashcardToUpdate]);

  // Reset textarea height when modal closes
  useEffect(() => {
    if (!show) {
      if (exampleTextareaRef.current) {
        exampleTextareaRef.current.style.height = 'auto';
      }
      if (exampleTranslationTextareaRef.current) {
        exampleTranslationTextareaRef.current.style.height = 'auto';
      }
    }
  }, [show]);

  const resetForm = () => {
    setWord("");
    setMeaning("");
    setPronunciation("");
    setPartOfSpeech("");
    setExample("");
    setExampleTranslation("");
    setSynonyms("");
    setAntonyms("");
    setImagePreview(null);
    setAudioPreview(null);
    setImageTempKey(null);
    setAudioTempKey(null);
    setShowConfirmClose(false);
  };

  // Check if form has data
  const hasFormData = () => {
    return (
      word.trim() !== "" ||
      meaning.trim() !== "" ||
      pronunciation.trim() !== "" ||
      partOfSpeech.trim() !== "" ||
      example.trim() !== "" ||
      imagePreview !== null ||
      audioPreview !== null
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

  const handleImageChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      setErrors({ ...errors, image: "Kích thước ảnh tối đa 5MB" });
      return;
    }

    setUploadingImage(true);
    try {
      const preview = URL.createObjectURL(file);
      setImagePreview(preview);
      
      const res = await fileService.uploadTempFile(file, FLASHCARD_IMAGE_BUCKET, "temp");
      if (res.data?.success) {
        setImageTempKey(res.data.data.TempKey || res.data.data.tempKey);
        setImageType(res.data.data.ImageType || res.data.data.imageType || file.type);
        setErrors({...errors, image: null});
      } else {
        setErrors({...errors, image: "Upload thất bại"});
      }
    } catch (err) {
      console.error(err);
      setErrors({...errors, image: "Lỗi upload ảnh"});
    } finally {
      setUploadingImage(false);
      if(imageInputRef.current) imageInputRef.current.value = "";
    }
  };

  const handleAudioChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      setErrors({ ...errors, audio: "Kích thước audio tối đa 10MB" });
      return;
    }

    setUploadingAudio(true);
    try {
      const preview = URL.createObjectURL(file);
      setAudioPreview(preview);
      
      const res = await fileService.uploadTempFile(file, FLASHCARD_AUDIO_BUCKET, "temp");
      if (res.data?.success) {
        setAudioTempKey(res.data.data.TempKey || res.data.data.tempKey);
        setAudioType(res.data.data.AudioType || res.data.data.audioType || file.type);
        setErrors({...errors, audio: null});
      } else {
        setErrors({...errors, audio: "Upload thất bại"});
      }
    } catch (err) {
      console.error(err);
      setErrors({...errors, audio: "Lỗi upload audio"});
    } finally {
      setUploadingAudio(false);
      if(audioInputRef.current) audioInputRef.current.value = "";
    }
  };

  const handleGenerateSuccess = (data) => {
    if(data.word) setWord(data.word);
    if(data.meaning) setMeaning(data.meaning);
    if(data.pronunciation) setPronunciation(data.pronunciation);
    if(data.partOfSpeech) setPartOfSpeech(data.partOfSpeech);
    if(data.example) setExample(data.example);
    if(data.exampleTranslation) setExampleTranslation(data.exampleTranslation);
    
    // Handle synonyms and antonyms (Backend returns JSON string)
    if(data.synonyms) setSynonyms(data.synonyms);
    if(data.antonyms) setAntonyms(data.antonyms);
    
    if(data.imageUrl || data.imageTempKey) {
        setImagePreview(data.imageUrl);
        setImageTempKey(data.imageTempKey);
        setImageType(data.imageType);
    }
    if(data.audioUrl || data.audioTempKey) {
        setAudioPreview(data.audioUrl);
        setAudioTempKey(data.audioTempKey);
        setAudioType(data.audioType);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Clear previous errors
    const newErrors = {};
    
    // Validate required fields
    if (!word.trim()) {
        newErrors.word = "Từ vựng là bắt buộc";
    }
    if (!meaning.trim()) {
        newErrors.meaning = "Nghĩa là bắt buộc";
    }
    if (!pronunciation.trim()) {
        newErrors.pronunciation = "Phiên âm là bắt buộc";
    }
    if (!partOfSpeech.trim()) {
        newErrors.partOfSpeech = "Từ loại là bắt buộc";
    }
    if (!imagePreview && !imageTempKey) {
        newErrors.image = "Ảnh là bắt buộc";
    }
    if (!audioPreview && !audioTempKey) {
        newErrors.audio = "Âm thanh là bắt buộc";
    }
    
    // If there are errors, set them and return
    if (Object.keys(newErrors).length > 0) {
        setErrors(newErrors);
        return;
    }

    setSubmitting(true);
    try {
        const payload = {
            word: word.trim(),
            meaning: meaning.trim(),
            pronunciation: pronunciation.trim(),
            partOfSpeech: partOfSpeech.trim(),
            example: example.trim() || null,
            exampleTranslation: exampleTranslation.trim() || null,
            synonyms: synonyms.trim() || null,
            antonyms: antonyms.trim() || null,
            imageTempKey: imageTempKey,
            audioTempKey: audioTempKey,
            imageType: imageType,
            audioType: audioType
        };

        let res;
        if (isEditMode) {
            const flashcardId = flashcardToUpdate.flashcardId || flashcardToUpdate.FlashcardId;
            res = isAdmin
                ? await flashcardService.updateAdminFlashcard(flashcardId, payload)
                : await flashcardService.updateFlashcard(flashcardId, payload);
        } else {
            payload.moduleId = parseInt(moduleId);
            res = isAdmin
                ? await flashcardService.createAdminFlashcard(payload)
                : await flashcardService.createFlashcard(payload);
        }

        if (res.data?.success) {
            onSuccess(res.data.data);
            onClose();
        } else {
            throw new Error(res.data?.message || "Lỗi thao tác");
        }
    } catch (err) {
        console.error(err);
        setErrors({submit: err.response?.data?.message || "Có lỗi xảy ra"});
    } finally {
        setSubmitting(false);
    }
  };

  return (
    <>
    <Modal show={show} onHide={handleClose} backdrop="static" keyboard={false} centered size="xl" className="create-flashcard-modal modal-modern" dialogClassName="create-flashcard-modal-dialog">
      <Modal.Header closeButton>
        <Modal.Title className="modal-title-custom">
          {isEditMode ? "Cập nhật Flashcard" : "Tạo Flashcard mới"}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {!isEditMode && (
            <div className="d-flex justify-content-end mb-3 gap-2">
                <Button variant="outline-info" size="sm" onClick={() => setShowLookupModal(true)}><FaSearch className="me-1"/> Tra từ</Button>
                <Button variant="outline-primary" size="sm" onClick={() => setShowGenerateModal(true)}><FaMagic className="me-1"/> AI Gen</Button>
            </div>
        )}
        <Form onSubmit={handleSubmit}>
            {/* SECTION 1: THÔNG TIN CƠ BẢN */}
            <div className="form-section">
                <div className="section-title"><FaBook /> Thông tin cơ bản</div>
                <Row className="g-3">
                    <Col md={6}>
                        <Form.Label className="fw-bold">Từ vựng <span className="text-danger">*</span></Form.Label>
                        <Form.Control
                            type="text"
                            value={word}
                            onChange={e => {
                                setWord(e.target.value);
                                if (errors.word) setErrors({...errors, word: null});
                            }}
                            placeholder="Nhập từ vựng tiếng Anh"
                            isInvalid={!!errors.word}
                        />
                        {errors.word && <Form.Control.Feedback type="invalid" className="d-block">{errors.word}</Form.Control.Feedback>}
                    </Col>
                    <Col md={6}>
                        <Form.Label className="fw-bold">Phiên âm <span className="text-danger">*</span></Form.Label>
                        <Form.Control
                            type="text"
                            value={pronunciation}
                            onChange={e => {
                                setPronunciation(e.target.value);
                                if (errors.pronunciation) setErrors({...errors, pronunciation: null});
                            }}
                            placeholder="Nhập phiên âm IPA (VD: /ˈæp.l/)"
                            isInvalid={!!errors.pronunciation}
                        />
                        {errors.pronunciation && <Form.Control.Feedback type="invalid" className="d-block">{errors.pronunciation}</Form.Control.Feedback>}
                    </Col>
                    <Col md={12}>
                        <Form.Label className="fw-bold">Nghĩa <span className="text-danger">*</span></Form.Label>
                        <Form.Control
                            type="text"
                            value={meaning}
                            onChange={e => {
                                setMeaning(e.target.value);
                                if (errors.meaning) setErrors({...errors, meaning: null});
                            }}
                            placeholder="Nhập nghĩa tiếng Việt"
                            isInvalid={!!errors.meaning}
                        />
                        {errors.meaning && <Form.Control.Feedback type="invalid" className="d-block">{errors.meaning}</Form.Control.Feedback>}
                    </Col>
                    <Col md={12}>
                        <Form.Label className="fw-bold">Từ loại <span className="text-danger">*</span></Form.Label>
                        <Form.Control
                            type="text"
                            value={partOfSpeech}
                            onChange={e => {
                                setPartOfSpeech(e.target.value);
                                if (errors.partOfSpeech) setErrors({...errors, partOfSpeech: null});
                            }}
                            placeholder="Nhập từ loại (VD: Noun, Verb, Adjective)"
                            isInvalid={!!errors.partOfSpeech}
                        />
                        {errors.partOfSpeech && <Form.Control.Feedback type="invalid" className="d-block">{errors.partOfSpeech}</Form.Control.Feedback>}
                    </Col>
                </Row>
            </div>

            {/* SECTION 2: MEDIA */}
            <div className="form-section">
                <div className="section-title"><FaImage /> Hình ảnh & Âm thanh</div>
                <Row className="g-3">
                    <Col md={6}>
                        <Form.Label className="fw-bold">Ảnh minh họa <span className="text-danger">*</span></Form.Label>
                        <div className="media-upload-area" onClick={() => !imagePreview && imageInputRef.current?.click()}>
                            {imagePreview ? (
                                <div className="position-relative media-preview">
                                    <img src={imagePreview} alt="Preview" className="img-fluid"/>
                                    <Button variant="danger" size="sm" className="position-absolute top-0 end-0 m-2" onClick={(e) => {e.stopPropagation(); setImagePreview(null); setImageTempKey(null);}}><FaTimes/></Button>
                                </div>
                            ) : (
                                <div className="media-upload-placeholder">
                                    <FaImage size={32} className="text-muted mb-2"/>
                                    <span className="text-muted">{uploadingImage ? "Đang tải..." : "Chọn Ảnh"}</span>
                                </div>
                            )}
                            <input type="file" ref={imageInputRef} onChange={handleImageChange} style={{display:'none'}} accept="image/*"/>
                        </div>
                        {errors.image && <div className="text-danger small mt-1">{errors.image}</div>}
                    </Col>
                    <Col md={6}>
                        <Form.Label className="fw-bold">Âm thanh <span className="text-danger">*</span></Form.Label>
                        <div className="media-upload-area" onClick={() => !audioPreview && audioInputRef.current?.click()}>
                            {audioPreview ? (
                                <div className="w-100">
                                    <audio controls src={audioPreview} style={{width: '100%'}}/>
                                    <Button variant="danger" size="sm" className="mt-2 w-100" onClick={(e) => {e.stopPropagation(); setAudioPreview(null); setAudioTempKey(null);}}><FaTimes className="me-1"/> Xóa audio</Button>
                                </div>
                            ) : (
                                <div className="media-upload-placeholder">
                                    <FaMusic size={32} className="text-muted mb-2"/>
                                    <span className="text-muted">{uploadingAudio ? "Đang tải..." : "Chọn Audio"}</span>
                                </div>
                            )}
                            <input type="file" ref={audioInputRef} onChange={handleAudioChange} style={{display:'none'}} accept="audio/*"/>
                        </div>
                        {errors.audio && <div className="text-danger small mt-1">{errors.audio}</div>}
                    </Col>
                </Row>
            </div>

            {/* SECTION 3: VÍ DỤ */}
            <div className="form-section">
                <div className="section-title"><FaFileAlt /> Ví dụ và bản dịch</div>
                <Row className="g-3">
                    <Col md={6}>
                        <Form.Label>Ví dụ</Form.Label>
                        <Form.Control
                            as="textarea"
                            ref={exampleTextareaRef}
                            rows={1}
                            value={example}
                            onChange={e => {
                                setExample(e.target.value);
                                autoResizeTextarea(exampleTextareaRef);
                            }}
                            placeholder="Nhập câu ví dụ tiếng Anh"
                            className="auto-resize-textarea"
                        />
                    </Col>
                    <Col md={6}>
                        <Form.Label>Dịch ví dụ</Form.Label>
                        <Form.Control
                            as="textarea"
                            ref={exampleTranslationTextareaRef}
                            rows={1}
                            value={exampleTranslation}
                            onChange={e => {
                                setExampleTranslation(e.target.value);
                                autoResizeTextarea(exampleTranslationTextareaRef);
                            }}
                            placeholder="Nhập bản dịch tiếng Việt"
                            className="auto-resize-textarea"
                        />
                    </Col>
                </Row>
            </div>

            {/* SECTION 4: TỪ ĐỒNG NGHĨA & TRÁI NGHĨA */}
            <div className="form-section">
                <div className="section-title"><FaTags /> Từ đồng nghĩa & Trái nghĩa</div>
                <Row className="g-3">
                    <Col md={6}>
                        <Form.Label>Từ đồng nghĩa</Form.Label>
                        <Form.Control
                            type="text"
                            value={synonyms}
                            onChange={e => setSynonyms(e.target.value)}
                            placeholder='["pretty", "gorgeous"] hoặc pretty, gorgeous'
                        />
                        <small className="text-muted d-block mt-1">JSON array hoặc danh sách cách nhau bởi dấu phẩy</small>
                    </Col>
                    <Col md={6}>
                        <Form.Label>Từ trái nghĩa</Form.Label>
                        <Form.Control
                            type="text"
                            value={antonyms}
                            onChange={e => setAntonyms(e.target.value)}
                            placeholder='["ugly", "unattractive"] hoặc ugly, unattractive'
                        />
                        <small className="text-muted d-block mt-1">JSON array hoặc danh sách cách nhau bởi dấu phẩy</small>
                    </Col>
                </Row>
            </div>

            {errors.submit && <div className="alert alert-danger mt-3">{errors.submit}</div>}
        </Form>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={handleClose} disabled={submitting}>Hủy</Button>
        <Button className="btn-primary-custom" onClick={handleSubmit} disabled={submitting || !word || !meaning || !pronunciation || !partOfSpeech}>
            {submitting ? "Đang lưu..." : (isEditMode ? "Cập nhật Flashcard" : "Tạo Flashcard")}
        </Button>
      </Modal.Footer>

      <GenerateFlashcardModal show={showGenerateModal} onClose={() => setShowGenerateModal(false)} onGenerate={handleGenerateSuccess} />
      <LookupWordModal show={showLookupModal} onClose={() => setShowLookupModal(false)} />
    </Modal>

    {/* Confirm Close Modal */}
    <ConfirmModal
      isOpen={showConfirmClose}
      onClose={() => setShowConfirmClose(false)}
      onConfirm={handleConfirmClose}
      title="Xác nhận đóng"
      message={`Bạn có dữ liệu chưa được lưu. Bạn có chắc chắn muốn ${isEditMode ? "hủy cập nhật" : "hủy tạo"} Flashcard không?`}
      confirmText="Đóng"
      cancelText="Tiếp tục"
      type="warning"
    />
    </>
  );
}