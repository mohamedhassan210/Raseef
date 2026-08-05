using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Rassef.Controllers;
using Rassef.Models.Identity;
using Rassef.Models.ValueObjects; // لاستخدام كلاس Email الـ Value Object
using Rassef.Common.Interfaces.Services.AuthenticationServices;
using Rassef.ViewModels.Authentication;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock; // استخدام IUserRepository المخصص
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<AuthenticationController>> _loggerMock;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _loggerMock = new Mock<ILogger<AuthenticationController>>();

            _controller = new AuthenticationController(
                _userRepositoryMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object
            );

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
        }

        #region Register Tests

        [Fact]
        public async Task RegisterPostValidDataCreatesUserAndRedirects()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "Ahmed Mohamed",
                UserName = "ahmed_new",
                Email = "newuser@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // مطابقة Email Value Object باستخدام It.IsAny<Email>()
            _jwtServiceMock
                .Setup(jwt => jwt.GenerateToken(It.IsAny<int>(), It.IsAny<Email>()))
                .Returns("mocked_jwt_token");

            // Act
            var result = await _controller.Register(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task RegisterPost_DuplicateUser_ReturnsViewWithModelError()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "Ahmed Mohamed",
                UserName = "existing_user",
                Email = "existing@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task RegisterPost_InvalidModelState_ReturnsViewWithoutAddingUser()
        {
            // Arrange
            var model = new RegisterViewModel();
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _controller.Register(model);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        #endregion

        #region Login Tests



        [Fact]
        public async Task Login_Post_InvalidUserOrPassword_ReturnsViewWithModelStateError()
        {
            // Arrange
            var loginVm = new LoginViewModel
            {
                Password = "wrong_password"
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(loginVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion
    }
}