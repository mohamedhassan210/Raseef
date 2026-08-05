using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.QueueAction;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class QueueActionControllerTests
    {
        private readonly Mock<IRepository<QueueAction>> _queueActionRepoMock;
        private readonly Mock<IRepository<QueueTicket>> _ticketRepoMock;
        private readonly Mock<IRepository<ActionTypes>> _actionTypeRepoMock;
        private readonly QueueActionController _controller;

        public QueueActionControllerTests()
        {
            _queueActionRepoMock = new Mock<IRepository<QueueAction>>();
            _ticketRepoMock = new Mock<IRepository<QueueTicket>>();
            _actionTypeRepoMock = new Mock<IRepository<ActionTypes>>();

            _controller = new QueueActionController(
                _queueActionRepoMock.Object,
                _ticketRepoMock.Object,
                _actionTypeRepoMock.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuth"));

            var httpContext = new DefaultHttpContext { User = user };
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;
        }

        private static T SetPropertyValue<T>(T entity, string propertyName, object value) where T : class
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(entity, value);
            return entity;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithListOfQueueActionListVM()
        {
            // Arrange
            var action1 = SetPropertyValue(new QueueAction { TicketId = 101, ActionTypeId = 1, ActionTime = DateTime.Now }, "Id", 1);
            var action2 = SetPropertyValue(new QueueAction { TicketId = 102, ActionTypeId = 2, ActionTime = DateTime.Now }, "Id", 2);

            var type1 = SetPropertyValue(SetPropertyValue(new ActionTypes(), "Name", "Call"), "Id", 1);

            _queueActionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueAction> { action1, action2 });
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes> { type1 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<QueueActionListVM>>(viewResult.Model);
            var list = model.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("Call", list[0].ActionTypeName);
            Assert.Equal("غير محدد", list[1].ActionTypeName);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_ActionNotFound_ReturnsViewWithEmptyModelAndError()
        {
            // Arrange
            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueAction)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueueActionDetailsVM>(viewResult.Model);
            Assert.Equal(0, model.Id);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_ActionFound_ReturnsViewWithDetailsVM()
        {
            // Arrange
            var action = SetPropertyValue(new QueueAction
            {
                TicketId = 101,
                ActionTypeId = 1,
                ActionTime = DateTime.Now
            }, "Id", 1);

            SetPropertyValue(action, "CreatedAT", DateTimeOffset.Now);

            var type = SetPropertyValue(SetPropertyValue(new ActionTypes(), "Name", "Serve"), "Id", 1);

            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(action);
            _actionTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(type);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueueActionDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(101, model.TicketId);
            Assert.Equal("Serve", model.ActionTypeName);
        }

        [Fact]
        public async Task Details_ActionFound_TypeNotFound_ReturnsDefaultTypeName()
        {
            // Arrange
            var action = SetPropertyValue(new QueueAction { TicketId = 101, ActionTypeId = 99 }, "Id", 1);

            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(action);
            _actionTypeRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ActionTypes)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueueActionDetailsVM>(viewResult.Model);
            Assert.Equal("غير محدد", model.ActionTypeName);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_Get_ReturnsViewWithPopulatedDropdowns()
        {
            // Arrange
            var ticket = SetPropertyValue(new QueueTicket { TicketNumber = "A-100" }, "Id", 1);
            var type = SetPropertyValue(SetPropertyValue(new ActionTypes(), "Name", "Transfer"), "Id", 1);

            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket> { ticket });
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes> { type });

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateQueueActionVM>(viewResult.Model);
            Assert.Single(model.Tickets);
            Assert.Single(model.ActionTypes);
        }

        [Fact]
        public async Task Create_Post_ValidModel_AddsQueueActionAndRedirectsToIndex()
        {
            // Arrange
            var inputModel = new CreateQueueActionVM
            {
                TicketId = 1,
                ActionTypeId = 2,
                ActionTime = DateTime.Now
            };

            // Act
            var result = await _controller.Create(inputModel);

            // Assert
            _queueActionRepoMock.Verify(r => r.AddAsync(It.Is<QueueAction>(q => q.TicketId == 1 && q.ActionTypeId == 2)), Times.Once);
            _queueActionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم إضافة الإجراء بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_ActionNotFound_ReturnsViewWithEmptyModelErrorAndDropdowns()
        {
            // Arrange
            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueAction)null);
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes>());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<UpdateQueueActionVM>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_Get_ActionFound_ReturnsViewWithModelAndDropdowns()
        {
            // Arrange
            var action = SetPropertyValue(new QueueAction { TicketId = 101, ActionTypeId = 2 }, "Id", 1);
            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(action);
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes>());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateQueueActionVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal(101, model.TicketId);
        }

        [Fact]
        public async Task Edit_Post_ActionNotFound_ReturnsViewWithModelErrorAndDropdowns()
        {
            // Arrange
            var inputModel = new UpdateQueueActionVM { Id = 1, TicketId = 101 };
            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueAction)null);
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes>());

            // Act
            var result = await _controller.Edit(inputModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inputModel, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _queueActionRepoMock.Verify(r => r.Update(It.IsAny<QueueAction>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesActionAndRedirectsToIndex()
        {
            // Arrange
            var inputModel = new UpdateQueueActionVM { Id = 1, TicketId = 200, ActionTypeId = 3 };
            var existingAction = SetPropertyValue(new QueueAction { TicketId = 100, ActionTypeId = 1 }, "Id", 1);

            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingAction);

            // Act
            var result = await _controller.Edit(inputModel);

            // Assert
            _queueActionRepoMock.Verify(r => r.Update(It.Is<QueueAction>(q => q.TicketId == 200 && q.ActionTypeId == 3)), Times.Once);
            _queueActionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم تعديل الإجراء بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Post_ActionNotFound_SetsTempDataErrorAndRedirectsToIndex()
        {
            // Arrange
            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueAction)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.Equal("تعذر الحذف، الإجراء غير موجود.", _controller.TempData["ErrorMessage"]);
            _queueActionRepoMock.Verify(r => r.Remove(It.IsAny<QueueAction>()), Times.Never);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_Post_ActionFound_RemovesActionAndRedirectsToIndex()
        {
            // Arrange
            var action = SetPropertyValue(new QueueAction { TicketId = 100 }, "Id", 1);
            _queueActionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(action);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            _queueActionRepoMock.Verify(r => r.Remove(action), Times.Once);
            _queueActionRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم حذف الإجراء بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_NullModel_ThrowsNullReferenceException()
        {
            // Arrange
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes>());

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _controller.Create((CreateQueueActionVM)null));
        }

        [Fact]
        public async Task Edit_Post_NullModel_ThrowsNullReferenceException()
        {
            // Arrange
            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _actionTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ActionTypes>());

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _controller.Edit((UpdateQueueActionVM)null));
        }

        #endregion
    }
}