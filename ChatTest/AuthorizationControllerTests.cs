using ChatGPT.Controllers;
using ChatGPT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace ChatTest
{
    public class AuthorizationControllerTests
    {

        [Fact]
        public async Task Registration_ValidUser_RedirectsToChatIndex()
        {
            // Arrange
            var user = new User
            {
                Login = "testuser9",
                Password = "password9"
            };

            using (var context = new ChatAppContext())
            {

                var httpContext = new Mock<HttpContext>();
                var authService = new Mock<IAuthenticationService>();
                httpContext.Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService))).Returns(authService.Object);
                var controller = new AuthorizationController(new ChatAppContext());

                // Act
                var result = await controller.Registration(user);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

                Assert.Equal("Index", redirectToActionResult.ActionName);
                Assert.Equal("Chat", redirectToActionResult.ControllerName);
                var createdUser = context.Users.FirstOrDefault(u => u.Login == user.Login);

                Assert.Equal(user.Login, createdUser.Login);
                Assert.Equal(user.Password, createdUser.Password);
                
                
                
                context.Users.Remove(user);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task Authorization_ValidCredentials_RedirectsToChatIndex()
        {
            var user = new User
            {
                Login = "testuser8",
                Password = "password8"
            };
            // Arrange
            using (var context = new ChatAppContext())
            {
                var httpContext = new Mock<HttpContext>();
                var authService = new Mock<IAuthenticationService>();
                httpContext.Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService))).Returns(authService.Object);
                var controller = new AuthorizationController(new ChatAppContext());
                // Act
                var result = await controller.Authorization(user.Login, user.Password);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);


                Assert.Equal("Index", redirectToActionResult.ActionName);

                context.Users.Remove(user);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task Authorization_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            using (var context = new ChatAppContext())
            {
                var controller = new AuthorizationController(context);

                // Act
                var result = await controller.Authorization("invaliduser", "invalidpassword");

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }
    }
}
