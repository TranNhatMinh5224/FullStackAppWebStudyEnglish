using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service ;

/// <summary>
/// Builds structured prompts for AI essay grading (SRP - Single Responsibility Principle)
/// </summary>
public class EssayPromptBuilder : IEssayPromptBuilder
{
    public string BuildGradingPrompt(Essay essay, string studentEssayContent, decimal maxScore)
    {
        return $@"You are an expert English essay grader. Grade the following student essay according to the rubric below.

ESSAY QUESTION:
{essay.Title}

{(string.IsNullOrWhiteSpace(essay.Description) ? "" : $"DESCRIPTION:\n{essay.Description}\n")}
MAX SCORE: {maxScore} points

STUDENT ESSAY:
{studentEssayContent}

GRADING RUBRIC:
1. Content & Ideas (40%):
   - Thesis statement (10%)
   - Supporting arguments (15%)
   - Examples and evidence (10%)
   - Conclusion (5%)

2. Language Use (30%):
   - Vocabulary range and accuracy (15%)
   - Grammar and sentence structure (15%)

3. Organization (20%):
   - Essay structure and flow (10%)
   - Coherence and cohesion (10%)

4. Mechanics (10%):
   - Spelling, punctuation, capitalization

INSTRUCTIONS:
- Provide a score out of {maxScore} points
- Give detailed feedback on each rubric category
- List specific strengths (at least 2)
- List specific areas for improvement (at least 2)
- Be constructive and encouraging

OUTPUT FORMAT (JSON only, no other text):
{{
    ""score"": <numeric score>,
    ""feedback"": ""<overall feedback>"",
    ""breakdown"": {{
        ""contentScore"": <score for content>,
        ""languageScore"": <score for language>,
        ""organizationScore"": <score for organization>,
        ""mechanicsScore"": <score for mechanics>
    }},
    ""strengths"": [""<strength 1>"", ""<strength 2>""],
    ""improvements"": [""<improvement 1>"", ""<improvement 2>""]
}}";
    }
}
