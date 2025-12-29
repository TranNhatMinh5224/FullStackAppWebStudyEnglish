namespace LearningEnglish.Application.Common.Prompts;

/// <summary>
/// Standardized prompt builder for Gemini essay grading
/// Follows SRP (Single Responsibility Principle) - only responsible for building prompts
/// </summary>
public static class EssayGradingPrompt
{
    /// <summary>
    /// Build optimized prompt for Gemini to grade English essays
    /// </summary>
    /// <param name="essayTitle">Title of the essay assignment</param>
    /// <param name="essayDescription">Description/instructions for the essay</param>
    /// <param name="studentAnswer">Student's essay content</param>
    /// <param name="maxScore">Maximum score for the essay</param>
    /// <returns>Formatted prompt string for AI grading</returns>
    public static string BuildPrompt(
        string essayTitle,
        string essayDescription,
        string studentAnswer,
        decimal maxScore)
    {
        return $@"You are an expert English language teacher and essay grader. Your task is to evaluate a student's essay submission and provide comprehensive, constructive feedback.

## ESSAY ASSIGNMENT
**Title:** {essayTitle}
{(string.IsNullOrWhiteSpace(essayDescription) ? "" : $"**Instructions:** {essayDescription}\n")}
**Maximum Score:** {maxScore} points

## STUDENT'S ESSAY
{studentAnswer}

## GRADING RUBRIC
Evaluate the essay based on these four criteria:

1. **Content & Ideas (40% of total score)**
   - Thesis statement clarity and relevance (10%)
   - Quality and depth of supporting arguments (15%)
   - Use of examples and evidence (10%)
   - Conclusion effectiveness (5%)

2. **Language Use (30% of total score)**
   - Vocabulary range and accuracy (15%)
   - Grammar and sentence structure (15%)

3. **Organization (20% of total score)**
   - Essay structure and logical flow (10%)
   - Coherence and cohesion between paragraphs (10%)

4. **Mechanics (10% of total score)**
   - Spelling, punctuation, and capitalization

## INSTRUCTIONS
- Provide a total score between 0 and {maxScore} points
- Calculate breakdown scores proportionally (they should sum to approximately the total score)
- Give detailed, constructive feedback that helps the student improve
- List at least 2 specific strengths
- List at least 2 specific areas for improvement
- Be encouraging and professional in your tone
- Focus on actionable feedback

## OUTPUT FORMAT
You MUST respond with ONLY valid JSON, no additional text, no markdown code blocks, no explanations. The JSON structure must be exactly as follows:

{{
    ""score"": <decimal number between 0 and {maxScore}>,
    ""feedback"": ""<comprehensive overall feedback in 2-3 sentences>"",
    ""breakdown"": {{
        ""contentScore"": <decimal number>,
        ""languageScore"": <decimal number>,
        ""organizationScore"": <decimal number>,
        ""mechanicsScore"": <decimal number>
    }},
    ""strengths"": [
        ""<specific strength 1>"",
        ""<specific strength 2>"",
        ""<optional strength 3>""
    ],
    ""improvements"": [
        ""<specific improvement area 1>"",
        ""<specific improvement area 2>"",
        ""<optional improvement area 3>""
    ]
}}

IMPORTANT: Return ONLY the JSON object, nothing else. Do not include markdown code blocks, explanations, or any other text.";
    }
}

