using Microsoft.AspNetCore.Mvc;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.Dock; // اضبط المجلد حسب مشروعك
using System.Reflection;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class DockControllerTests
    {
        private readonly Mock<IDockRepository> _dockRepoMock;
        private readonly Mock<IRepository<Department>> _departmentRepoMock;
        private readonly Mock<IRepository<Warehouse>> _warehouseRepoMock;
        private readonly Mock<IRepository<DockStatuses>> _statusRepoMock;
        private readonly DockController _controller;

        public DockControllerTests()
        {
            _dockRepoMock = new Mock<IDockRepository>();
            _departmentRepoMock = new Mock<IRepository<Department>>();
            _warehouseRepoMock = new Mock<IRepository<Warehouse>>();
            _statusRepoMock = new Mock<IRepository<DockStatuses>>();

            _controller = new DockController(
                _dockRepoMock.Object,
                _departmentRepoMock.Object,
                _warehouseRepoMock.Object,
                _statusRepoMock.Object
            );
        }

        // دالة عامة لتجاوز private/protected set لأي Property داخل الـ Entities
        private static T SetPropertyValue<T>(T entity, string propertyName, object value) where T : class
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(entity, value);
            return entity;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithMappedViewModels()
        {
            // Arrange
            var status = SetPropertyValue(new DockStatuses(), "Name", "Active");
            var warehouse = SetPropertyValue(new Warehouse(), "Name", "Main Warehouse");
            var department = SetPropertyValue(new Department(), "Name", "Logistics");

            var dock = SetPropertyValue(new Dock
            {
                DockName = "Dock 1",
                Warehouse = warehouse,
                Department = department,
                DockStatus = status
            }, "Id", 1);

            _dockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Dock> { dock });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<DockListVM>>(viewResult.Model);
            var list = model.ToList();
            Assert.Single(list);
            Assert.Equal("Dock 1", list[0].DockName);
            Assert.Equal("Main Warehouse", list[0].WarehouseName);
            Assert.Equal("Logistics", list[0].DepartmentName);
            Assert.Equal("Active", list[0].DockStatusName);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_NotFound_ReturnsViewWithNullModel()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Dock)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task Details_Found_ReturnsViewWithDetailsVM()
        {
            // Arrange
            var status = SetPropertyValue(new DockStatuses(), "Name", "Available");
            var warehouse = SetPropertyValue(new Warehouse(), "Name", "Warehouse B");
            var department = SetPropertyValue(new Department(), "Name", "Ops");

            var dock = SetPropertyValue(new Dock
            {
                DockName = "Dock 2",
                Warehouse = warehouse,
                Department = department,
                DockStatus = status,
                CreatedById = 10
            }, "Id", 2);

            _dockRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(dock);

            // Act
            var result = await _controller.Details(2);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DockDetailsVM>(viewResult.Model);
            Assert.Equal(2, model.Id);
            Assert.Equal("Dock 2", model.DockName);
            Assert.Equal("Warehouse B", model.WarehouseName);
            Assert.Equal("Ops", model.DepartmentName);
            Assert.Equal("Available", model.DockStatusName);
            Assert.Equal(10, model.CreatedBy);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_PopulatesDropdownsAndReturnsView()
        {
            // Arrange
            var dept = SetPropertyValue(SetPropertyValue(new Department(), "Name", "Dept 1"), "Id", 1);
            var wh = SetPropertyValue(SetPropertyValue(new Warehouse(), "Name", "Wh 1"), "Id", 1);
            var st = SetPropertyValue(SetPropertyValue(new DockStatuses(), "Name", "Status 1"), "Id", 1);

            _departmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department> { dept });
            _warehouseRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Warehouse> { wh });
            _statusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DockStatuses> { st });

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateDockVM>(viewResult.Model);
            Assert.Single(model.Departments);
            Assert.Single(model.Warehouses);
            Assert.Single(model.DockStatus);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithDropdowns()
        {
            // Arrange
            _controller.ModelState.AddModelError("DockName", "Required");
            _departmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());
            _warehouseRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Warehouse>());
            _statusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DockStatuses>());

            var createVm = new CreateDockVM();

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateDockVM>(viewResult.Model);
            Assert.NotNull(model.Departments);
            Assert.NotNull(model.Warehouses);
            Assert.NotNull(model.DockStatus);
        }

        [Fact]
        public async Task Create_Post_ValidData_AddsDockAndRedirectsToIndex()
        {
            // Arrange
            var createVm = new CreateDockVM
            {
                DockName = "New Dock",
                DepartmentId = 1,
                WarehouseId = 2,
                DockStatusId = 3
            };

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            _dockRepoMock.Verify(r => r.AddAsync(It.Is<Dock>(d =>
                d.DockName == "New Dock" &&
                d.DepartmentId == 1 &&
                d.WarehouseId == 2 &&
                d.DockStatusId == 3
            )), Times.Once);
            _dockRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DockController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Update GET Tests

        [Fact]
        public async Task Update_Get_NotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Dock)null);

            // Act
            var result = await _controller.Update(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Get_Found_ReturnsViewWithPopulatedVM()
        {
            // Arrange
            var dock = SetPropertyValue(new Dock
            {
                DockName = "Existing Dock",
                DepartmentId = 1,
                WarehouseId = 2,
                DockStatusId = 3
            }, "Id", 5);

            _dockRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dock);
            _departmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());
            _warehouseRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Warehouse>());
            _statusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DockStatuses>());

            // Act
            var result = await _controller.Update(5);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateDockVM>(viewResult.Model);
            Assert.Equal(5, model.Id);
            Assert.Equal("Existing Dock", model.DockName);
            Assert.Equal(1, model.DepartmentId);
            Assert.Equal(2, model.WarehouseId);
            Assert.Equal(3, model.DockStatusId);
        }

        #endregion

        #region Update POST Tests

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithDropdowns()
        {
            // Arrange
            _controller.ModelState.AddModelError("DockName", "Required");
            _departmentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());
            _warehouseRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Warehouse>());
            _statusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DockStatuses>());

            var updateVm = new UpdateDockVM();

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateDockVM>(viewResult.Model);
            Assert.NotNull(model.Departments);
            Assert.NotNull(model.Warehouses);
            Assert.NotNull(model.DockStatus);
        }

        [Fact]
        public async Task Update_Post_NotFound_ReturnsNotFoundResult()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Dock)null);

            var updateVm = new UpdateDockVM { Id = 1 };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_Post_ValidData_UpdatesDockAndRedirectsToIndex()
        {
            // Arrange
            var existingDock = SetPropertyValue(new Dock
            {
                DockName = "Old Name",
                DepartmentId = 1,
                WarehouseId = 1,
                DockStatusId = 1
            }, "Id", 10);

            _dockRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existingDock);

            var updateVm = new UpdateDockVM
            {
                Id = 10,
                DockName = "Updated Name",
                DepartmentId = 2,
                WarehouseId = 3,
                DockStatusId = 4
            };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            Assert.Equal("Updated Name", existingDock.DockName);
            Assert.Equal(2, existingDock.DepartmentId);
            Assert.Equal(3, existingDock.WarehouseId);
            Assert.Equal(4, existingDock.DockStatusId);

            _dockRepoMock.Verify(r => r.Update(existingDock), Times.Once);
            _dockRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DockController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task Delete_Get_NotFound_ReturnsNotFoundResult()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Dock)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_Found_ReturnsViewWithDockDetailsVM()
        {
            // Arrange
            var dock = SetPropertyValue(new Dock { DockName = "Dock to Delete" }, "Id", 3);
            _dockRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(dock);

            // Act
            var result = await _controller.Delete(3);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DockDetailsVM>(viewResult.Model);
            Assert.Equal(3, model.Id);
            Assert.Equal("Dock to Delete", model.DockName);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteConfirmed_Post_NotFound_ReturnsViewWithNullModel()
        {
            // Arrange
            _dockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Dock)null);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            _dockRepoMock.Verify(r => r.Remove(It.IsAny<Dock>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_Found_RemovesDockAndRedirectsToIndex()
        {
            // Arrange
            var dock = SetPropertyValue(new Dock { DockName = "Dock to Remove" }, "Id", 1);
            _dockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dock);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _dockRepoMock.Verify(r => r.Remove(dock), Times.Once);
            _dockRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DockController.Index), redirectResult.ActionName);
        }

        #endregion
    }
}