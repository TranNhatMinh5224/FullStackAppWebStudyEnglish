import React from "react";
import GenericCreateModal from "../../Common/GenericCreateModal";
import { ENTITY_CONFIGS } from "../../Common/entityConfigs";

export default function CreateModuleModal({ show, onClose, onSuccess, lessonId, moduleData, isUpdateMode = false, isAdmin = false }) {
  return (
    <GenericCreateModal
      config={ENTITY_CONFIGS.module}
      show={show}
      onClose={onClose}
      onSuccess={onSuccess}
      parentId={lessonId}
      entityData={moduleData}
      isUpdateMode={isUpdateMode}
      isAdmin={isAdmin}
    />
  );
}

