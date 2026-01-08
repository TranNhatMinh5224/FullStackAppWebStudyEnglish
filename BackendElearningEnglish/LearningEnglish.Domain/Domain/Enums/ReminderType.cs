namespace LearningEnglish.Domain.Enums;

public enum ReminderType
{
    DailyStudy = 1,
    FlashcardReview = 2,
    AssignmentDue = 3, 
    QuizDeadline = 4,
    LessonReminder = 5
}

[Flags]
public enum DaysOfWeek
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,

    // Common combinations
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
    Weekend = Saturday | Sunday,
    All = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday
}
