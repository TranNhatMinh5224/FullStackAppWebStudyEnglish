import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import { PublicRoute, RoleDashboardRedirect, AdminRoute, TeacherRoute, StudentRoute } from "./components/common/RoleBasedRoute";
import WelcomeScreen from "./pages/Welcome/WelcomeScreen";
import IntroScreen from "./pages/Intro/IntroScreen";
import HomeScreen from "./pages/Home/User/HomeScreen";
import LoginScreen from "./pages/Login/LoginScreen";
import RegisterScreen from "./pages/Register/RegisterScreen";
import UpdateProfileScreen from "./pages/UpdateProfile/UpdateProfileScreen";
import ForgotPasswordScreen from "./pages/ForgotPassword/ForgotPasswordScreen";
import OTPVerificationScreen from "./pages/ForgotPassword/OTPVerificationScreen";
import ResetPasswordScreen from "./pages/ForgotPassword/ResetPasswordScreen";
import TipsPage from "./pages/Tips/TipsPage";
import AdminDashboard from "./pages/Dashboard/Admin/AdminDashboard";
import TeacherDashboard from "./pages/Dashboard/Teacher/TeacherDashboard";
import Unauthorized from "./pages/Unauthorized/Unauthorized";
// Course pages
import { CoursesPage, CourseDetailPage, MyCoursesPage } from "./pages/Courses";

// Loading component
const LoadingSpinner = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100vh',
    background: 'linear-gradient(135deg, #FFCB08 0%, #FFA000 100%)',
    fontFamily: 'var(--font-family)',
    fontSize: '18px',
    color: '#333'
  }}>
    Đang tải...
  </div>
);

// Main app content component
const AppContent = () => {
  const [showIntro, setShowIntro] = useState(true);
  const { loading } = useAuth();

  const handleIntroComplete = () => {
    setShowIntro(false);
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  if (showIntro) {
    return <IntroScreen onIntroComplete={handleIntroComplete} />;
  }

  return (
    <Router>
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<WelcomeScreen />} />
        <Route path="/login" element={<PublicRoute><LoginScreen /></PublicRoute>} />
        <Route path="/register" element={<PublicRoute><RegisterScreen /></PublicRoute>} />
        <Route path="/forgot-password" element={<PublicRoute><ForgotPasswordScreen /></PublicRoute>} />
        <Route path="/otp-verification" element={<PublicRoute><OTPVerificationScreen /></PublicRoute>} />
        <Route path="/reset-password" element={<PublicRoute><ResetPasswordScreen /></PublicRoute>} />
        
        {/* Home cho cả guest và user đã đăng nhập */}
        <Route path="/home" element={<HomeScreen />} />
        <Route path="/tips" element={<TipsPage />} />
        
        {/* Course routes - Public access for browsing */}
        <Route path="/courses" element={<CoursesPage />} />
        <Route path="/courses/:courseId" element={<CourseDetailPage />} />
        
        {/* My Courses - Protected route */}
        <Route path="/my-courses" element={<StudentRoute><MyCoursesPage /></StudentRoute>} />
        
        {/* Dashboard redirect based on role */}
        <Route path="/dashboard" element={<RoleDashboardRedirect />} />
        
        {/* Admin routes */}
        <Route path="/admin/dashboard" element={<AdminRoute><AdminDashboard /></AdminRoute>} />
        
        {/* Teacher routes */}
        <Route path="/teacher/dashboard" element={<TeacherRoute><TeacherDashboard /></TeacherRoute>} />
        
        {/* Protected routes chỉ cho user đã đăng nhập */}
        <Route path="/profile/update" element={<StudentRoute><UpdateProfileScreen /></StudentRoute>} />
        
        {/* Unauthorized page */}
        <Route path="/unauthorized" element={<Unauthorized />} />
      </Routes>
    </Router>
  );
};

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
