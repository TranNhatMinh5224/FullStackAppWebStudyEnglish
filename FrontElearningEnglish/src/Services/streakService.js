import axiosClient from "./axiosClient";
import { API_ENDPOINTS } from "./apiConfig";

export const streakService = {
    // Lấy thông tin streak hiện tại
    getMyStreak: () => axiosClient.get(API_ENDPOINTS.STREAKS.GET_MY_STREAK),
    
    // Check-in streak (cập nhật streak khi user online)
    checkinStreak: () => axiosClient.post(API_ENDPOINTS.STREAKS.CHECKIN),
};

