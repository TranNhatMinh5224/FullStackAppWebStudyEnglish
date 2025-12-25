using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public AdminAuthController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        // GET: api/admin/users/users - lấy tất cả người dùng với phân trang, search và sort
        // Query parameters:
        // - PageNumber, PageSize: Phân trang
        // - SearchTerm: Tìm kiếm theo Email (case-insensitive)
        // - SortBy: Sắp xếp theo field (email, firstname, lastname, createdat, status)
        // - SortOrder: 1 = A-Z, 2 = Z-A
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters request)
        {
            var pagedResult = await _userManagementService.GetAllUsersPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
        }

        // PUT: api/admin/users/block-account/{userId} - khoá tài khoản người dùng
        [HttpPut("block-account/{userId}")]
        public async Task<IActionResult> BlockAccount(int userId)
        {
            var result = await _userManagementService.BlockAccountAsync(userId);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // PUT: api/admin/users/unblock-account/{userId} - mở khoá tài khoản người dùng
        [HttpPut("unblock-account/{userId}")]
        public async Task<IActionResult> UnblockAccount(int userId)
        {
            var result = await _userManagementService.UnblockAccountAsync(userId);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // GET: api/admin/users/list-blocked-accounts - lấy tất cả tài khoản bị khoá với phân trang
        // Query parameters:
        // - PageNumber, PageSize: Phân trang
        // - SearchTerm: Tìm kiếm theo Email, FirstName, LastName (case-insensitive)
        // - SortBy: Sắp xếp theo field (email, firstname, lastname, createdat)
        // - SortOrder: 1 = A-Z, 2 = Z-A
        [HttpGet("list-blocked-accounts")]
        public async Task<IActionResult> GetListBlockedAccounts([FromQuery] UserQueryParameters request)
        {
            var pagedResult = await _userManagementService.GetListBlockedAccountsPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
        }

        // GET: api/admin/users/teachers - lấy tất cả giáo viên với phân trang
        // Query parameters:
        // - PageNumber, PageSize: Phân trang
        // - SearchTerm: Tìm kiếm theo Email, FirstName, LastName (case-insensitive)
        // - SortBy: Sắp xếp theo field (email, firstname, lastname, createdat)
        // - SortOrder: 1 = A-Z, 2 = Z-A
        [HttpGet("teachers")]
        public async Task<IActionResult> GetListTeachers([FromQuery] UserQueryParameters request)
        {
            var pagedResult = await _userManagementService.GetListTeachersPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
        }

    }
}
