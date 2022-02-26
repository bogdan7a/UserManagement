using AutoMapper;
using UserManagement.Data.Repository;
using UserManagement.Dtos.Department;
using UserManagement.Models;

namespace UserManagement.Data.Services.Department
{
    public class DepartmentService : BaseRepository<DepartmentModel, DepartmentRead, DepartmentCreate, DepartmentUpdate>, IDepartmentService
    {
        public DepartmentService(AppDbContext context, IMapper mapper) : base(context, mapper)
        { }
    }
}
