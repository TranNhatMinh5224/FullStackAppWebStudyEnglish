using System;
using System.Collections.Generic;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Common.Helpers
{
    public static class QuizShuffleHelper
    {
        // Shuffle chỉ questions không thuộc group (standalone)
        public static void ShuffleStandaloneQuestions(List<AttemptQuizSectionDto> sections, int seed)
        {
            var random = new Random(seed);
            foreach (var section in sections)
            {
                // Questions trong groups: Không shuffle (giữ nguyên)
                // Questions không thuộc group: Shuffle
                var standaloneQuestions = section.Questions ?? new List<QuestionDto>();
                FisherYatesShuffle(standaloneQuestions, random);
                section.Questions = standaloneQuestions;
            }
        }

        // Shuffle answers
        public static void ShuffleAnswers(List<AnswerOptionDto> options, int seed)
        {
            var random = new Random(seed);
            FisherYatesShuffle(options, random);
        }

        // Fisher-Yates Shuffle
        private static void FisherYatesShuffle<T>(List<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}