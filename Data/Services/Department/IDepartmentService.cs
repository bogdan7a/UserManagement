using UserManagement.Data.Repository;
using UserManagement.Dtos.Department;
using UserManagement.Models;

namespace UserManagement.Data.Services.Department
{
    public interface IDepartmentService : IBaseRepository<DepartmentModel, DepartmentRead, DepartmentCreate, DepartmentUpdate>
    { }
}
