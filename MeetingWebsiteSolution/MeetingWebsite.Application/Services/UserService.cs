using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class UserService : IUserService
    {
        private IRepository<User, long> _userRepository;
        public UserService(IRepository<User, long> rep)
        {
            _userRepository = rep;
        }
        
        public Task<User?> FindByIdAsync(long id) =>
            _userRepository.GetQueryable()
                .Include(u => u.Interests)
                .Include(u => u.Friends)
                .FirstOrDefaultAsync(u => u.UserId == id);

        public Task<User> CreateAsync(User entity) =>
            _userRepository.CreateAsync(entity);

        public ValueTask<User> UpdateAsync(User entity) =>
            _userRepository.UpdateAsync(entity);

        public ValueTask<User> DeleteAsync(User entity) =>
            _userRepository.DeleteAsync(entity);

        public Task<int> SaveChangesAsync() =>
            _userRepository.SaveChangesAsync();
    }
}
