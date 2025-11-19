import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { Navigate } from 'react-router-dom';

/**
 * Role-based Route Protection Component
 * Redirects users to appropriate dashboard based on their role
 */
export const RoleBasedRoute = ({ children, allowedRoles = [], fallbackPath = '/home' }) => {
  const { isLoggedIn, userRole, userRoles, loading } = useAuth();

  // Show loading while checking auth
  if (loading) {
    return (
      <div className="loading-container">
        <div>Đang tải...</div>
      </div>
    );
  }

  // Not logged in
  if (!isLoggedIn) {
    return <Navigate to="/login" replace />;
  }

  // No role restrictions - allow access
  if (allowedRoles.length === 0) {
    return children;
  }

  // Check if user has any of the allowed roles
  const hasPermission = allowedRoles.some(role => 
    userRoles.includes(role) || userRole === role
  );

  if (!hasPermission) {
    return <Navigate to={fallbackPath} replace />;
  }

  return children;
};

/**
 * Role-based Dashboard Redirect Component
 * Automatically redirects users to their appropriate dashboard
 */
export const RoleDashboardRedirect = () => {
  const { isLoggedIn, userRole, loading } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div>Đang tải...</div>
      </div>
    );
  }

  if (!isLoggedIn) {
    return <Navigate to="/login" replace />;
  }

  // Redirect based on role
  switch (userRole) {
    case 'Admin':
      return <Navigate to="/admin/dashboard" replace />;
    case 'Teacher':
      return <Navigate to="/teacher/dashboard" replace />;
    case 'Student':
    case 'User':
      return <Navigate to="/home" replace />;
    default:
      return <Navigate to="/home" replace />;
  }
};

/**
 * Public Route Component (for login, register pages)
 * Redirects logged-in users to their dashboard
 */
export const PublicRoute = ({ children }) => {
  const { isLoggedIn, userRole, loading } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div>Đang tải...</div>
      </div>
    );
  }

  if (isLoggedIn) {
    // Redirect to appropriate dashboard based on role
    switch (userRole) {
      case 'Admin':
        return <Navigate to="/admin/dashboard" replace />;
      case 'Teacher':
        return <Navigate to="/teacher/dashboard" replace />;
      case 'Student':
      case 'User':
        return <Navigate to="/home" replace />;
      default:
        return <Navigate to="/home" replace />;
    }
  }

  return children;
};

/**
 * Admin Only Route Component
 */
export const AdminRoute = ({ children }) => {
  return (
    <RoleBasedRoute allowedRoles={['Admin']} fallbackPath="/unauthorized">
      {children}
    </RoleBasedRoute>
  );
};

/**
 * Teacher Only Route Component
 */
export const TeacherRoute = ({ children }) => {
  return (
    <RoleBasedRoute allowedRoles={['Teacher']} fallbackPath="/unauthorized">
      {children}
    </RoleBasedRoute>
  );
};

/**
 * Student Only Route Component
 */
export const StudentRoute = ({ children }) => {
  return (
    <RoleBasedRoute allowedRoles={['Student', 'User']} fallbackPath="/unauthorized">
      {children}
    </RoleBasedRoute>
  );
};

/**
 * Teacher and Admin Route Component
 */
export const TeacherAdminRoute = ({ children }) => {
  return (
    <RoleBasedRoute allowedRoles={['Teacher', 'Admin']} fallbackPath="/unauthorized">
      {children}
    </RoleBasedRoute>
  );
};

export default {
  RoleBasedRoute,
  RoleDashboardRedirect,
  PublicRoute,
  AdminRoute,
  TeacherRoute,
  StudentRoute,
  TeacherAdminRoute
};