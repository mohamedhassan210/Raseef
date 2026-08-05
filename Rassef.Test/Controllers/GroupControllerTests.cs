using Microsoft.AspNetCore.Mvc;
using Moq;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Identity;
using Rassef.ViewModels.Authentication.Identity;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class GroupControllerTests
    {
        private readonly Mock<IRepository<UserGroup>> _groupRepoMock;
        private readonly Mock<IRepository<Permission>> _permissionRepoMock;
        private readonly Mock<IRepository<GroupPermission>> _groupPermissionRepoMock;
        private readonly GroupController _controller;

        public GroupControllerTests()
        {
            _groupRepoMock = new Mock<IRepository<UserGroup>>();
            _permissionRepoMock = new Mock<IRepository<Permission>>();
            _groupPermissionRepoMock = new Mock<IRepository<GroupPermission>>();

            _controller = new GroupController(
                _groupRepoMock.Object,
                _permissionRepoMock.Object,
                _groupPermissionRepoMock.Object
            );

            // إعداد الـ HttpContext
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

        #region Constructor Tests

        [Fact]
        public void Constructor_NullGroupRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupController(
                null,
                _permissionRepoMock.Object,
                _groupPermissionRepoMock.Object
            ));
        }

        [Fact]
        public void Constructor_NullPermissionRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupController(
                _groupRepoMock.Object,
                null,
                _groupPermissionRepoMock.Object
            ));
        }

        [Fact]
        public void Constructor_NullGroupPermissionRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupController(
                _groupRepoMock.Object,
                _permissionRepoMock.Object,
                null
            ));
        }

        #endregion

        #region ManagePermissions GET Tests

        [Fact]
        public async Task ManagePermissions_Get_GroupNotFound_ReturnsViewWithModelError()
        {
            // Arrange
            _groupRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserGroup)null);

            // Act
            var result = await _controller.ManagePermissions(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task ManagePermissions_Get_GroupFound_ReturnsViewWithGroupPermissionsViewModel()
        {
            // Arrange
            var group = SetPropertyValue(new UserGroup { Name = "Admins" }, "Id", 1);

            var perm1 = SetPropertyValue(new Permission { ControllerName = "Driver", ActionName = "Index", Description = "View Drivers" }, "Id", 10);
            var perm2 = SetPropertyValue(new Permission { ControllerName = "Driver", ActionName = "Create", Description = "Add Driver" }, "Id", 20);
            var perm3 = SetPropertyValue(new Permission { ControllerName = "Truck", ActionName = "Index", Description = "View Trucks" }, "Id", 30);

            var permissions = new List<Permission> { perm1, perm2, perm3 };

            var groupPermissions = new List<GroupPermission>
            {
                new GroupPermission { GroupId = 1, PermissionId = 10 }
            };

            _groupRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(group);
            _permissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(permissions);
            _groupPermissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(groupPermissions);

            // Act
            var result = await _controller.ManagePermissions(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupPermissionsViewModel>(viewResult.Model);

            Assert.Equal(1, model.GroupId);
            Assert.Equal("Admins", model.GroupName);
            Assert.Equal(2, model.Controllers.Count); // Driver & Truck

            var driverController = model.Controllers.FirstOrDefault(c => c.ControllerName == "Driver");
            Assert.NotNull(driverController);
            Assert.Equal(2, driverController.Actions.Count);

            var indexAction = driverController.Actions.FirstOrDefault(a => a.PermissionId == 10);
            Assert.NotNull(indexAction);
            Assert.True(indexAction.IsSelected);

            var createAction = driverController.Actions.FirstOrDefault(a => a.PermissionId == 20);
            Assert.NotNull(createAction);
            Assert.False(createAction.IsSelected);
        }

        #endregion

        #region ManagePermissions POST Tests

        [Fact]
        public async Task ManagePermissions_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("GroupName", "Required");
            var inputModel = new GroupPermissionsViewModel { GroupId = 1, GroupName = "" };

            // Act
            var result = await _controller.ManagePermissions(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupPermissionsViewModel>(viewResult.Model);
            Assert.Equal(inputModel, model);
            _groupPermissionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ManagePermissions_Post_EmptyControllers_ClearsOldPermissionsWithoutAddingNew()
        {
            // Arrange
            var oldGp1 = SetPropertyValue(new GroupPermission { GroupId = 1, PermissionId = 5 }, "Id", 100);
            _groupPermissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<GroupPermission> { oldGp1 });

            var inputModel = new GroupPermissionsViewModel
            {
                GroupId = 1,
                GroupName = "Admins",
                Controllers = new List<ControllerPermissionsViewModel>() // قائمة فارغة من التحكمات
            };

            // Act
            var result = await _controller.ManagePermissions(inputModel);

            // Assert
            _groupPermissionRepoMock.Verify(r => r.Remove(It.Is<GroupPermission>(gp => gp.Id == 100)), Times.Once);
            _groupPermissionRepoMock.Verify(r => r.AddAsync(It.IsAny<GroupPermission>()), Times.Never);
            _groupPermissionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task ManagePermissions_Post_ValidData_RemovesOldPermissionsAndAddsSelectedOnes()
        {
            // Arrange
            var oldGp1 = SetPropertyValue(new GroupPermission { GroupId = 1, PermissionId = 5 }, "Id", 100);
            var oldGp2 = SetPropertyValue(new GroupPermission { GroupId = 2, PermissionId = 6 }, "Id", 101);

            var oldGroupPermissions = new List<GroupPermission> { oldGp1, oldGp2 };

            _groupPermissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(oldGroupPermissions);

            var inputModel = new GroupPermissionsViewModel
            {
                GroupId = 1,
                GroupName = "Admins",
                Controllers = new List<ControllerPermissionsViewModel>
                {
                    new ControllerPermissionsViewModel
                    {
                        ControllerName = "Driver",
                        Actions = new List<PermissionCheckBoxViewModel>
                        {
                            new PermissionCheckBoxViewModel { PermissionId = 10, IsSelected = true },
                            new PermissionCheckBoxViewModel { PermissionId = 20, IsSelected = false }
                        }
                    },
                    new ControllerPermissionsViewModel
                    {
                        ControllerName = "Truck",
                        Actions = new List<PermissionCheckBoxViewModel>
                        {
                            new PermissionCheckBoxViewModel { PermissionId = 30, IsSelected = true }
                        }
                    }
                }
            };

            // Act
            var result = await _controller.ManagePermissions(inputModel);

            // Assert
            // 1. التأكد من حذف الصلاحيات القديمة للمجموعة رقم 1 فقط
            _groupPermissionRepoMock.Verify(r => r.Remove(It.Is<GroupPermission>(gp => gp.Id == 100)), Times.Once);
            _groupPermissionRepoMock.Verify(r => r.Remove(It.Is<GroupPermission>(gp => gp.Id == 101)), Times.Never);

            // 2. التأكد من إضافة الصلاحيات المحددة فقط (10 و 30)
            _groupPermissionRepoMock.Verify(r => r.AddAsync(It.Is<GroupPermission>(gp => gp.GroupId == 1 && gp.PermissionId == 10)), Times.Once);
            _groupPermissionRepoMock.Verify(r => r.AddAsync(It.Is<GroupPermission>(gp => gp.GroupId == 1 && gp.PermissionId == 30)), Times.Once);
            _groupPermissionRepoMock.Verify(r => r.AddAsync(It.Is<GroupPermission>(gp => gp.PermissionId == 20)), Times.Never);

            // 3. التأكد من حفظ التغييرات والـ Redirect
            _groupPermissionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion
    }
}