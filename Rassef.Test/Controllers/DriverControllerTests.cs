using Microsoft.AspNetCore.Mvc;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.ViewModels.Driver;  // اضبط النايم سبيس حسب مشروعك
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class DriverControllerTests
    {
        private readonly Mock<IDriverRepository> _driverRepoMock;
        private readonly Mock<ISupplierRepository> _supplierRepoMock;
        private readonly Mock<ITruckRepository> _truckRepoMock;
        private readonly DriverController _controller;

        public DriverControllerTests()
        {
            _driverRepoMock = new Mock<IDriverRepository>();
            _supplierRepoMock = new Mock<ISupplierRepository>();
            _truckRepoMock = new Mock<ITruckRepository>();

            _controller = new DriverController(
                _driverRepoMock.Object,
                _supplierRepoMock.Object,
                _truckRepoMock.Object
            );

            // إعداد ClaimsPrincipal للتعامل مع User.FindFirstValue(ClaimTypes.NameIdentifier)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123")
            }, "TestAuthentication"));

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
        public async Task Index_SupplierNotFound_RedirectsToSupplierIndex()
        {
            // Arrange
            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Supplier)null);

            // Act
            var result = await _controller.Index(10, 1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Supplier", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_SupplierFound_ReturnsViewWithDriversAndPopulatesViewBag()
        {
            // Arrange
            var supplier = SetPropertyValue(new Supplier(), "Name", "Test Supplier");
            var truck = new Truck { PlateLetter = "ABC", PlateNumber = "1234" };
            var drivers = new List<Driver>
            {
                SetPropertyValue(new Driver { FullName = "Driver 1", NationalId = "111", Phone = "010" }, "Id", 1)
            };

            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);
            _truckRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(truck);
            _driverRepoMock.Setup(r => r.GetDriversBySupplierIdAsync(1)).ReturnsAsync(drivers);

            // Act
            var result = await _controller.Index(10, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<DriverListVM>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Driver 1", model[0].FullName);
            Assert.Equal("Test Supplier", _controller.ViewBag.SupplierName);
            Assert.Equal("ABC 1234", _controller.ViewBag.TruckName);
        }

        [Fact]
        public async Task Index_TruckNull_PopulatesFallbackTruckNameInViewBag()
        {
            // Arrange
            var supplier = SetPropertyValue(new Supplier(), "Name", "Test Supplier");
            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);
            _truckRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Truck)null);
            _driverRepoMock.Setup(r => r.GetDriversBySupplierIdAsync(1)).ReturnsAsync(new List<Driver>());

            // Act
            await _controller.Index(10, 1);

            // Assert
            Assert.Equal("سيارة غير محددة", _controller.ViewBag.TruckName);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_DriverNotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Driver)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_DriverFound_ReturnsViewWithDriverDetailsVM()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver
            {
                FullName = "Ahmed",
                NationalId = "12345678901234",
                Phone = "01000000000",
                CreatedById = 5
            }, "Id", 1);

            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DriverDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Ahmed", model.FullName);
            Assert.Equal(5, model.CreatedBy);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_PopulatesSuppliersAndReturnsView()
        {
            // Arrange
            var suppliers = new List<Supplier>
            {
                SetPropertyValue(SetPropertyValue(new Supplier(), "Name", "Supplier A"), "Id", 1)
            };
            _supplierRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(suppliers);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateDriverVM>(viewResult.Model);
            Assert.Single(model.Suppliers);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithSuppliers()
        {
            // Arrange
            _controller.ModelState.AddModelError("FullName", "Required");
            _supplierRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>());

            var createVm = new CreateDriverVM();

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateDriverVM>(viewResult.Model);
            Assert.NotNull(model.Suppliers);
        }

        [Fact]
        public async Task Create_Post_NationalIdExists_ReturnsViewWithModelError()
        {
            // Arrange
            var createVm = new CreateDriverVM { NationalId = "123456" };
            _driverRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(true);
            _supplierRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>());

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(CreateDriverVM.NationalId)));
        }

        [Fact]
        public async Task Create_Post_PhoneExists_ReturnsViewWithModelError()
        {
            // Arrange
            var createVm = new CreateDriverVM { NationalId = "123456", Phone = "010123" };
            _driverRepoMock.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                           .ReturnsAsync(false) // NationalId unique
                           .ReturnsAsync(true);  // Phone exists
            _supplierRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>());

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(CreateDriverVM.Phone)));
        }

        [Fact]
        public async Task Create_Post_ValidData_AddsDriverAndRedirectsToIndex()
        {
            // Arrange
            var createVm = new CreateDriverVM
            {
                FullName = "Driver New",
                NationalId = "10020030040050",
                Phone = "01122334455"
            };

            _driverRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(false);

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            _driverRepoMock.Verify(r => r.AddAsync(It.Is<Driver>(d =>
                d.FullName == "Driver New" &&
                d.NationalId == "10020030040050" &&
                d.Phone == "01122334455" &&
                d.CreatedById == 123
            )), Times.Once);
            _driverRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DriverController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Update GET Tests

        [Fact]
        public async Task Update_Get_DriverNotFound_ReturnsViewWithModelError()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Driver)null);

            // Act
            var result = await _controller.Update(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Get_DriverFound_ReturnsViewWithUpdateDriverVM()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver
            {
                FullName = "Driver Update",
                NationalId = "999888",
                Phone = "0123"
            }, "Id", 5);

            _driverRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(driver);

            // Act
            var result = await _controller.Update(5);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateDriverVM>(viewResult.Model);
            Assert.Equal(5, model.Id);
            Assert.Equal("Driver Update", model.FullName);
        }

        #endregion

        #region Update POST Tests

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithVM()
        {
            // Arrange
            _controller.ModelState.AddModelError("FullName", "Required");
            var updateVm = new UpdateDriverVM();

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<UpdateDriverVM>(viewResult.Model);
        }

        [Fact]
        public async Task Update_Post_DriverNotFound_ReturnsViewWithModelError()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Driver)null);
            var updateVm = new UpdateDriverVM { Id = 1 };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Post_NationalIdExistsForAnotherDriver_ReturnsViewWithModelError()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver(), "Id", 1);
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
            _driverRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(true);

            var updateVm = new UpdateDriverVM { Id = 1, NationalId = "111" };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(UpdateDriverVM.NationalId)));
        }

        [Fact]
        public async Task Update_Post_PhoneExistsForAnotherDriver_ReturnsViewWithModelError()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver(), "Id", 1);
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
            _driverRepoMock.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Driver, bool>>>()))
                           .ReturnsAsync(false) // NationalId unique
                           .ReturnsAsync(true);  // Phone exists

            var updateVm = new UpdateDriverVM { Id = 1, NationalId = "111", Phone = "010" };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(UpdateDriverVM.Phone)));
        }

        [Fact]
        public async Task Update_Post_ValidData_UpdatesDriverAndRedirectsToIndex()
        {
            // Arrange
            var existingDriver = SetPropertyValue(new Driver
            {
                FullName = "Old Name",
                NationalId = "111",
                Phone = "010"
            }, "Id", 10);

            _driverRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existingDriver);
            _driverRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Driver, bool>>>())).ReturnsAsync(false);

            var updateVm = new UpdateDriverVM
            {
                Id = 10,
                FullName = "Updated Name",
                NationalId = "222",
                Phone = "011"
            };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            Assert.Equal("Updated Name", existingDriver.FullName);
            Assert.Equal("222", existingDriver.NationalId);
            Assert.Equal("011", existingDriver.Phone);

            _driverRepoMock.Verify(r => r.Update(existingDriver), Times.Once);
            _driverRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DriverController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task Delete_Get_DriverNotFound_ReturnsViewWithModelError()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Driver)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_Get_DriverFound_ReturnsViewWithDriverDetailsVM()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver
            {
                FullName = "Driver To Delete",
                NationalId = "123",
                Phone = "010"
            }, "Id", 3);

            _driverRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(driver);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DriverDetailsVM>(viewResult.Model);
            Assert.Equal(3, model.Id);
            Assert.Equal("Driver To Delete", model.FullName);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteDriver_Post_DriverNotFound_RedirectsToIndex()
        {
            // Arrange
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Driver)null);

            // Act
            var result = await _controller.DeleteDriver(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DriverController.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteDriver_Post_HasRequests_ReturnsDeleteViewWithModelError()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver { FullName = "Driver With Requests" }, "Id", 1);
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
            _driverRepoMock.Setup(r => r.HasRequestsAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteDriver(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Delete", viewResult.ViewName);
            var model = Assert.IsType<DriverDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.False(_controller.ModelState.IsValid);
            _driverRepoMock.Verify(r => r.Remove(It.IsAny<Driver>()), Times.Never);
        }

        [Fact]
        public async Task DeleteDriver_Post_NoRequests_RemovesDriverAndRedirectsToIndex()
        {
            // Arrange
            var driver = SetPropertyValue(new Driver { FullName = "Driver Ready For Delete" }, "Id", 1);
            _driverRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
            _driverRepoMock.Setup(r => r.HasRequestsAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteDriver(1);

            // Assert
            _driverRepoMock.Verify(r => r.Remove(driver), Times.Once);
            _driverRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DriverController.Index), redirectResult.ActionName);
        }

        #endregion
    }
}