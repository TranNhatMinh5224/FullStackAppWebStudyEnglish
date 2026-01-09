// Configuration for different entities that can be created with GenericCreateModal

export const ENTITY_CONFIGS = {
  lesson: {
    entityName: 'lesson',
    bucketName: 'lessons',
    parentIdField: 'courseId',
    fields: [
      {
        name: 'title',
        label: 'Tiêu đề',
        type: 'text',
        required: true,
        placeholder: 'Nhập tiêu đề bài học',
        hint: '*Bắt buộc'
      },
      {
        name: 'description',
        label: 'Mô tả',
        type: 'textarea',
        required: false,
        placeholder: 'Mô tả ngắn gọn về bài học (tối đa 200 ký tự)...',
        maxLength: 200,
        rows: 2,
        hint: 'Mô tả ngắn gọn giúp học viên hiểu nhanh về bài học (tối đa 200 ký tự).',
        validate: (value) => {
          if (value && value.length > 200) {
            return 'Mô tả không được vượt quá 200 ký tự';
          }
          return null;
        }
      }
    ]
  },

  module: {
    entityName: 'module',
    bucketName: 'modules',
    parentIdField: 'lessonId',
    fields: [
      {
        name: 'name',
        label: 'Tên module',
        type: 'text',
        required: true,
        placeholder: 'Nhập tên module',
        hint: '*Bắt buộc'
      },
      {
        name: 'description',
        label: 'Mô tả',
        type: 'textarea',
        required: false,
        placeholder: 'Mô tả về module...',
        rows: 3,
        hint: 'Mô tả chi tiết về nội dung module'
      },
      {
        name: 'contentType',
        label: 'Loại nội dung',
        type: 'select',
        required: true,
        hint: '*Bắt buộc',
        options: [
          { value: 1, label: 'Lecture' },
          { value: 2, label: 'FlashCard' },
          { value: 3, label: 'Assessment' }
        ]
      }
    ]
  }
};