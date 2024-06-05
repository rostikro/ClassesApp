using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CoursesApp.Controllers;
using CoursesApp.Data;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ClassesApp.Tests;

public class UserControllerTest
{
    private UserController _controller;
    
    private List<User> _users = new List<User>
    {
        new User { Id = "1", Email = "test@gmail.com", UserName = "test" },
        new User { Id = "2", Email = "test1@gmail.com", UserName = "test1" }
    };

    private UserManager<User> _userManager;
    
    public UserControllerTest()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"JWT:ValidAudience", "http://localhost:4200"},
            {"JWT:ValidIssuer", "http://localhost:61955"},
            {"JWT:Secret", "Super Secret Secret Secret Secret Secret Secret Secret Secret Secret"},
            //...populate as needed for the test
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        _userManager = MockUserManager<User>(_users).Object;
        _controller = new UserController(_userManager, configuration);
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
    
    [Fact]
    public async void UserController_RegisterUser()
    {
        LoginRegisterModel model = new LoginRegisterModel("unit-test@gmail.com", "123");

        var response = await _controller.Register(model);

        var item = response as OkObjectResult;
        var jwt = item.Value as JwtToken;

        var token = jwt.token;
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token);
        var tokenS = jsonToken as JwtSecurityToken;
        
        var userName = tokenS.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
        
        // username must be the same with email and userName in jwt token
        Assert.Equal("unit-test@gmail.com", userName);
    }
}