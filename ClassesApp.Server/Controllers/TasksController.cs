using System.Security.Claims;
using CoursesApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = CoursesApp.Data.Task;

namespace CoursesApp.Controllers
{

    public record CreateTaskModel(int CourseId, string Title, string Description, int MaxGrade, DateTime DueDate);
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly UserManager<User> _userManager;

        public TasksController(DBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var course = await _context.Courses.FindAsync(model.CourseId);
            if (course == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid course id." });
            }

            if (course.TeacherId != user.Id)
            {
                return Unauthorized();
            }

            var task = new Task()
            {
                Title = model.Title,
                Description = model.Description,
                CourseId = model.CourseId,
                MaxGrade = model.MaxGrade,
                DueDate = model.DueDate
            };

            await _context.AddAsync(task);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("getTask/{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var task = await _context.Tasks
                .Include(task => task.Course)
                .ThenInclude(course => course.Enrollments)
                .Include(task => task.Course)
                .ThenInclude(course => course.Teacher)
                .Include(task => task.Submissions)
                .FirstOrDefaultAsync(task => task.Id == id);
            
            if (task == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid task id." });
            }
            
            // Check if user has enrollment to this course
            if (!task.Course.Enrollments.Any(e => e.UserId == user.Id))
            {
                return Unauthorized();
            }

            var submission = task.Submissions.FirstOrDefault(s => s.UserId == user.Id);
            var submissionUrl = submission?.Url;
            var taskGrade = submission?.Grade;

            var taskDetails = new
            {
                Title = task.Title, Description = task.Description, Teacher = task.Course.Teacher.UserName, MaxGrade = task.MaxGrade, DueDate = task.DueDate.ToString("MMM dd HH:mm"), SubmissionUrl = submissionUrl, Grade = taskGrade
            };

            return Ok(taskDetails);
        }
        
        [NonAction]
        private async Task<User> GetCurrentUserAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var userClaims = identity.Claims;

            return await _userManager.FindByNameAsync(
                userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value);
        }
    }
}
