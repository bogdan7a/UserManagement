using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class DepartmentModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        // Relationship
        public virtual IEnumerable<UserModel>? Users { get; set; }
    }
}
