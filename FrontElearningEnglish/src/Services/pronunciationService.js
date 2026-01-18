import axiosClient from "./axiosClient";
import { API_ENDPOINTS } from "./apiConfig";

export const pronunciationService = {
    // Tạo pronunciation assessment
    assess: (data) => axiosClient.post(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.ASSESS, data),

    // Lấy danh sách flashcard với pronunciation progress theo module
    getByModule: (moduleId) => axiosClient.get(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.GET_BY_MODULE(moduleId)),

    // Lấy summary/statistics của pronunciation cho module
    getModuleSummary: (moduleId) => axiosClient.get(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.GET_MODULE_SUMMARY(moduleId)),
};

