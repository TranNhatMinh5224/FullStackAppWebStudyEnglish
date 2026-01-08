import axiosClient from "./axiosClient";
import { API_ENDPOINTS } from "./apiConfig";

export const moduleService = {
    getModulesByCourseId: (courseId) => axiosClient.get(API_ENDPOINTS.MODULES.GET_BY_COURSE(courseId)),
    
    getModulesByLessonId: (lessonId) => axiosClient.get(API_ENDPOINTS.MODULES.GET_BY_LESSON(lessonId)),
    
    getModuleById: (moduleId) => axiosClient.get(API_ENDPOINTS.MODULES.GET_BY_ID(moduleId)),
    
    startModule: (moduleId) => axiosClient.post(API_ENDPOINTS.MODULES.START(moduleId)),
};

