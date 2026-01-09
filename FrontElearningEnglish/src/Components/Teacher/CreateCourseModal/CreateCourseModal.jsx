import React, { useState, useEffect } from "react";
import GenericCreateModal from "../../Common/GenericCreateModal";
import { teacherPackageService } from "../../../Services/teacherPackageService";
import { useAuth } from "../../../Context/AuthContext";

const COURSE_IMAGE_BUCKET = "courses"; // Bucket name for course images

export default function CreateCourseModal({ show, onClose, onSuccess, courseData, isUpdateMode = false }) {
  const { user } = useAuth();
  const [maxStudent, setMaxStudent] = useState(0);
  const [loadingPackage, setLoadingPackage] = useState(false);

  // Load maxStudent from teacher package
  useEffect(() => {
    const loadMaxStudent = async () => {
      if (!show) return;

      if (!user) {
        setMaxStudent(0);
        return;
      }

      if (!user?.teacherSubscription?.packageLevel) {
        setMaxStudent(0);
        return;
      }

      try {
        setLoadingPackage(true);
        const packageResponse = await teacherPackageService.getAll();

        const userPackageLevel = user.teacherSubscription.packageLevel;
        const userPackage = packageResponse.data?.data?.find(pkg =>
          pkg.packageName === userPackageLevel ||
          pkg.PackageName === userPackageLevel
        );

        if (userPackage) {
          const maxStudents = userPackage.maxStudent || userPackage.MaxStudent || 0;
          setMaxStudent(maxStudents);
        } else {
          console.error(`⚠️ No package found matching: "${userPackageLevel}"`);
          setMaxStudent(0);
        }
      } catch (error) {
        console.error("❌ Error loading teacher package:", error);
        setMaxStudent(0);
      } finally {
        setLoadingPackage(false);
      }
    };

    if (show) {
      loadMaxStudent();
    }
  }, [show, user]);

  // Custom config for course
  const courseConfig = {
    entityName: 'course',
    bucketName: COURSE_IMAGE_BUCKET,
    fields: [
      {
        name: 'title',
        label: 'Tiêu đề',
        type: 'text',
        required: true,
        placeholder: 'Nhập tiêu đề khóa học',
        hint: '*Bắt buộc'
      },
      {
        name: 'description',
        label: 'Mô tả',
        type: 'textarea',
        required: true,
        placeholder: 'Nhập mô tả khóa học...',
        rows: 4,
        hint: '*Bắt buộc. Sử dụng Markdown để định dạng văn bản'
      },
      {
        name: 'maxStudent',
        label: 'Số học viên tối đa',
        type: 'number',
        required: false,
        defaultValue: maxStudent,
        hint: `Giá trị này được lấy từ gói giáo viên hiện tại của bạn (${maxStudent} học viên)`
      }
    ]
  };

  return (
    <GenericCreateModal
      config={courseConfig}
      show={show}
      onClose={onClose}
      onSuccess={onSuccess}
      entityData={courseData}
      isUpdateMode={isUpdateMode}
    />
  );
}

