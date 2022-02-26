using System.ComponentModel.DataAnnotations;

namespace UserManagement.Dtos.Department
{
    public class DepartmentCreate
    {
        [Required(ErrorMessage = "Can't be empty")]
        public string Name { get; set; }
    }
}
