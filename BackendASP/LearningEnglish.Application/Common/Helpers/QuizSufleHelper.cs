using LearningEnglish.Domain.Entities;
namespace LearningEnglish.Application.Common.Helpers
{
    public static class QuizShuffleHelper
    {
        private static readonly Random _random = new Random(); // khai báo  
        // Phương thức xáo trộn danh sách câu hỏi
        public static void ShuffleQuestion(List<Question> questions)

        {

            int n = questions.Count;
            if (questions == null || n <= 1)
            {
                return; // danh sách rỗng hoặc chỉ có một phần tử, không cần xáo trộn
            }
            for (int i = n - 1; i > 0; i--)

            {
                int j = _random.Next(i + 1);  // chọn một chỉ số ngẫu nhiên từ 0 đến i vì random sẽ random 1 số từ 0 đến n-1
                // hoán đổi questions[i] với questions[j]
                (questions[i], questions[j]) = (questions[j], questions[i]);
            }

        }
        // Phương thức xáo trộn danh sách câu trả lời
        public static void ShuffleAnswers(Question question)
        {
            if (question?.Options == null || question.Options.Count <= 1) return;

            for (int i = question.Options.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (question.Options[i], question.Options[j]) = (question.Options[j], question.Options[i]);
            }
        }
        public static void ShuffleQuizGroup(QuizGroup group, bool shuffleQuestions, bool shuffleAnswers)
        {
            if (shuffleQuestions)
            {
                ShuffleQuestion(group.Questions);
            }

            if (shuffleAnswers)
            {
                foreach (var question in group.Questions)
                {
                    ShuffleAnswers(question);
                }
            }
        }

    }

}