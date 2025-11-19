/**
 * JWT Utilities for token decoding and role extraction
 */

// Decode JWT token without verification (for frontend use only)
export const decodeJWT = (token) => {
  try {
    if (!token) return null;
    
    // JWT has 3 parts separated by dots: header.payload.signature
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    
    // Decode payload (middle part)
    const payload = parts[1];
    
    // Add padding if needed for base64 decoding
    const paddedPayload = payload + '='.repeat((4 - payload.length % 4) % 4);
    
    // Decode base64
    const decodedPayload = atob(paddedPayload);
    
    // Parse JSON
    return JSON.parse(decodedPayload);
  } catch (error) {
    console.error('Error decoding JWT:', error);
    return null;
  }
};

// Extract user roles from JWT token
export const extractRolesFromToken = (token) => {
  try {
    const decoded = decodeJWT(token);
    if (!decoded) {
      console.warn('[jwtUtils] Token decode failed');
      return [];
    }
    
    console.log('[jwtUtils] Decoded token:', decoded);
    console.log('[jwtUtils] All token keys:', Object.keys(decoded));
    
    // Backend uses ClaimTypes.Role which maps to: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    // When there are multiple roles, JWT may store them as:
    // 1. An array: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] = ['Admin', 'Teacher']
    // 2. Multiple claims with same key (less common in JSON, but possible)
    // 3. A single string: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] = 'Student'
    
    const roleClaimKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    let roles = [];
    
    // Try to get roles from the standard claim
    if (decoded[roleClaimKey]) {
      const roleValue = decoded[roleClaimKey];
      if (Array.isArray(roleValue)) {
        roles = roleValue;
      } else if (typeof roleValue === 'string') {
        roles = [roleValue];
      }
    }
    
    // Fallback to other possible formats
    if (roles.length === 0) {
      if (decoded.role) {
        roles = Array.isArray(decoded.role) ? decoded.role : [decoded.role];
      } else if (decoded.roles) {
        roles = Array.isArray(decoded.roles) ? decoded.roles : [decoded.roles];
      }
    }
    
    console.log('[jwtUtils] Extracted roles:', {
      roleClaimKey,
      roleClaimValue: decoded[roleClaimKey],
      extractedRoles: roles,
      allKeys: Object.keys(decoded)
    });
    
    return roles;
  } catch (error) {
    console.error('[jwtUtils] Error extracting roles from token:', error);
    return [];
  }
};

// Get primary role (first role in the list)
export const getPrimaryRole = (token) => {
  const roles = extractRolesFromToken(token);
  return roles.length > 0 ? roles[0] : null;
};

// Check if user has specific role
export const hasRole = (token, roleName) => {
  const roles = extractRolesFromToken(token);
  return roles.includes(roleName);
};

// Check if token is expired
export const isTokenExpired = (token) => {
  try {
    const decoded = decodeJWT(token);
    if (!decoded || !decoded.exp) return true;
    
    // exp is in seconds, Date.now() is in milliseconds
    const currentTime = Date.now() / 1000;
    return decoded.exp < currentTime;
  } catch (error) {
    return true;
  }
};

// Get user info from token
export const getUserInfoFromToken = (token) => {
  try {
    const decoded = decodeJWT(token);
    if (!decoded) return null;
    
    return {
      userId: decoded.sub || decoded.userId,
      email: decoded.email,
      name: decoded.name || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      roles: extractRolesFromToken(token),
      exp: decoded.exp,
      iat: decoded.iat
    };
  } catch (error) {
    console.error('Error extracting user info from token:', error);
    return null;
  }
};

// Role constants
export const USER_ROLES = {
  ADMIN: 'Admin',
  TEACHER: 'Teacher', 
  STUDENT: 'Student',
  USER: 'User' // fallback for Student
};

// Get dashboard route based on role
export const getDashboardRoute = (role) => {
  switch (role) {
    case USER_ROLES.ADMIN:
      return '/admin/dashboard';
    case USER_ROLES.TEACHER:
      return '/teacher/dashboard';
    case USER_ROLES.STUDENT:
    case USER_ROLES.USER:
      return '/home';
    default:
      return '/home'; // default to user dashboard
  }
};

export default {
  decodeJWT,
  extractRolesFromToken,
  getPrimaryRole,
  hasRole,
  isTokenExpired,
  getUserInfoFromToken,
  USER_ROLES,
  getDashboardRoute
};