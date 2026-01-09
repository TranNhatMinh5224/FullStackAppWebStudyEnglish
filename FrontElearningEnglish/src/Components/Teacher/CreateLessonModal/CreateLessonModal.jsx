import React from "react";
import GenericCreateModal from "../../Common/GenericCreateModal";
import { ENTITY_CONFIGS } from "../../Common/entityConfigs";

export default function CreateLessonModal({ show, onClose, onSuccess, courseId, lessonData, isUpdateMode = false, isAdmin = false }) {
  return (
    <GenericCreateModal
      config={ENTITY_CONFIGS.lesson}
      show={show}
      onClose={onClose}
      onSuccess={onSuccess}
      parentId={courseId}
      entityData={lessonData}
      isUpdateMode={isUpdateMode}
      isAdmin={isAdmin}
    />
  );
}

