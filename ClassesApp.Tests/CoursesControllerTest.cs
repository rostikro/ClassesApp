using System.Security.Claims;
using System.Security.Principal;
using CoursesApp.Controllers;
using CoursesApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClassesApp.Tests;

public class CoursesControllerTest
{
    private CoursesController _controller;

    private List<User> _users;

    private UserManager<User> _userManager;
    
    public CoursesControllerTest()
    {
        _users = new List<User>
        {
            new User { Id = "1", Email = "test@gmail.com", NormalizedEmail = "TEST@GMAIL.COM", UserName = "test", NormalizedUserName = "TEST" },
            new User { Id = "2", NormalizedEmail = "TEST1@GMAIL.COM", NormalizedUserName = "TEST1" }
        };
        _userManager = MockUserManager<User>(_users).Object;
        
        var _context = GetDbContext();
        
        var identity = new GenericIdentity("test@gmail.com", ClaimTypes.Name);
        var contextUser = new ClaimsPrincipal(identity); //add claims as needed

        //...then set user and other required properties on the httpContext as needed
        var httpContext = new DefaultHttpContext {
            User = contextUser
        };

        //Controller needs a controller context to access HttpContext
        var controllerContext = new ControllerContext() {
            HttpContext = httpContext,
        };
        
        _controller = new CoursesController(_context, _userManager){
            ControllerContext = controllerContext,
        };
    }
    
    public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

        return mgr;
    }

    private DBContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase(databaseName: "courses_app")
            .Options;

        var context = new DBContext(options);
        context.Database.EnsureCreated();

        context.Courses.Add(new Course { Id = 1, Name = "Test", Subject = "Math", TeacherId = "1", Code = "12345" });
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async void CoursesController_GetCourseById()
    {
        var validId = 1;
        var invalidId = 2;

        var okResult = await _controller.Get(validId);
        var errResult = await _controller.Get(invalidId);

        var item = okResult as OkObjectResult;

        var course = item.Value as GetCourseResponse;
        
        Assert.Equal("Test", course.Name);
        Assert.Equal("Math", course.Subject);
        Assert.Equal(1, course.Id);
        Assert.Equal("test", course.Teacher);
    }
}