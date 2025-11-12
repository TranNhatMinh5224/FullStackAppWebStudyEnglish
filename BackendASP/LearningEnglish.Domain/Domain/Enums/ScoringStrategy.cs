namespace LearningEnglish.Domain.Enums
{
    public enum ScoringStrategy
    {
        AllOrNothing = 0, // đúng hết thì được điểm, sai 1 câu là không được điểm
        PartialProportional = 1, // đúng từng phần thì được điểm tương ứng
        PartialWithPenalty = 2 // đúng từng phần nhưng có hình phạt cho câu sai
    }
}