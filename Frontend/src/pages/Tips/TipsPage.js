import React from 'react';
import { useNavigate } from 'react-router-dom';
import './TipsPage.css';

const TipsPage = () => {
  const navigate = useNavigate();

  const tips = [
    {
      id: 1,
      title: "Phương pháp Spaced Repetition",
      description: "Lặp lại từ vựng theo khoảng thời gian ngày càng tăng",
      icon: "🔄",
      color: "#667eea",
      content: [
        "Học từ mới ngày đầu tiên",
        "Ôn lại sau 1 ngày",
        "Ôn lại sau 3 ngày", 
        "Ôn lại sau 1 tuần",
        "Ôn lại sau 1 tháng"
      ]
    },
    {
      id: 2,
      title: "Tạo câu chuyện kết nối",
      description: "Liên kết các từ vựng thành một câu chuyện có ý nghĩa",
      icon: "📚",
      color: "#48bb78",
      content: [
        "Chọn 5-10 từ vựng cần học",
        "Tạo một câu chuyện ngắn",
        "Kết nối các từ một cách logic",
        "Lặp lại câu chuyện nhiều lần",
        "Visualize hình ảnh trong đầu"
      ]
    },
    {
      id: 3,
      title: "Sử dụng flashcards thông minh",
      description: "Tạo thẻ từ vựng với hình ảnh và ví dụ cụ thể",
      icon: "🎴",
      color: "#ed8936",
      content: [
        "Viết từ vựng ở mặt trước",
        "Ghi nghĩa + ví dụ ở mặt sau",
        "Thêm hình ảnh minh họa",
        "Ghi âm thanh phát âm",
        "Ôn tập hàng ngày 15-20 phút"
      ]
    },
    {
      id: 4,
      title: "Học từ vựng qua ngữ cảnh",
      description: "Học từ trong câu và tình huống thực tế",
      icon: "💬",
      color: "#9f7aea",
      content: [
        "Đọc từ vựng trong bài báo/truyện",
        "Xem phim có phụ đề",
        "Nghe podcast tiếng Anh",
        "Thực hành đối thoại",
        "Viết nhật ký bằng tiếng Anh"
      ]
    },
    {
      id: 5,
      title: "Phương pháp Mind Map",
      description: "Tạo sơ đồ tư duy kết nối các từ liên quan",
      icon: "🧠",
      color: "#38b2ac",
      content: [
        "Chọn chủ đề trung tâm",
        "Tạo các nhánh chủ đề con", 
        "Thêm từ vựng vào từng nhánh",
        "Sử dụng màu sắc khác nhau",
        "Ôn tập bằng cách nhìn sơ đồ"
      ]
    },
    {
      id: 6,
      title: "Luyện tập với âm nhạc",
      description: "Học từ vựng thông qua bài hát và nhạc điệu",
      icon: "🎵",
      color: "#f56565",
      content: [
        "Chọn bài hát yêu thích",
        "Tra cứu từ vựng khó",
        "Hát theo với lời bài hát",
        "Tạo rap với từ vựng mới",
        "Ghi nhớ qua giai điệu"
      ]
    }
  ];

  return (
    <div className="tips-page">
      {/* Header */}
      <div className="tips-header">
        <button className="back-btn" onClick={() => navigate('/home')}>
          <span className="back-icon">←</span>
          <span>Quay lại trang chủ</span>
        </button>
        <div className="tips-header-content">
          <div className="header-icon">💡</div>
          <h1>Tips Ghi Nhớ Từ Vựng</h1>
          <p>Khám phá các phương pháp hiệu quả để ghi nhớ từ vựng tiếng Anh lâu dài</p>
        </div>
      </div>

      {/* Tips Grid */}
      <div className="tips-container">
        <div className="tips-grid">
          {tips.map((tip) => (
            <div key={tip.id} className="tip-card" style={{'--tip-color': tip.color}}>
              <div className="tip-header">
                <div className="tip-icon">{tip.icon}</div>
                <h3>{tip.title}</h3>
              </div>
              <p className="tip-description">{tip.description}</p>
              <div className="tip-content">
                <h4>Các bước thực hiện:</h4>
                <ul>
                  {tip.content.map((step, index) => (
                    <li key={index}>{step}</li>
                  ))}
                </ul>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default TipsPage;