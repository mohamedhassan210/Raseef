using Microsoft.AspNetCore.Mvc;
using Moq;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.ViewModels.Position; // اضبط النايم سبيس حسب مشروعك
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class PositionControllerTests
    {
        private readonly Mock<IRepository<Position>> _positionRepoMock;
        private readonly PositionController _controller;

        public PositionControllerTests()
        {
            _positionRepoMock = new Mock<IRepository<Position>>();

            _controller = new PositionController(_positionRepoMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private static T SetPropertyValue<T>(T entity, string propertyName, object value) where T : class
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(entity, value);
            return entity;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithListOfPositionViewModels()
        {
            // Arrange
            var pos1 = SetPropertyValue(new Position { PositionCode = 1, PositionName = "Manager", Users = new List<User> { new User() } }, "Id", 1);
            var pos2 = SetPropertyValue(new Position { PositionCode = 2, PositionName = "Developer", Users = new List<User>() }, "Id", 2);

            _positionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Position> { pos1, pos2 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<PositionViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_PositionNotFound_ReturnsIndexViewWithModelErrorAndPositions()
        {
            // Arrange
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Position)null);
            _positionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Position>());

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_PositionFound_ReturnsViewWithPositionDetailsViewModel()
        {
            // Arrange
            var user1 = new User { Name = "Ahmed" };
            var pos = SetPropertyValue(new Position
            {
                PositionCode = 1,
                PositionName = "Manager",
                Users = new List<User> { user1 }
            }, "Id", 1);

            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pos);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PositionDetailsViewModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(1, model.PositionCode);
            Assert.Single(model.Users);
            Assert.Contains("Ahmed", model.Users);
        }

        #endregion

        #region Create Tests

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("PositionCode", "Required");
            var inputModel = new CreatePositionViewModel { PositionCode = 0, PositionName = "Manager" };

            // Act
            var result = await _controller.Create(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inputModel, viewResult.Model);
            _positionRepoMock.Verify(r => r.AddAsync(It.IsAny<Position>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_PositionAlreadyExists_ReturnsViewWithModelError()
        {
            // Arrange
            var inputModel = new CreatePositionViewModel { PositionCode = 1, PositionName = "Manager" };
            _positionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Position, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _controller.Create(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inputModel, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _positionRepoMock.Verify(r => r.AddAsync(It.IsAny<Position>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_ValidData_AddsPositionAndRedirectsToIndex()
        {
            // Arrange
            var inputModel = new CreatePositionViewModel { PositionCode = 1, PositionName = "Manager" };
            _positionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Position, bool>>>())).ReturnsAsync(false);

            // Act
            var result = await _controller.Create(inputModel);

            // Assert
            _positionRepoMock.Verify(r => r.AddAsync(It.Is<Position>(p => p.PositionCode == 1 && p.PositionName == "Manager")), Times.Once);
            _positionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_PositionNotFound_ReturnsIndexViewWithModelError()
        {
            // Arrange
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Position)null);
            _positionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Position>());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_Get_PositionFound_ReturnsViewWithUpdatePositionViewModel()
        {
            // Arrange
            var pos = SetPropertyValue(new Position { PositionCode = 1, PositionName = "Manager" }, "Id", 1);
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pos);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdatePositionViewModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(1, model.PositionCode);
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("PositionCode", "Required");
            var inputModel = new UpdatePositionViewModel { Id = 1, PositionCode = 0, PositionName = "Manager" };

            // Act
            var result = await _controller.Edit(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inputModel, viewResult.Model);
            _positionRepoMock.Verify(r => r.Update(It.IsAny<Position>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_PositionNotFound_ReturnsViewWithModelError()
        {
            // Arrange
            var inputModel = new UpdatePositionViewModel { Id = 1, PositionCode = 1, PositionName = "Manager" };
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Position)null);

            // Act
            var result = await _controller.Edit(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inputModel, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _positionRepoMock.Verify(r => r.Update(It.IsAny<Position>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_DuplicatePosition_ReturnsViewWithModelError()
        {
            // Arrange
            var inputModel = new UpdatePositionViewModel { Id = 1, PositionCode = 1, PositionName = "Manager" };
            var existingPos = SetPropertyValue(new Position { PositionCode = 1, PositionName = "Old Manager" }, "Id", 1);

            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPos);
            _positionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Position, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _controller.Edit(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inputModel, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _positionRepoMock.Verify(r => r.Update(It.IsAny<Position>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ValidData_UpdatesPositionAndRedirectsToIndex()
        {
            // Arrange
            var inputModel = new UpdatePositionViewModel { Id = 1, PositionCode = 2, PositionName = "New Manager" };
            var existingPos = SetPropertyValue(new Position { PositionCode = 1, PositionName = "Old Manager" }, "Id", 1);

            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPos);
            _positionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Position, bool>>>())).ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(inputModel);

            // Assert
            _positionRepoMock.Verify(r => r.Update(It.Is<Position>(p => p.PositionCode == 2 && p.PositionName == "New Manager")), Times.Once);
            _positionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Get_PositionNotFound_ReturnsIndexViewWithModelError()
        {
            // Arrange
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Position)null);
            _positionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Position>());

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_Get_PositionFound_ReturnsViewWithPositionViewModel()
        {
            // Arrange
            var pos = SetPropertyValue(new Position { PositionCode = 1, PositionName = "Manager", Users = new List<User>() }, "Id", 1);
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pos);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PositionViewModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(1, model.PositionCode);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_PositionNotFound_ReturnsIndexViewWithModelError()
        {
            // Arrange
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Position)null);
            _positionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Position>());

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            _positionRepoMock.Verify(r => r.Remove(It.IsAny<Position>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_PositionFound_RemovesPositionAndRedirectsToIndex()
        {
            // Arrange
            var pos = SetPropertyValue(new Position { PositionCode = 1, PositionName = "Manager" }, "Id", 1);
            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pos);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _positionRepoMock.Verify(r => r.Remove(pos), Times.Once);
            _positionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }


        [Fact]
        public async Task Details_PositionFound_WithNullUsers_ThrowsOrHandlesNullReference()
        {
            // Arrange
            var pos = SetPropertyValue(new Position
            {
                PositionCode = 1,
                PositionName = "Manager",
                Users = null // محاكاة عدم تحميل الـ Related Data
            }, "Id", 1);

            _positionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pos);

            // Act & Assert
            // للتأكد من سلوك الكنترولر عند إرجاع Users كـ null
            await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.Details(1));
        }

        #endregion
    }
}