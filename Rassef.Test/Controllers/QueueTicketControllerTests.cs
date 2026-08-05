using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.QueueTicket;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class QueueTicketControllerTests
    {
        private readonly Mock<IRepository<QueueTicket>> _ticketRepoMock;
        private readonly Mock<IRepository<Department>> _departmentRepoMock;
        private readonly Mock<IRepository<TicketStatuses>> _ticketStatusRepoMock;
        private readonly Mock<IRepository<TransferRequest>> _transferRequestRepoMock;
        private readonly Mock<IRepository<SupplierRequest>> _supplierRequestRepoMock;
        private readonly QueueTicketController _controller;

        public QueueTicketControllerTests()
        {
            _ticketRepoMock = new Mock<IRepository<QueueTicket>>();
            _departmentRepoMock = new Mock<IRepository<Department>>();
            _ticketStatusRepoMock = new Mock<IRepository<TicketStatuses>>();
            _transferRequestRepoMock = new Mock<IRepository<TransferRequest>>();
            _supplierRequestRepoMock = new Mock<IRepository<SupplierRequest>>();

            _controller = new QueueTicketController(
                _ticketRepoMock.Object,
                _departmentRepoMock.Object,
                _ticketStatusRepoMock.Object,
                _transferRequestRepoMock.Object,
                _supplierRequestRepoMock.Object
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
        public async Task Index_ReturnsViewWithMappedQueueTicketListVM()
        {
            // Arrange
            var ticket1 = SetPropertyValue(new QueueTicket
            {
                TicketNumber = "T-01",
                DepartmentId = 10,
                TicketStatusId = 20
            }, "Id", 1);

            var ticket2 = SetPropertyValue(new QueueTicket
            {
                TicketNumber = "T-02",
                DepartmentId = 99,
                TicketStatusId = 99
            }, "Id", 2);

            var dept = SetPropertyValue(SetPropertyValue(new Department(), "Name", "IT"), "Id", 10);
            var status = SetPropertyValue(SetPropertyValue(new TicketStatuses(), "Name", "Active"), "Id", 20);

            _ticketRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<QueueTicket> { ticket1, ticket2 });
            _departmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department> { dept });
            _ticketStatusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TicketStatuses> { status });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<QueueTicketListVM>>(viewResult.Model);
            var list = model.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("IT", list[0].DepartmentName);
            Assert.Equal("Active", list[0].TicketStatusName);
            Assert.Equal("غير محدد", list[1].DepartmentName);
            Assert.Equal("غير محدد", list[1].TicketStatusName);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_TicketNotFound_ReturnsViewWithEmptyVMAndModelError()
        {
            // Arrange
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueTicket)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueueTicketDetailsVM>(viewResult.Model);
            Assert.Equal(0, model.Id);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_TicketFound_FullDetailsMappedCorrectly()
        {
            // Arrange
            var ticket = SetPropertyValue(new QueueTicket
            {
                TicketNumber = "T-100",
                DepartmentId = 1,
                TicketStatusId = 2,
                TransferRequestId = 5,
                SupplierRequestId = 10,
                CreatedBy = new ApplicationUser { UserName = "ahmed_user", Name = "ahmed_user" },
                DockAssignments = new List<DockAssignment> { new DockAssignment() },
                QueueActions = new List<QueueAction> { new QueueAction(), new QueueAction() },
                CheckOut = new CheckOut()
            }, "Id", 1);

            var dept = SetPropertyValue(SetPropertyValue(new Department(), "Name", "HR"), "Id", 1);
            var status = SetPropertyValue(SetPropertyValue(new TicketStatuses(), "Name", "Pending"), "Id", 2);

            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);
            _departmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dept);
            _ticketStatusRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(status);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueueTicketDetailsVM>(viewResult.Model);

            Assert.Equal("T-100", model.TicketNumber);
            Assert.Equal("HR", model.DepartmentName);
            Assert.Equal("Pending", model.TicketStatusName);
            Assert.Equal("طلب تحويل #5", model.TransferRequestInfo);
            Assert.Equal("طلب مورد #10", model.SupplierRequestInfo);
            Assert.Equal("ahmed_user", model.CreatedByUserName);
            Assert.Equal(1, model.DockAssignmentsCount);
            Assert.Equal(2, model.QueueActionsCount);
            Assert.True(model.HasCheckedOut);
        }

        [Fact]
        public async Task Details_TicketFound_NullRelations_ReturnsDefaults()
        {
            // Arrange
            var ticket = SetPropertyValue(new QueueTicket
            {
                TicketNumber = "T-101",
                DepartmentId = 1,
                TicketStatusId = 2
            }, "Id", 1);

            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);
            _departmentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Department)null);
            _ticketStatusRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((TicketStatuses)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QueueTicketDetailsVM>(viewResult.Model);

            Assert.Equal("غير محدد", model.DepartmentName);
            Assert.Equal("غير محدد", model.TicketStatusName);
            Assert.Equal("لا يوجد", model.TransferRequestInfo);
            Assert.Equal("لا يوجد", model.SupplierRequestInfo);
            Assert.Equal("غير محدد", model.CreatedByUserName);
            Assert.Equal(0, model.DockAssignmentsCount);
            Assert.Equal(0, model.QueueActionsCount);
            Assert.False(model.HasCheckedOut);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_Get_ReturnsViewWithPopulatedDropdowns()
        {
            // Arrange
            SetupDropdownMocks();

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateQueueTicketVM>(viewResult.Model);
            Assert.Single(model.Departments);
            Assert.Single(model.TicketStatuses);
            Assert.Single(model.TransferRequests);
            Assert.Single(model.SupplierRequests);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithDropdowns()
        {
            // Arrange
            SetupDropdownMocks();
            var invalidModel = new CreateQueueTicketVM
            {
                TicketNumber = "", // Invalid
                DepartmentId = 0,   // Invalid
                TicketStatusId = 0  // Invalid
            };

            // Act
            var result = await _controller.Create(invalidModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(3, _controller.ModelState.ErrorCount);
            _ticketRepoMock.Verify(r => r.AddAsync(It.IsAny<QueueTicket>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_ValidModel_AddsTicketAndRedirects()
        {
            // Arrange
            var validModel = new CreateQueueTicketVM
            {
                TicketNumber = "T-500",
                DepartmentId = 1,
                TicketStatusId = 2
            };

            // Act
            var result = await _controller.Create(validModel);

            // Assert
            _ticketRepoMock.Verify(r => r.AddAsync(It.Is<QueueTicket>(t => t.TicketNumber == "T-500" && t.DepartmentId == 1)), Times.Once);
            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم إنشاء التذكرة بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_TicketNotFound_ReturnsEmptyModelWithDropdownsAndError()
        {
            // Arrange
            SetupDropdownMocks();
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueTicket)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<UpdateQueueTicketVM>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_Get_TicketFound_ReturnsViewWithModelAndDropdowns()
        {
            // Arrange
            SetupDropdownMocks();
            var ticket = SetPropertyValue(new QueueTicket { TicketNumber = "T-99", DepartmentId = 1, TicketStatusId = 2 }, "Id", 1);
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateQueueTicketVM>(viewResult.Model);
            Assert.Equal("T-99", model.TicketNumber);
            Assert.Single(model.Departments);
        }

        [Fact]
        public async Task Edit_Post_InvalidValidation_ReturnsViewWithDropdowns()
        {
            // Arrange
            SetupDropdownMocks();
            var invalidModel = new UpdateQueueTicketVM
            {
                Id = 0,
                TicketNumber = null,
                DepartmentId = -1,
                TicketStatusId = -1
            };

            // Act
            var result = await _controller.Edit(invalidModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _ticketRepoMock.Verify(r => r.Update(It.IsAny<QueueTicket>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_TicketNotFoundInDb_ReturnsViewWithModelError()
        {
            // Arrange
            SetupDropdownMocks();
            var model = new UpdateQueueTicketVM { Id = 10, TicketNumber = "T-10", DepartmentId = 1, TicketStatusId = 1 };
            _ticketRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((QueueTicket)null);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _ticketRepoMock.Verify(r => r.Update(It.IsAny<QueueTicket>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
        {
            // Arrange
            var existingTicket = SetPropertyValue(new QueueTicket { TicketNumber = "Old", DepartmentId = 1, TicketStatusId = 1 }, "Id", 5);
            _ticketRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existingTicket);

            var updateModel = new UpdateQueueTicketVM
            {
                Id = 5,
                TicketNumber = "New-Number",
                DepartmentId = 2,
                TicketStatusId = 3
            };

            // Act
            var result = await _controller.Edit(updateModel);

            // Assert
            _ticketRepoMock.Verify(r => r.Update(It.Is<QueueTicket>(t => t.TicketNumber == "New-Number" && t.DepartmentId == 2 && t.TicketStatusId == 3)), Times.Once);
            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم تعديل بيانات التذكرة بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Post_TicketNotFound_SetsTempDataErrorAndRedirects()
        {
            // Arrange
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((QueueTicket)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.Equal("تعذر الحذف، التذكرة غير موجودة.", _controller.TempData["ErrorMessage"]);
            _ticketRepoMock.Verify(r => r.Remove(It.IsAny<QueueTicket>()), Times.Never);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_Post_TicketFound_RemovesAndRedirects()
        {
            // Arrange
            var ticket = SetPropertyValue(new QueueTicket { TicketNumber = "T-Del" }, "Id", 1);
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            _ticketRepoMock.Verify(r => r.Remove(ticket), Times.Once);
            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم حذف التذكرة بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Helper Methods

        private void SetupDropdownMocks()
        {
            var dept = SetPropertyValue(SetPropertyValue(new Department(), "Name", "Dept1"), "Id", 1);
            var status = SetPropertyValue(SetPropertyValue(new TicketStatuses(), "Name", "Status1"), "Id", 1);
            var transfer = SetPropertyValue(new TransferRequest(), "Id", 1);
            var supplier = SetPropertyValue(new SupplierRequest(), "Id", 1);

            _departmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department> { dept });
            _ticketStatusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TicketStatuses> { status });
            _transferRequestRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TransferRequest> { transfer });
            _supplierRequestRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SupplierRequest> { supplier });
        }

        [Fact]
        public async Task Create_Get_PopulatesAllDropdownsFromRepositories()
        {
            // Arrange
            SetupDropdownMocks();

            // Act
            await _controller.Create();

            // Assert
            _departmentRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _ticketStatusRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _transferRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _supplierRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_RepopulatesAllDropdowns()
        {
            // Arrange
            SetupDropdownMocks();
            var invalidModel = new CreateQueueTicketVM();

            // Act
            await _controller.Create(invalidModel);

            // Assert
            _departmentRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _ticketStatusRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _transferRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _supplierRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Edit_Get_TicketFound_PopulatesAllDropdownsFromRepositories()
        {
            // Arrange
            SetupDropdownMocks();
            var ticket = SetPropertyValue(new QueueTicket { TicketNumber = "T-100", DepartmentId = 1, TicketStatusId = 1 }, "Id", 1);
            _ticketRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

            // Act
            await _controller.Edit(1);

            // Assert
            _departmentRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _ticketStatusRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _transferRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _supplierRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_RepopulatesAllDropdowns()
        {
            // Arrange
            SetupDropdownMocks();
            var invalidModel = new UpdateQueueTicketVM { Id = 0 };

            // Act
            await _controller.Edit(invalidModel);

            // Assert
            _departmentRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _ticketStatusRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _transferRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _supplierRequestRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        #endregion
    }
}