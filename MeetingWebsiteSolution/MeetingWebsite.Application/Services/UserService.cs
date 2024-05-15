using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Application.Services
{
    public class UserService : IUserService
    {
        private IRepository<User, long> _userRepository;
        public UserService(IRepository<User, long> rep)
        {
            _userRepository = rep;
        }
        
        public ValueTask<User?> FindByIdAsync(long id) =>
            _userRepository.FindByIdAsync(id);

        public Task<User> CreateAsync(User entity) =>
            _userRepository.CreateAsync(entity);

        public ValueTask<User> UpdateAsync(User entity) =>
            _userRepository.UpdateAsync(entity);

        public ValueTask<User> DeleteAsync(User entity) =>
            _userRepository.DeleteAsync(entity);        

        public IEnumerable<User> GetEnumerable() =>
            _userRepository.GetEnumerable();

        public Task<int> SaveChangesAsync() =>
            _userRepository.SaveChangesAsync();
    }
}
