import axiosClient from "./axiosClient";
import { API_ENDPOINTS } from "./apiConfig";

export const pronunciationService = {
    // Tạo pronunciation assessment
    assess: (data) => axiosClient.post(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.ASSESS, data),

    // Lấy danh sách flashcard với pronunciation progress theo module
    getByModule: (moduleId) => axiosClient.get(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.GET_BY_MODULE(moduleId)),

    // Lấy danh sách flashcard với pronunciation progress theo module (paginated)
    getByModulePaginated: (moduleId, pageNumber = 1, pageSize = 10) =>
        axiosClient.get(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.GET_BY_MODULE_PAGINATED(moduleId), {
            params: { pageNumber, pageSize }
        }),

    // Lấy tất cả pronunciation assessments
    getAll: () => axiosClient.get(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.GET_ALL),

    // Lấy summary/statistics của pronunciation cho module
    getModuleSummary: (moduleId) => axiosClient.get(API_ENDPOINTS.PRONUNCIATION_ASSESSMENTS.GET_MODULE_SUMMARY(moduleId)),
};

