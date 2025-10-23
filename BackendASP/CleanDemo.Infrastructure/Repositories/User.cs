using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanDemo.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == id);

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == email);

        public async Task<List<User>> GetAllUsersAsync() =>
            await _context.Users.Include(u => u.Roles).ToListAsync();

        public async Task AddUserAsync(User user) => await _context.Users.AddAsync(user);

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null) _context.Users.Remove(user);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<Role?> GetRoleByNameAsync(string roleName) =>
            await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        public async Task<bool> UpdateRoleTeacher(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return false;
            }

            // Check if user already has Teacher role (RoleId=3)
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == 3);
            if (existingUserRole != null)
            {
                // Already has the role, no need to add
                return true;
            }

            UserRole userRole = new UserRole
            {
                UserId = userId,
                RoleId = 3
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();  // Save changes
            return true;
        }
    }
}
