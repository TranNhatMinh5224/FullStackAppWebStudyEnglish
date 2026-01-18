import React, { useState, useEffect } from "react";
import { adminService } from "../../../Services/adminService";
import UserStatsCards from "../../../Components/Admin/UserManagement/UserStatsCards/UserStatsCards";
import UserFilters from "../../../Components/Admin/UserManagement/UserFilters/UserFilters";
import UserTable from "../../../Components/Admin/UserManagement/UserTable/UserTable";
import UpgradeUserModal from "../../../Components/Admin/UserManagement/UpgradeUserModal/UpgradeUserModal";
import UserDetailModal from "../../../Components/Admin/UserManagement/UserDetailModal/UserDetailModal";
import "./AdminUserList.css";
import SuccessModal from "../../../Components/Common/SuccessModal/SuccessModal";
import ConfirmModal from "../../../Components/Common/ConfirmModal/ConfirmModal";
import NotificationModal from "../../../Components/Common/NotificationModal/NotificationModal";

export default function AdminUserList() {
  const [activeTab, setActiveTab] = useState("all"); 
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [stats, setStats] = useState({ totalUsers: 0, totalTeachers: 0, newUsersToday: 0 });
  const [pagination, setPagination] = useState({ pageNumber: 1, pageSize: 10, totalCount: 0 });

  // Modal State
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const [upgradePackageId, setUpgradePackageId] = useState(1); 
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  
  // Confirm Modal State
  const [showConfirmBlockModal, setShowConfirmBlockModal] = useState(false);
  const [userToBlock, setUserToBlock] = useState(null);
  const [isBlocking, setIsBlocking] = useState(false);
  
  // Notification Modal State
  const [notification, setNotification] = useState({
    isOpen: false,
    type: "success", // "success", "error", "info"
    message: ""
  });

  useEffect(() => {
    fetchUserStats();
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [activeTab, searchTerm, pagination.pageNumber]);

  const fetchUserStats = async () => {
    try {
      const res = await adminService.getUserStats();
      if (res.data && res.data.success) {
        setStats(res.data.data);
      }
    } catch (error) {
      console.error("Error fetching stats", error);
    }
  };

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const params = {
        searchTerm,
        pageNumber: pagination.pageNumber,
        pageSize: pagination.pageSize
      };

      let response;
      if (activeTab === 'teachers') {
        response = await adminService.getTeachers(params);
      } else if (activeTab === 'blocked') {
        response = await adminService.getBlockedUsers(params);
      } else {
        response = await adminService.getAllUsers(params);
      }

      if (response.data && response.data.success) {
        const usersData = response.data.data.items || [];
        setUsers(usersData);
        setPagination({
          ...pagination,
          totalCount: response.data.data.totalCount
        });
      }
    } catch (error) {
      console.error("Failed to fetch users:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetail = async (user) => {
    try {
      const userId = user.userId || user.id || user.UserId;
      if (!userId) {
        // Fallback: dùng data từ danh sách nếu không có userId
        setSelectedUser(user);
        setShowDetailModal(true);
        return;
      }
      
      // Gọi API để lấy chi tiết user với đầy đủ thông tin (bao gồm avatar)
      const response = await adminService.getUserById(userId);
      if (response.data && response.data.success) {
        const userData = response.data.data;
        // Debug: log để kiểm tra avatarUrl
        console.log("User data from API:", userData);
        console.log("AvatarUrl:", userData.avatarUrl || userData.AvatarUrl);
        setSelectedUser(userData);
        setShowDetailModal(true);
      } else {
        // Fallback: dùng data từ danh sách nếu API lỗi
        setSelectedUser(user);
        setShowDetailModal(true);
      }
    } catch (error) {
      console.error("Error fetching user detail:", error);
      // Fallback: dùng data từ danh sách nếu API lỗi
      setSelectedUser(user);
      setShowDetailModal(true);
    }
  };

  const handleToggleStatus = (user) => {
    setUserToBlock(user);
    setShowConfirmBlockModal(true);
  };

  const confirmToggleStatus = async () => {
    if (!userToBlock) return;
    
    const isBlocked = userToBlock.status === 'Inactive' || userToBlock.status === 0;
    const action = isBlocked ? 'Unblock' : 'Block';
    const actionText = isBlocked ? 'Mở khóa' : 'Khóa';
    
    setIsBlocking(true);
    try {
      if (isBlocked) {
        await adminService.unblockUser(userToBlock.userId || userToBlock.id);
      } else {
        await adminService.blockUser(userToBlock.userId || userToBlock.id);
      }
      
      setShowConfirmBlockModal(false);
      setNotification({
        isOpen: true,
        type: "success",
        message: `${actionText} tài khoản thành công!`
      });
      
      fetchUsers(); 
      fetchUserStats();
      setUserToBlock(null);
    } catch (error) {
      setNotification({
        isOpen: true,
        type: "error",
        message: `Không thể ${actionText.toLowerCase()} tài khoản. Vui lòng thử lại.`
      });
      console.error(`Failed to ${action} user:`, error);
    } finally {
      setIsBlocking(false);
    }
  };

  const openUpgradeModal = (user) => {
    setSelectedUser(user);
    setShowUpgradeModal(true);
  };

  const handleUpgradeUser = async () => {
    if (!selectedUser) return;
    
    try {
      await adminService.upgradeUserToTeacher({
        email: selectedUser.email,
        teacherPackageId: parseInt(upgradePackageId)
      });
      setSuccessMessage("Nâng cấp người dùng thành Teacher thành công!");
      setShowSuccessModal(true);
      setShowUpgradeModal(false);
      fetchUsers();
      fetchUserStats();
    } catch (error) {
      setNotification({
        isOpen: true,
        type: "error",
        message: "Không thể nâng cấp người dùng. Người dùng có thể đã là Teacher hoặc đã xảy ra lỗi."
      });
      console.error(error);
    }
  };

  return (
    <div className="user-management-container">
      {/* HEADER */}
      <div className="page-header">
        <h1 className="page-title">User Management</h1>
        <button className="btn-refresh" onClick={fetchUsers}>
          Refresh Data
        </button>
      </div>

      {/* STATS */}
      <UserStatsCards stats={stats} />

      {/* FILTERS */}
      <UserFilters 
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        searchTerm={searchTerm}
        setSearchTerm={setSearchTerm}
      />

      {/* USER TABLE */}
      <UserTable 
        users={users}
        loading={loading}
        onViewDetail={handleViewDetail}
        onUpgrade={openUpgradeModal}
        onToggleStatus={handleToggleStatus}
      />

      {/* MODALS */}
      <UpgradeUserModal 
        show={showUpgradeModal}
        onClose={() => setShowUpgradeModal(false)}
        user={selectedUser}
        packageId={upgradePackageId}
        setPackageId={setUpgradePackageId}
        onConfirm={handleUpgradeUser}
      />

      <UserDetailModal 
        show={showDetailModal} 
        onClose={() => setShowDetailModal(false)} 
        user={selectedUser} 
      />
      <SuccessModal
        isOpen={showSuccessModal}
        onClose={() => setShowSuccessModal(false)}
        title="Thành công"
        message={successMessage}
      />
      
      {/* Confirm Block/Unblock Modal */}
      <ConfirmModal
        isOpen={showConfirmBlockModal}
        onClose={() => {
          setShowConfirmBlockModal(false);
          setUserToBlock(null);
        }}
        onConfirm={confirmToggleStatus}
        title={userToBlock ? (userToBlock.status === 'Inactive' || userToBlock.status === 0 ? "Mở khóa tài khoản" : "Khóa tài khoản") : "Xác nhận"}
        message={userToBlock ? `Bạn có chắc chắn muốn ${userToBlock.status === 'Inactive' || userToBlock.status === 0 ? 'mở khóa' : 'khóa'} tài khoản ${userToBlock.email || userToBlock.Email || ''}?` : ""}
        confirmText={userToBlock ? (userToBlock.status === 'Inactive' || userToBlock.status === 0 ? "Mở khóa" : "Khóa") : "Xác nhận"}
        cancelText="Hủy"
        type={userToBlock && (userToBlock.status === 'Inactive' || userToBlock.status === 0) ? "warning" : "danger"}
        loading={isBlocking}
        disabled={isBlocking}
      />
      
      {/* Notification Modal */}
      <NotificationModal
        isOpen={notification.isOpen}
        onClose={() => setNotification({ ...notification, isOpen: false })}
        type={notification.type}
        message={notification.message}
        autoClose={true}
        autoCloseDelay={3000}
      />
    </div>
  );
}
