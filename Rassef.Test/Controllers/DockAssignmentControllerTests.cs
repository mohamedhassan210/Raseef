using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class DockAssignmentControllerTests
    {
        private readonly Mock<IRepository<DockAssignment>> _assignmentRepoMock;
        private readonly Mock<IRepository<Dock>> _dockRepoMock;
        private readonly Mock<IRepository<QueueTicket>> _ticketRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly DockAssignmentController _controller;

        public DockAssignmentControllerTests()
        {
            _assignmentRepoMock = new Mock<IRepository<DockAssignment>>();
            _dockRepoMock = new Mock<IRepository<Dock>>();
            _ticketRepoMock = new Mock<IRepository<QueueTicket>>();
            _userRepoMock = new Mock<IRepository<User>>();

            _controller = new DockAssignmentController(
                _assignmentRepoMock.Object,
                _dockRepoMock.Object,
                _ticketRepoMock.Object,
                _userRepoMock.Object
            );

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        // دالة مساعدة لتعيين الـ Id للـ Entities التي تحتوي على private set
        private static T SetEntityId<T>(T entity, int id) where T : class
        {
            var property = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(entity, id);
            return entity;
        }

        private void SetupUserClaims(string userId)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithMappedViewModels()
        {
            // Arrange
            var assignment = SetEntityId(new DockAssignment
            {
                Dock = new Dock { DockName = "Dock A" },
                QueueTicket = new QueueTicket { TicketNumber = "T-100" },
                AssignedAt = DateTimeOffset.Now,
                FinishedAt = DateTimeOffset.Now.AddHours(1)
            }, 1);

            _assignmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DockAssignment> { assignment });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<DockAssignmentListVM>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Dock A", model[0].DockName);
            Assert.Equal("T-100", model[0].TicketNumber);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_NullId_ReturnsViewWithModelStateError()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DockAssignmentDetailsVM>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_NotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DockAssignment)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DockAssignmentDetailsVM>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_Found_ReturnsViewResultWithDetailsVM()
        {
            // Arrange
            var assignment = SetEntityId(new DockAssignment
            {
                Dock = new Dock { DockName = "Dock B" },
                QueueTicket = new QueueTicket { TicketNumber = "T-200" },
                CreatedBy = new User { Name = "Admin User" },
                AssignedAt = DateTimeOffset.Now,
                FinishedAt = DateTimeOffset.Now.AddHours(2)
            }, 1);

            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DockAssignmentDetailsVM>(viewResult.Model);
            Assert.Equal("Dock B", model.DockName);
            Assert.Equal("T-200", model.TicketNumber);
            Assert.Equal("Admin User", model.CreatedByName);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_PopulatesDropdownsAndReturnsView()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock> { SetEntityId(new Dock { DockName = "D1" }, 1) });
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket> { SetEntityId(new QueueTicket { TicketNumber = "TK1" }, 1) });

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateDockAssignmentVM>(viewResult.Model);
            Assert.Single(model.Docks);
            Assert.Single(model.Tickets);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithDropdowns()
        {
            // Arrange
            _controller.ModelState.AddModelError("DockId", "Required");
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            var vm = new CreateDockAssignmentVM();

            // Act
            var result = await _controller.Create(vm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(((CreateDockAssignmentVM)viewResult.Model).Docks);
        }

        [Fact]
        public async Task Create_Post_NoUserIdClaim_ReturnsViewWithError()
        {
            // Arrange
            SetupUserClaims(null);
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Create(new CreateDockAssignmentVM());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_UserNotFoundInDb_ReturnsViewWithError()
        {
            // Arrange
            SetupUserClaims("5");
            _userRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((User)null);
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Create(new CreateDockAssignmentVM());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_ValidData_AddsAssignmentAndRedirectsToIndex()
        {
            // Arrange
            SetupUserClaims("10");
            var user = SetEntityId(new User { Name = "Test Admin" }, 10);
            _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

            var vm = new CreateDockAssignmentVM
            {
                DockId = 1,
                TicketId = 2,
                AssignedAt = DateTimeOffset.Now,
                FinishedAt = DateTimeOffset.Now.AddHours(1)
            };

            // Act
            var result = await _controller.Create(vm);

            // Assert
            _assignmentRepoMock.Verify(r => r.AddAsync(It.IsAny<DockAssignment>()), Times.Once);
            _assignmentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DockAssignmentController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Update GET Tests

        [Fact]
        public async Task Update_Get_NullId_ReturnsViewWithError()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Update(id: (int?)null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Get_NotFound_ReturnsViewWithError()
        {
            // Arrange
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DockAssignment)null);
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Update(id: (int?)1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Get_Found_ReturnsViewWithPopulatedModel()
        {
            // Arrange
            var assignment = SetEntityId(new DockAssignment { DockId = 3, TicketId = 4 }, 1);
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Update(id: (int?)1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateDockAssignmentVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(3, model.DockId);
            Assert.Equal(4, model.TicketId);
        }

        #endregion

        #region Update POST Tests

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithDropdowns()
        {
            // Arrange
            _controller.ModelState.AddModelError("DockId", "Required");
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Update(update: new UpdateDockAssignmentVM());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(((UpdateDockAssignmentVM)viewResult.Model).Docks);
        }

        [Fact]
        public async Task Update_Post_NotFound_ReturnsViewWithError()
        {
            // Arrange
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DockAssignment)null);
            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock>());
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());

            // Act
            var result = await _controller.Update(update: new UpdateDockAssignmentVM { Id = 1 });

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Post_ValidData_UpdatesAndRedirectsToIndex()
        {
            // Arrange
            var existingAssignment = SetEntityId(new DockAssignment { DockId = 1, TicketId = 1 }, 1);
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingAssignment);

            var updateVm = new UpdateDockAssignmentVM
            {
                Id = 1,
                DockId = 5,
                TicketId = 10,
                AssignedAt = DateTimeOffset.Now,
                FinishedAt = DateTimeOffset.Now.AddHours(2)
            };

            // Act
            var result = await _controller.Update(update: updateVm);

            // Assert
            Assert.Equal(5, existingAssignment.DockId);
            Assert.Equal(10, existingAssignment.TicketId);
            _assignmentRepoMock.Verify(r => r.Update(existingAssignment), Times.Once);
            _assignmentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DockAssignmentController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task Delete_Get_NullId_ReturnsViewWithError()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_Get_NotFound_ReturnsViewWithError()
        {
            // Arrange
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DockAssignment)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_Get_Found_ReturnsViewWithModel()
        {
            // Arrange
            var assignment = SetEntityId(new DockAssignment(), 1);
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DockAssignmentDetailsVM>(viewResult.Model);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteConfirmed_Post_NotFound_ReturnsViewWithError()
        {
            // Arrange
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DockAssignment)null);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _assignmentRepoMock.Verify(r => r.Remove(It.IsAny<DockAssignment>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_Found_RemovesAndRedirectsToIndex()
        {
            // Arrange
            var assignment = SetEntityId(new DockAssignment(), 1);
            _assignmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(assignment);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _assignmentRepoMock.Verify(r => r.Remove(assignment), Times.Once);
            _assignmentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DockAssignmentController.Index), redirectResult.ActionName);
        }

        #endregion
    }
}