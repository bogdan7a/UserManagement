using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Services.Department;
using UserManagement.Dtos.Department;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ILogger<DepartmentsController> _logger;
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(ILogger<DepartmentsController> logger, IDepartmentService departmentService)
        {
            _logger = logger;
            _departmentService = departmentService;
        }

        [HttpGet("GetAllDepartments")]
        [Authorize(Roles = "Employee, Manager")]
        public async Task<IActionResult> GetAllDepartments()
        {
            _logger.LogInformation("--- Getting all departments ---");
            var departments = await _departmentService.GetAll();
            return Ok(departments);
        }

        [HttpGet("GetDepartmentById")]
        [Authorize(Roles = "Employee, Manager")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            _logger.LogInformation($"--- Getting department with id: {id} ---");
            var department = await _departmentService.GetById(id);
            return Ok(department);
        }

        [HttpPost("CreateDepartment")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateDepartment([FromForm] DepartmentCreate department)
        {
            _logger.LogInformation("--- Trying to create a new department ---");

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _departmentService.Create(department);
            return RedirectToAction(nameof(GetAllDepartments));
        }

        [HttpPut("UpdateDepartment")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentUpdate department)
        {
            _logger.LogInformation($"--- Trying to update department with id: {id} ---");

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _departmentService.Update(id, department);
            return Ok(department);
        }

        [HttpDelete("DeleteDepartment")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            _logger.LogInformation($"--- Trying to delete department with id: {id} ---");
            await _departmentService.Delete(id);
            return Ok(new { message = $"Department with id:{id} has been deleted" });
        }
    }
}
