using AutoMapper;
using UserManagement.Dtos.User;
using UserManagement.Models;

namespace UserManagement.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            //Source => Target
            CreateMap<UserModel, UserRead>();
            CreateMap<UserRegister, UserModel>();
            CreateMap<UserUpdate, UserModel>();
        }
    }
}
