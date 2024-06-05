using System.Security.Claims;
using CoursesApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = CoursesApp.Data.Task;

namespace CoursesApp.Controllers
{
    public record CreateCourseModel(string Name, string Subject, string Passcode);
    public record EnrollCourseModel(int Id, string Passcode);
    public record DeleteCourseModel(int Id);

    public record GetCourseResponse(int Id, string Name, string Subject, string Teacher, IEnumerable<Task> Tasks);
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly UserManager<User> _userManager;

        public CoursesController(DBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllCourses()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == user.Id)
                .Include(e => e.Course)
                .ThenInclude(p => p.Teacher)
                .ToListAsync();

            var courses = enrollments
                .Select(e => e.Course)
                .Select(course => new { Id = course.Id, Name = course.Name, Subject = course.Subject, Teacher = course.Teacher.UserName }).ToList();
            
            return Ok(courses);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var course = await _context.Courses.Include(c => c.Teacher).Include(c => c.Tasks).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid course id." });
            }

            var tasks = course.Tasks.Select(task => new
            {
                Id = task.Id, Title = task.Title, Description = task.Description, MaxGrade = task.MaxGrade,
                DueDate = task.DueDate
            }).Reverse();

            var response = new
            {
                Id = course.Id, Name = course.Name, Subject = course.Subject, Teacher = course.Teacher.UserName,
                Tasks = tasks
            };
            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var course = new Course()
            {
                Name = model.Name,
                Subject = model.Subject,
                TeacherId = user.Id,
                Code = model.Passcode,
            };

            await _context.AddAsync(course);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCourse([FromBody] DeleteCourseModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid course id." });
            }

            if (course.TeacherId != user.Id)
            {
                return Unauthorized();
            }

            _context.Remove(course);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollCourse([FromBody] EnrollCourseModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null || course.Code != model.Passcode)
            {
                return StatusCode(500,
                    new { Status = "Error", Message = "Course is not exists or passcode is wrong." });
            }

            var enrollment = new Enrollment()
            {
                CourseId = model.Id,
                UserId = user.Id,
            };

            await _context.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            return Ok();
        }
        
        [NonAction]
        private async Task<User> GetCurrentUserAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var userClaims = identity.Claims;

            var user =  await _userManager.FindByNameAsync(
                userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value);
            return user;
        }
    }
}
