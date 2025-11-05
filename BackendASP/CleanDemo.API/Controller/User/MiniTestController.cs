using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
namespace CleanDemo.API.Controller.User
{

    [ApiController]
    [Route("api/user/mini-test")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public class MiniTestController : ControllerBase
    {
        private readonly IMiniTestService _miniTestService;
        private readonly ILogger<MiniTestController> _logger;

        public MiniTestController(
            IMiniTestService miniTestService,
            ILogger<MiniTestController> logger)
        {
            _miniTestService = miniTestService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetListMiniTests([FromQuery] int lessonId)
        {
            try
            {
                var result = await _miniTestService.GetAllMiniTests(lessonId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListMiniTests endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}