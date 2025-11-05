using System.Linq;
using System.Threading.Tasks;
using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class QuestionAnswerService : IQuestionAnswerService
    {
        private readonly IQuestionAnswerRepository _repo;
        private readonly IMiniTestRepository _miniTestRepo;
        private readonly ILogger<QuestionAnswerService> _logger;
        

        public QuestionAnswerService(IQuestionAnswerRepository repo, IMiniTestRepository miniTestRepo, ILogger<QuestionAnswerService> logger)
        {
            _repo = repo;
            _miniTestRepo = miniTestRepo;
            _logger = logger;
        }

        public async Task<ServiceResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto dto, int? actorUserId = null)
        {
            var res = new ServiceResponse<QuestionDto>();
            try
            {
                var miniTest = await _miniTestRepo.GetMiniTestByIdAsync(dto.MiniTestId);
                if (miniTest == null)
                {
                    res.Success = false;
                    res.StatusCode = 404;
                    res.Message = "Mini test không tồn tại.";
                    return res;
                }

                var question = new Question { MiniTestId = dto.MiniTestId, Text = dto.Text };
                var answers = dto.Answers.Select(a => new AnswerOption
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList();

                var saved = await _repo.AddQuestionWithAnswersAsync(question, answers);

                res.Data = new QuestionDto
                {
                    QuestionId = saved.QuestionId,
                    MiniTestId = saved.MiniTestId,
                    Text = saved.Text,
                    Answers = saved.Options.Select(o => new AnswerDto
                    {
                        AnswerOptionId = o.AnswerOptionId,
                        QuestionId = o.QuestionId,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                };
                res.StatusCode = 201;
                res.Message = "Tạo câu hỏi thành công.";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                res.Success = false;
                res.StatusCode = 500;
                res.Message = "Đã xảy ra lỗi hệ thống.";
            }
            return res;
        }
    }
}
