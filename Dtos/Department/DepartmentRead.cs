using UserManagement.Dtos.User;
using UserManagement.Models;

namespace UserManagement.Dtos.Department
{
    public class DepartmentRead
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IEnumerable<UserRead> Users { get; set; }
    }
}
