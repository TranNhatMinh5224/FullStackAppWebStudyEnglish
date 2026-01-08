import React, { useState, useRef } from "react";
import { Button } from "react-bootstrap";
import { FaChevronLeft, FaChevronRight, FaVolumeUp } from "react-icons/fa";
import PronunciationProgress from "../PronunciationProgress/PronunciationProgress";
import PronunciationMic from "../PronunciationMic/PronunciationMic";
import { pronunciationService } from "../../../Services/pronunciationService";
import { fileService } from "../../../Services/fileService";
import "./PronunciationCard.css";

export default function PronunciationCard({
    flashcard,
    currentIndex,
    totalCards,
    onNext,
    onPrevious,
    canGoNext,
    canGoPrevious,
    onAssessmentComplete,
    onComplete
}) {
    const [isRecording, setIsRecording] = useState(false);
    const [isProcessing, setIsProcessing] = useState(false);
    const [assessmentResult, setAssessmentResult] = useState(null);
    const [audioBlob, setAudioBlob] = useState(null);
    const [recordedAudioUrl, setRecordedAudioUrl] = useState(null);
    const [isPlayingRecorded, setIsPlayingRecorded] = useState(false);
    const [isPlayingReference, setIsPlayingReference] = useState(false);
    const mediaRecorderRef = useRef(null);
    const audioChunksRef = useRef([]);
    const recordedAudioPlayerRef = useRef(null);
    const referenceAudioPlayerRef = useRef(null);
    const referenceBlobUrlRef = useRef(null);

    const word = flashcard?.word || flashcard?.Word || "";
    const definition = flashcard?.definition || flashcard?.Definition || "";
    const phonetic = flashcard?.phonetic || flashcard?.Phonetic || "";
    const audioUrl = flashcard?.audioUrl || flashcard?.AudioUrl || "";
    const flashCardId = flashcard?.flashCardId || flashcard?.FlashCardId;
    
    // Debug: Log audioUrl to check if it's being passed correctly
    React.useEffect(() => {
        console.log("🔊 [PronunciationCard] Flashcard data:", {
            word,
            audioUrl,
            flashCardId,
            audioUrlFromFlashcard: flashcard?.audioUrl || flashcard?.AudioUrl,
            fullFlashcard: flashcard
        });
    }, [flashCardId, audioUrl]);
    const progress = flashcard?.progress || flashcard?.Progress;
    const bestScore = progress?.bestScore || progress?.BestScore || 0;
    const hasPracticed = progress?.hasPracticed || progress?.HasPracticed || false;

    // Reset state when flashcard changes
    React.useEffect(() => {
        // Cleanup previous recorded audio
        if (recordedAudioUrl) {
            URL.revokeObjectURL(recordedAudioUrl);
            setRecordedAudioUrl(null);
        }
        if (recordedAudioPlayerRef.current) {
            recordedAudioPlayerRef.current.pause();
            recordedAudioPlayerRef.current = null;
        }
        if (referenceAudioPlayerRef.current) {
            referenceAudioPlayerRef.current.pause();
            referenceAudioPlayerRef.current = null;
        }
        if (referenceBlobUrlRef.current) {
            URL.revokeObjectURL(referenceBlobUrlRef.current);
            referenceBlobUrlRef.current = null;
        }

        // Reset states
        setAudioBlob(null);
        setAssessmentResult(null);
        setIsRecording(false);
        setIsProcessing(false);
        setIsPlayingRecorded(false);
        setIsPlayingReference(false);
        audioChunksRef.current = [];
    }, [flashCardId]);

    const handleStartRecording = async () => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const mediaRecorder = new MediaRecorder(stream);
            mediaRecorderRef.current = mediaRecorder;
            audioChunksRef.current = [];

            mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    audioChunksRef.current.push(event.data);
                }
            };

            mediaRecorder.onstop = async () => {
                const audioBlob = new Blob(audioChunksRef.current, { type: 'audio/webm' });
                setAudioBlob(audioBlob);

                // Create audio URL for playback
                const url = URL.createObjectURL(audioBlob);
                setRecordedAudioUrl(url);

                // Calculate duration from audio blob
                const audio = new Audio(url);

                audio.addEventListener('loadedmetadata', async () => {
                    const duration = audio.duration;
                    await handleProcessRecording(audioBlob, duration);
                });

                audio.addEventListener('error', async () => {
                    console.warn("Could not load audio metadata, proceeding without duration");
                    await handleProcessRecording(audioBlob, null);
                });

                // Fallback: if metadata doesn't load, proceed without duration
                setTimeout(async () => {
                    if (audio.readyState < 2) { // HAVE_CURRENT_DATA
                        await handleProcessRecording(audioBlob, null);
                    }
                }, 1000);

                // Stop all tracks
                stream.getTracks().forEach(track => track.stop());
            };

            mediaRecorder.start();
            setIsRecording(true);
        } catch (err) {
            console.error("Error starting recording:", err);
            alert("Không thể truy cập microphone. Vui lòng kiểm tra quyền truy cập.");
        }
    };

    const handleStopRecording = () => {
        if (mediaRecorderRef.current && isRecording) {
            mediaRecorderRef.current.stop();
            setIsRecording(false);
        }
    };

    const handleProcessRecording = async (audioBlob, audioDuration = null) => {
        if (!flashCardId) {
            console.error("FlashCard ID is missing");
            return;
        }

        try {
            setIsProcessing(true);
            setAssessmentResult(null);

            console.log("📤 [PronunciationCard] Starting audio upload...");

            // Upload audio to temp storage (MinIO)
            const audioFile = new File([audioBlob], `pronunciation_${Date.now()}.webm`, {
                type: 'audio/webm'
            });

            const uploadResponse = await fileService.uploadTempFile(
                audioFile,
                "pronunciations",
                "temp"
            );

            console.log("📥 [PronunciationCard] Upload response:", uploadResponse.data);

            if (!uploadResponse.data?.success || !uploadResponse.data?.data) {
                throw new Error("Không thể upload audio lên MinIO");
            }

            // Handle both camelCase and PascalCase from backend
            const resultData = uploadResponse.data.data;
            const tempKey = resultData.TempKey || resultData.tempKey;
            let audioType = resultData.ImageType || resultData.imageType || resultData.AudioType || resultData.audioType || 'audio/webm';
            const audioSize = audioBlob.size;

            // Ensure AudioType doesn't exceed 50 characters (backend validation)
            if (audioType && audioType.length > 50) {
                audioType = audioType.substring(0, 50);
            }

            // Calculate duration if available
            // Only include DurationInSeconds if > 0 (backend validator requires > 0 if provided)
            let durationInSeconds = null;
            if (audioDuration && !isNaN(audioDuration) && audioDuration > 0) {
                durationInSeconds = audioDuration;
            } else if (audioBlob.duration && !isNaN(audioBlob.duration) && audioBlob.duration > 0) {
                durationInSeconds = audioBlob.duration;
            }

            if (!tempKey) {
                throw new Error("Không nhận được TempKey từ server sau khi upload");
            }

            console.log("✅ [PronunciationCard] Audio uploaded successfully:", {
                tempKey,
                audioType,
                audioSize,
                durationInSeconds
            });

            // Call pronunciation assessment API with tempKey
            // Backend expects: FlashCardId (required), AudioTempKey (required), AudioType (optional, max 50), AudioSize (optional, > 0), DurationInSeconds (optional, > 0)
            const assessmentData = {
                FlashCardId: flashCardId,
                AudioTempKey: tempKey,
            };

            // Only add optional fields if they have valid values
            if (audioType && audioType.length > 0 && audioType.length <= 50) {
                assessmentData.AudioType = audioType;
            }

            if (audioSize && audioSize > 0) {
                assessmentData.AudioSize = audioSize;
            }

            if (durationInSeconds && durationInSeconds > 0) {
                assessmentData.DurationInSeconds = durationInSeconds;
            }

            console.log("📤 [PronunciationCard] Calling assessment API with data:", assessmentData);

            const assessmentResponse = await pronunciationService.assess(assessmentData);

            console.log("📥 [PronunciationCard] Assessment response:", assessmentResponse.data);
            console.log("📥 [PronunciationCard] Full response:", JSON.stringify(assessmentResponse.data, null, 2));

            // Backend returns ServiceResponse<PronunciationAssessmentDto>
            // Structure: { Success: bool, StatusCode: int, Message: string, Data: PronunciationAssessmentDto }
            // Data contains: PronunciationScore, AccuracyScore, FluencyScore, CompletenessScore (all PascalCase)
            const responseData = assessmentResponse.data;
            const isSuccess = responseData?.Success || responseData?.success;
            const resultDto = responseData?.Data || responseData?.data;

            if (isSuccess && resultDto) {
                // Backend returns PascalCase fields: PronunciationScore, AccuracyScore, etc.
                console.log("📊 [PronunciationCard] Score fields from backend (PascalCase):", {
                    PronunciationScore: resultDto.PronunciationScore,
                    AccuracyScore: resultDto.AccuracyScore,
                    FluencyScore: resultDto.FluencyScore,
                    CompletenessScore: resultDto.CompletenessScore,
                    Feedback: resultDto.Feedback,
                    Status: resultDto.Status,
                    fullResult: resultDto
                });

                // Store the result (backend uses PascalCase)
                setAssessmentResult(resultDto);

                console.log("✅ [PronunciationCard] Assessment completed successfully");

                // Notify parent component
                if (onAssessmentComplete) {
                    onAssessmentComplete(resultDto);
                }
            } else {
                const errorMessage = responseData?.Message || responseData?.message || "Không thể đánh giá phát âm";
                console.error("❌ [PronunciationCard] Assessment failed:", errorMessage);
                console.error("❌ [PronunciationCard] Full error response:", responseData);
                throw new Error(errorMessage);
            }
        } catch (err) {
            console.error("❌ [PronunciationCard] Error processing recording:", err);
            console.error("❌ [PronunciationCard] Error details:", {
                message: err.message,
                response: err.response?.data,
                status: err.response?.status
            });

            const errorMessage = err.response?.data?.message || err.message || "Không thể xử lý bản ghi âm. Vui lòng thử lại.";
            alert(errorMessage);
        } finally {
            setIsProcessing(false);
        }
    };

    const handlePlayRecordedAudio = () => {
        if (!recordedAudioUrl || !audioBlob) return;

        if (recordedAudioPlayerRef.current) {
            recordedAudioPlayerRef.current.pause();
            recordedAudioPlayerRef.current = null;
        }

        const audio = new Audio(recordedAudioUrl);
        recordedAudioPlayerRef.current = audio;

        audio.onended = () => {
            setIsPlayingRecorded(false);
            recordedAudioPlayerRef.current = null;
        };

        audio.onerror = () => {
            setIsPlayingRecorded(false);
            recordedAudioPlayerRef.current = null;
            alert("Không thể phát lại bản ghi âm");
        };

        audio.play().then(() => {
            setIsPlayingRecorded(true);
        }).catch((err) => {
            console.error("Error playing recorded audio:", err);
            setIsPlayingRecorded(false);
        });
    };

    const handleStopRecordedPlayback = () => {
        if (recordedAudioPlayerRef.current) {
            recordedAudioPlayerRef.current.pause();
            recordedAudioPlayerRef.current.currentTime = 0;
            recordedAudioPlayerRef.current = null;
            setIsPlayingRecorded(false);
        }
    };

    const handlePlayReferenceAudio = async (e) => {
        if (e) {
            e.stopPropagation();
        }
        if (!audioUrl) {
            console.warn("No audio URL provided");
            return;
        }

        try {
            // Stop any currently playing audio
            if (referenceAudioPlayerRef.current) {
                referenceAudioPlayerRef.current.pause();
                referenceAudioPlayerRef.current.src = "";
                referenceAudioPlayerRef.current = null;
            }
            
            // Clean up previous blob URL if exists
            if (referenceBlobUrlRef.current) {
                URL.revokeObjectURL(referenceBlobUrlRef.current);
                referenceBlobUrlRef.current = null;
            }
            
            // Try fetching audio as blob first (to bypass CORS if possible) - EXACT SAME AS FLASHCARD
            try {
                const response = await fetch(audioUrl, {
                    method: 'GET',
                    headers: {
                        'Accept': 'audio/mpeg, audio/*',
                    },
                    mode: 'cors', // Try CORS first
                });
                
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                
                const blob = await response.blob();
                const blobUrl = URL.createObjectURL(blob);
                referenceBlobUrlRef.current = blobUrl;
                const audio = new Audio(blobUrl);
                
                audio.onended = () => {
                    setIsPlayingReference(false);
                    referenceAudioPlayerRef.current = null;
                    // Clean up blob URL when audio ends
                    if (referenceBlobUrlRef.current) {
                        URL.revokeObjectURL(referenceBlobUrlRef.current);
                        referenceBlobUrlRef.current = null;
                    }
                };
                
                referenceAudioPlayerRef.current = audio;
                await audio.play();
                setIsPlayingReference(true);
                console.log("Reference audio playing successfully via blob");
            } catch (fetchError) {
                // If fetch fails, try direct audio URL - EXACT SAME AS FLASHCARD
                console.log("Fetch failed, trying direct audio URL:", fetchError);
                const audio = new Audio(audioUrl);
                referenceAudioPlayerRef.current = audio;
                await audio.play();
                setIsPlayingReference(true);
                console.log("Reference audio playing successfully via direct URL");
            }
        } catch (err) {
            console.error("Error playing reference audio:", err);
            console.error("Error name:", err.name);
            console.error("Error message:", err.message);
            console.error("Audio URL:", audioUrl);
            setIsPlayingReference(false);
            
            // Clean up on error
            if (referenceAudioPlayerRef.current) {
                referenceAudioPlayerRef.current.pause();
                referenceAudioPlayerRef.current = null;
            }
            if (referenceBlobUrlRef.current) {
                URL.revokeObjectURL(referenceBlobUrlRef.current);
                referenceBlobUrlRef.current = null;
            }
            
            // Silent fail - don't show alert as it might be annoying - EXACT SAME AS FLASHCARD
            // User can check console for details
        }
    };

    const handleStopReferencePlayback = () => {
        if (referenceAudioPlayerRef.current) {
            referenceAudioPlayerRef.current.pause();
            referenceAudioPlayerRef.current.currentTime = 0;
            referenceAudioPlayerRef.current = null;
            setIsPlayingReference(false);
        }
        // Clean up blob URL if exists
        if (referenceBlobUrlRef.current) {
            URL.revokeObjectURL(referenceBlobUrlRef.current);
            referenceBlobUrlRef.current = null;
        }
    };

    // Cleanup audio URLs on unmount
    React.useEffect(() => {
        return () => {
            if (recordedAudioUrl) {
                URL.revokeObjectURL(recordedAudioUrl);
            }
            if (recordedAudioPlayerRef.current) {
                recordedAudioPlayerRef.current.pause();
                recordedAudioPlayerRef.current = null;
            }
            if (referenceAudioPlayerRef.current) {
                referenceAudioPlayerRef.current.pause();
                referenceAudioPlayerRef.current = null;
            }
            if (referenceBlobUrlRef.current) {
                URL.revokeObjectURL(referenceBlobUrlRef.current);
                referenceBlobUrlRef.current = null;
            }
        };
    }, [recordedAudioUrl]);


    // Get pronunciation score from result - Backend returns PascalCase
    const getPronunciationScore = () => {
        if (assessmentResult) {
            // Backend returns PascalCase: PronunciationScore, AccuracyScore, FluencyScore, CompletenessScore
            // PronunciationScore is the main overall score
            const score = assessmentResult.PronunciationScore !== undefined
                ? assessmentResult.PronunciationScore
                : assessmentResult.pronunciationScore !== undefined
                    ? assessmentResult.pronunciationScore
                    : null;

            console.log("🔍 [PronunciationCard] Getting PronunciationScore:", {
                PronunciationScore: assessmentResult.PronunciationScore,
                pronunciationScore: assessmentResult.pronunciationScore,
                foundScore: score,
                allScores: {
                    PronunciationScore: assessmentResult.PronunciationScore,
                    AccuracyScore: assessmentResult.AccuracyScore,
                    FluencyScore: assessmentResult.FluencyScore,
                    CompletenessScore: assessmentResult.CompletenessScore
                },
                fullResult: assessmentResult
            });

            // Return score even if it's 0 (0 is a valid score)
            if (score !== undefined && score !== null && !isNaN(score)) {
                return parseFloat(score);
            }
        }
        // If no assessment result, return best score from progress
        return bestScore || 0;
    };

    const pronunciationScore = getPronunciationScore();
    // Show score if we have assessment result (even if score is 0) or if user has practiced before
    const showScore = assessmentResult !== null || hasPracticed;

    // Log for debugging
    if (assessmentResult) {
        console.log("🎯 [PronunciationCard] Displaying score:", {
            pronunciationScore,
            showScore,
            assessmentResult: {
                PronunciationScore: assessmentResult.PronunciationScore,
                AccuracyScore: assessmentResult.AccuracyScore,
                FluencyScore: assessmentResult.FluencyScore,
                CompletenessScore: assessmentResult.CompletenessScore,
                Feedback: assessmentResult.Feedback,
                Status: assessmentResult.Status
            }
        });
    }

    const isLastCard = currentIndex === totalCards - 1;
    const handleNextOrComplete = () => {
        if (isLastCard && onComplete) {
            onComplete();
        } else if (onNext) {
            onNext();
        }
    };

    return (
        <div className="pronunciation-card">
            <div className="pronunciation-card-header">
                <div className="pronunciation-content">
                    <PronunciationProgress
                        score={pronunciationScore}
                        showScore={showScore}
                        feedback={
                            assessmentResult
                                ? (assessmentResult.Feedback || assessmentResult.feedback || "Chưa tính điểm")
                                : hasPracticed
                                    ? `Điểm tốt nhất: ${Math.round(bestScore)}`
                                    : "Chưa tính điểm"
                        }
                    />

                    <div className="word-display">
                        <h2 className="word-text">{word}</h2>
                        {phonetic && (
                            <p className="phonetic-text">{phonetic}</p>
                        )}
                        {definition && (
                            <p className="definition-text">{definition}</p>
                        )}
                    </div>

                    <div className="pronunciation-instruction">
                        <p>Nhấn vào mic để phát âm</p>
                    </div>

                    <PronunciationMic
                        isRecording={isRecording}
                        isProcessing={isProcessing}
                        onStartRecording={handleStartRecording}
                        onStopRecording={handleStopRecording}
                    />
                </div>
            </div>

            <div className="pronunciation-card-actions">
                {canGoPrevious && (
                    <Button
                        variant="outline-primary"
                        className="nav-button prev-button"
                        onClick={onPrevious}
                    >
                        <FaChevronLeft className="me-2" />
                        Từ trước
                    </Button>
                )}

                {audioBlob && recordedAudioUrl && (
                    <Button
                        variant="outline-primary"
                        className="playback-button"
                        onClick={isPlayingRecorded ? handleStopRecordedPlayback : handlePlayRecordedAudio}
                    >
                        <FaVolumeUp className="me-2" />
                        {isPlayingRecorded ? "Dừng" : "Nghe lại"}
                    </Button>
                )}

                {assessmentResult && audioUrl && audioUrl.trim() !== "" && (
                    <Button
                        variant="outline-success"
                        className="reference-audio-button"
                        onClick={isPlayingReference ? handleStopReferencePlayback : handlePlayReferenceAudio}
                    >
                        <FaVolumeUp className="me-2" />
                        {isPlayingReference ? "Dừng" : "Nghe phát âm chuẩn"}
                    </Button>
                )}

                {(canGoNext || isLastCard) && (
                    <Button
                        variant="primary"
                        className="nav-button next-button"
                        onClick={handleNextOrComplete}
                    >
                        {isLastCard ? "Hoàn thành" : "Từ tiếp theo"}
                        {!isLastCard && <FaChevronRight className="ms-2" />}
                    </Button>
                )}
            </div>
        </div>
    );
}

