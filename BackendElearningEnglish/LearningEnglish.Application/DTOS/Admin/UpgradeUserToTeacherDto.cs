namespace LearningEnglish.Application.DTOs.Admin
{
    
    public class UpgradeUserToTeacherDto
    {
     
        ///Email của user cần nâng cấp
      
        public string Email { get; set; } = string.Empty;

       
        // ID của TeacherPackage 
    
        public int TeacherPackageId { get; set; }
    }
}

