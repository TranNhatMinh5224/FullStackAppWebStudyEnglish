import React, { useState } from "react";
import { MdBlock, MdCheckCircle, MdArrowUpward, MdVisibility } from "react-icons/md";
import "./UserTable.css";

// Component để hiển thị avatar - chỉ hiển thị nếu có avatarUrl
const Avatar = ({ avatarUrl, displayName }) => {
  const [imageError, setImageError] = useState(false);
  
  // Chỉ hiển thị avatar nếu có avatarUrl và chưa lỗi
  if (avatarUrl && avatarUrl.trim() && !imageError) {
    return (
      <img 
        src={avatarUrl} 
        className="rounded-circle me-2" 
        width="40" 
        height="40" 
        alt={displayName}
        style={{ objectFit: 'cover', flexShrink: 0 }}
        onError={() => setImageError(true)}
      />
    );
  }
  
  // Không hiển thị gì nếu không có avatar
  return null;
};

export default function UserTable({ 
  users, 
  loading, 
  onViewDetail, 
  onUpgrade, 
  onToggleStatus 
}) {
  const getStatusBadge = (status) => {
    if (status === 'Active' || status === 1) {
      return <span className="status-badge status-active">Active</span>;
    }
    return <span className="status-badge status-blocked">Blocked</span>;
  };

  return (
    <div className="admin-card">
      <div className="table-responsive">
        <table className="admin-table">
          <thead>
            <tr>
              <th>User Info</th>
              <th>Role</th>
              <th>Phone</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan="5" className="text-center py-4">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </td>
              </tr>
            ) : users.length === 0 ? (
              <tr>
                <td colSpan="5" className="text-center py-4 text-muted">
                  No users found
                </td>
              </tr>
            ) : (
              users.map((user) => {
                // Lấy avatarUrl từ API (check cả camelCase và PascalCase)
                const avatarUrl = (user.avatarUrl || user.AvatarUrl || '').trim();
                
                const firstName = user.firstName || user.FirstName || '';
                const lastName = user.lastName || user.LastName || '';
                const email = user.email || user.Email || '';
                const displayName = user.displayName || user.DisplayName || `${firstName} ${lastName}`.trim();
                const userId = user.userId || user.id || user.UserId || 0;
                
                return (
                <tr key={userId}>
                  <td>
                    <div className="d-flex align-items-center">
                      <Avatar 
                        avatarUrl={avatarUrl}
                        displayName={displayName}
                      />
                      <div>
                        <div className="fw-bold">{displayName || 'N/A'}</div>
                        <small className="text-muted" style={{fontSize: '0.75rem'}}>{email}</small>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className={`badge ${(user.roles || user.Roles || [])?.includes('Teacher') ? 'bg-primary' : 'bg-secondary'}`}>
                      {(user.roles || user.Roles || [])?.[0] || 'Student'}
                    </span>
                  </td>
                  <td className="text-muted">{user.phoneNumber || user.PhoneNumber || 'N/A'}</td>
                  <td>{getStatusBadge(user.status || user.Status)}</td>
                  <td>
                    <div className="d-flex gap-2">
                      <button 
                        className="btn btn-sm btn-light text-info" 
                        title="View Detail"
                        onClick={() => onViewDetail(user)}
                      >
                        <MdVisibility />
                      </button>

                      {!(user.roles || user.Roles || [])?.includes('Teacher') && (
                        <button 
                          className="btn btn-sm btn-light text-primary" 
                          title="Upgrade to Teacher"
                          onClick={() => onUpgrade(user)}
                        >
                          <MdArrowUpward />
                        </button>
                      )}
                      
                      <button 
                        className="btn btn-sm btn-light" 
                        title={(user.status || user.Status) === 'Active' || (user.status || user.Status) === 1 ? "Block User" : "Unblock User"}
                        onClick={() => onToggleStatus(user)}
                      >
                        {(user.status || user.Status) === 'Active' || (user.status || user.Status) === 1 ? 
                          <MdBlock className="text-danger" /> : 
                          <MdCheckCircle className="text-success" />
                        }
                      </button>
                    </div>
                  </td>
                </tr>
              );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
