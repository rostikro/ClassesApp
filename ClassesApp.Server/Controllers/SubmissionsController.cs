using System.Security.Claims;
using CoursesApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Controllers
{
    public record SubmitTaskModel(int TaskId, string Url);

    public record CheckSubmissionModel(int SubmissionId);

    public record GiveFeedbackModel(int SubmissionId, int Grade, string Feedback);
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly UserManager<User> _userManager;

        public SubmissionsController(DBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTask([FromBody] SubmitTaskModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var task = await _context.Tasks
                .Include(task => task.Course)
                .ThenInclude(course => course.Enrollments)
                .FirstOrDefaultAsync(task => task.Id == model.TaskId);
            
            if (task == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid task id." });
            }
            
            // Check if user has enrollment to this course
            if (!task.Course.Enrollments.Any(e => e.UserId == user.Id))
            {
                return Unauthorized();
            }

            var submission = new Submission()
            {
                TaskId = model.TaskId,
                UserId = user.Id,
                SubmissionDate = DateTime.UtcNow,
                Url = model.Url
            };

            await _context.AddAsync(submission);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckSubmission([FromBody] CheckSubmissionModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var submission = await _context.Submissions
                .Include(p => p.Task)
                .ThenInclude(p => p.Course)
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);
            
            if (submission == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid submission id." });
            }
            
            if (submission.Task.Course.TeacherId != user.Id)
            {
                return Unauthorized();
            }

            return Ok(new { Solution = submission.Url, SubmissionDate = submission.SubmissionDate });
        }

        [HttpPost("giveFeedback")]
        public async Task<IActionResult> GiveFeedback([FromBody] GiveFeedbackModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var submission = await _context.Submissions
                .Include(p => p.Task)
                .ThenInclude(p => p.Course)
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);
            
            if (submission == null)
            {
                return StatusCode(500, new { Status = "Error", Message = "Invalid submission id." });
            }
            
            if (submission.Task.Course.TeacherId != user.Id)
            {
                return Unauthorized();
            }

            submission.Grade = model.Grade;
            submission.Feedback = model.Feedback;

            _context.Update(submission);
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

            return await _userManager.FindByNameAsync(
                userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value);
        }
    }
}
