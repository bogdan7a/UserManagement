using AutoMapper;
using UserManagement.Dtos.Department;
using UserManagement.Models;

namespace UserManagement.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            // Source => Target
            CreateMap<DepartmentModel, DepartmentRead>();
            CreateMap<DepartmentCreate, DepartmentModel>();
            CreateMap<DepartmentUpdate, DepartmentModel>();
        }
    }
}
