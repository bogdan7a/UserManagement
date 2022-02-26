using UserManagement.Data.Repository;
using UserManagement.Dtos.User;
using UserManagement.Models;

namespace UserManagement.Data.Services.User
{
    public interface IUserService : IBaseRepository<UserModel, UserRead, UserRegister, UserUpdate>
    {
        Task<TokenModel> Login(UserLogin user);
        Task Register(UserRegister user);
        Task<TokenModel> RefreshToken(TokenModel tokenModel);
        Task Revoke(string email);
        Task ChangeUserRole(int id, Helpers.UserRole role);
    }
}
