using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS.Common;
using LearningEnglish.Application.Interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Common
{
    [ApiController]
    [Route("api/public/enums")]
    public class EnumController : ControllerBase
    {
        private readonly IEnumService _enumService;

        public EnumController(IEnumService enumService)
        {
            _enumService = enumService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(ServiceResponse<Dictionary<string, List<EnumMappingDto>>>), 200)]
        public IActionResult GetAllEnums()
        {
            var result = _enumService.GetAllEnums();
            return Ok(result);
        }

    }
}
