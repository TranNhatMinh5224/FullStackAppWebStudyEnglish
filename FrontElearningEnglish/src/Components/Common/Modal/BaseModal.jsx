/**
 * BaseModal - Reusable Modal Component với SaaS Modern Design
 * 
 * CÁCH SỬ DỤNG:
 * <BaseModal
 *   show={showModal}
 *   onHide={handleClose}
 *   title="Modal Title"
 *   size="xl"                    // "sm" | "md" | "lg" | "xl" 
 *   footer={<button>Save</button>}
 *   onConfirmClose={handleConfirm} // Nếu cần confirm khi đóng
 * >
 *   <div>Modal content here</div>
 * </BaseModal>
 */

import React from 'react';
import { Modal } from 'react-bootstrap';
import './BaseModal.css';

const SIZE_MAP = {
  sm: 'modal-md-custom',  // 600px
  md: 'modal-md-custom',  // 600px
  lg: 'modal-lg-custom',  // 900px
  xl: 'modal-xl-custom',  // 1100px
};

const BaseModal = ({
  show,
  onHide,
  title,
  children,
  footer,
  size = 'lg',
  className = '',
  confirmOnClose = false,
  onConfirmClose,
  loading = false,
}) => {
  const handleHide = () => {
    if (confirmOnClose && onConfirmClose) {
      onConfirmClose();
    } else {
      onHide();
    }
  };

  return (
    <Modal
      show={show}
      onHide={handleHide}
      centered
      className={`modal-modern ${className}`}
      dialogClassName={SIZE_MAP[size] || SIZE_MAP.lg}
      backdrop={loading ? 'static' : true}
      keyboard={!loading}
    >
      <Modal.Header closeButton>
        <Modal.Title>{title}</Modal.Title>
      </Modal.Header>
      
      <Modal.Body>
        {children}
      </Modal.Body>
      
      {footer && (
        <Modal.Footer>
          {footer}
        </Modal.Footer>
      )}
    </Modal>
  );
};

export default BaseModal;
