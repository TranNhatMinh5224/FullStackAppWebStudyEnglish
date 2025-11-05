using  namespace Domain.Entities;
using System.Collections.Generic; 
namespace CleanDemo.Application.DTOs{
    // class tao Question 
    public class CreateAnswerDto{
        public string Text {get ; set ; }
        public bool IsCorrect { get; set; }
    }
    public class CreteQuestionDto{
        public string Text { get; set; }
        pubic List<CreateAnswerDto>  Answers { get; set;}
        

         
    }
    public class QuestionDto{
         public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;

    public int MiniTestId { get; set; }
    }



    public class AnswerDto{
          public int AnswerOptionId { get; set; }
          public string Text { get; set; } = string.Empty;
    
          public bool IsCorrect { get; set; }

          public int QuestionId { get; set; }
    } 
    public class ResListQuestionAndAnswer{
        public List<QuestionDto> Items { get ; set ; } = new List<QuestionDto>

    }
}

   
   
     
   
