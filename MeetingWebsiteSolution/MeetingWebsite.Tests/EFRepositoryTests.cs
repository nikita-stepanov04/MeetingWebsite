using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.EFRepository;
using MeetingWebsite.Infrastracture.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Tests
{
    public class EFRepositoryTests
    {
        private EFRepository<User, long> _repository;
        private User _user = new()
        {
            Firstname = "Test",
            Secondname = "Test",
        };

        private async Task SeedRepository()
        {
            _repository = new EFRepository<User, long>(
                new DataContext(
                    new DbContextOptionsBuilder<DataContext>()
                        .UseInMemoryDatabase("testdb").Options));
            await _repository.CreateAsync(_user);
            await _repository.SaveChangesAsync();
        }

        [Fact]
        public async Task RepositorySavesAndGetsUserById()
        {
            await SeedRepository();

            User? savedUser = await _repository.FindByIdAsync(_user.UserId);

            Assert.NotNull(savedUser);
            Assert.Equal(_user.Firstname, savedUser.Firstname);
        }

        [Fact]
        public async Task RepositoryUpdatesUser()
        {
            await SeedRepository();
            _user.Secondname = "Test1";
            await _repository.UpdateAsync(_user);
            await _repository.SaveChangesAsync();

            User? updatedUser = await _repository.FindByIdAsync(_user.UserId);

            Assert.NotNull(updatedUser);
            Assert.Equal("Test1", updatedUser.Secondname);
        }

        [Fact]
        public async Task RepositoryDeletesUser()
        {
            await SeedRepository();
            await _repository.DeleteAsync(_user);
            await _repository.SaveChangesAsync();
            User? deletedUser = await _repository.FindByIdAsync(_user.UserId);

            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task RepositoryGetsUsersEnumerable()
        {
            await SeedRepository();
            IEnumerable<User> users = _repository.GetQueryable();
            
            Assert.NotNull(users);
            Assert.Equal(_user.Firstname, (users.ToArray())[0].Firstname);
        }
    }
}
