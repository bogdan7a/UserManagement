using UserManagement.Models;

namespace UserManagement.Dtos.User
{
    public class UserRead
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int DepartmentId{ get; set; }
        public string Role { get; set; }
    }
}
