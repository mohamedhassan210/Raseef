using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.ViewModels.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
// ضيف مسارات الـ Models والـ ViewModels والـ Interfaces هنا

namespace Rassef.Tests
{
    public class WarehousesControllerTests
    {
        private readonly Mock<IRepository<Warehouse>> _warehouseRepoMock;
        private readonly Mock<IRepository<Department>> _departmentRepoMock;
        private readonly Mock<IRepository<Dock>> _dockRepoMock;
        private readonly WarehousesController _controller;

        public WarehousesControllerTests()
        {
            _warehouseRepoMock = new Mock<IRepository<Warehouse>>();
            _departmentRepoMock = new Mock<IRepository<Department>>();
            _dockRepoMock = new Mock<IRepository<Dock>>();

            _controller = new WarehousesController(
                _warehouseRepoMock.Object,
                _departmentRepoMock.Object,
                _dockRepoMock.Object
            );
        }

        #region Helpers
        private T SetProperty<T>(T entity, string propertyName, object value)
        {
            entity.GetType().GetProperty(propertyName)?.SetValue(entity, value);
            return entity;
        }

        private T SetId<T>(T entity, int id)
        {
            return SetProperty(entity, "Id", id);
        }

        private Warehouse GetFakeWarehouse(int id)
        {
            var user = SetProperty(new User(), "UserName", "مستخدم تست");
            var dept = SetId(new Department { Name = "قسم 1" }, 1);
            var dock = SetId(new Dock { DockName = "رصيف 1" }, 1);

            var warehouse = new Warehouse
            {
                Name = "مستودع تست",
                Location = "موقع تست",
                CreatedBy = user,
                Departments = new List<Department> { dept },
                Docks = new List<Dock> { dock }
            };

            return SetId(warehouse, id);
        }

        private void SetupDropdownMocks()
        {
            _departmentRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Department> { SetId(new Department { Name = "قسم 1" }, 1) });

            _dockRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Dock> { SetId(new Dock { DockName = "رصيف 1" }, 1) });
        }
        #endregion

        #region Index Tests
        [Fact]
        public async Task Index_FriendlyCase_ReturnsData()
        {
            var warehouse = GetFakeWarehouse(1);
            _warehouseRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Warehouse> { warehouse });

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<WarehouseListVM>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("مستودع تست", model[0].Name);
            Assert.Equal(1, model[0].DepartmentsCount);
        }

        [Fact]
        public async Task Index_EdgeCase_EmptyData_ReturnsEmptyList()
        {
            _warehouseRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Warehouse>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<WarehouseListVM>>(viewResult.Model);
            Assert.Empty(model);
        }
        #endregion

        #region Details Tests
        [Fact]
        public async Task Details_WorstCase_NullId_ReturnsError()
        {
            var result = await _controller.Details(null);

            Assert.False(_controller.ModelState.IsValid);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<WarehouseDetailsVM>(viewResult.Model);
        }

        [Fact]
        public async Task Details_WorstCase_NotFound_ReturnsError()
        {
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Warehouse)null);

            var result = await _controller.Details(99);

            Assert.False(_controller.ModelState.IsValid);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<WarehouseDetailsVM>(viewResult.Model);
        }

        [Fact]
        public async Task Details_FriendlyCase_ReturnsWarehouse()
        {
            var warehouse = GetFakeWarehouse(1);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(warehouse);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WarehouseDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Single(model.DepartmentNames);
        }
        #endregion

        #region Create GET Tests
        [Fact]
        public async Task CreateGet_FriendlyCase_PopulatesLists_ReturnsView()
        {
            SetupDropdownMocks();

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateWarehouseVM>(viewResult.Model);
            Assert.NotNull(model.Departments);
            Assert.NotNull(model.Docks);
        }
        #endregion

        #region Create POST Tests
        [Fact]
        public async Task CreatePost_WorstCase_NameExists_ReturnsError()
        {
            SetupDropdownMocks();
            _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(true);

            var result = await _controller.Create(new CreateWarehouseVM { Name = "موجود" });

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Name"));
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateWarehouseVM>(viewResult.Model);
        }

        [Fact]
        public async Task CreatePost_WorstCase_InvalidModel_ReturnsView()
        {
            SetupDropdownMocks();
            _controller.ModelState.AddModelError("Error", "Error");

            _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(false);

            var result = await _controller.Create(new CreateWarehouseVM());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            SetupDropdownMocks();
            _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(false);

            var model = new CreateWarehouseVM
            {
                Name = "جديد",
                SelectedDepartmentIds = new List<int> { 1 },
                SelectedDockIds = new List<int> { 1 }
            };

            var result = await _controller.Create(model);

            _warehouseRepoMock.Verify(r => r.AddAsync(It.IsAny<Warehouse>()), Times.Once);
            _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        #endregion

        #region Edit GET Tests
        [Fact]
        public async Task EditGet_WorstCase_NullId_ReturnsError()
        {
            SetupDropdownMocks();
            var result = await _controller.Edit(null);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditGet_WorstCase_NotFound_ReturnsError()
        {
            SetupDropdownMocks();
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Warehouse)null);

            var result = await _controller.Edit(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditGet_FriendlyCase_ReturnsPopulatedVM()
        {
            SetupDropdownMocks();
            var warehouse = GetFakeWarehouse(1);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(warehouse);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateWarehouseVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Contains(1, model.SelectedDepartmentIds);
            Assert.NotNull(model.Departments);
        }
        #endregion

        #region Edit POST Tests
        [Fact]
        public async Task EditPost_WorstCase_IdMismatch_ReturnsError()
        {
            SetupDropdownMocks();
            var result = await _controller.Edit(1, new UpdateWarehouseVM { Id = 2 });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditPost_WorstCase_NameExistsForOther_ReturnsError()
        {
            SetupDropdownMocks();
            _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(true);

            var result = await _controller.Edit(1, new UpdateWarehouseVM { Id = 1, Name = "مستخدم" });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditPost_WorstCase_NotFoundDuringUpdate_ReturnsError()
        {
            SetupDropdownMocks();
            _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(false);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Warehouse)null);

            var result = await _controller.Edit(1, new UpdateWarehouseVM { Id = 1 });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task EditPost_FriendlyCase_Success_RedirectsToIndex()
        {
            SetupDropdownMocks();
            var warehouse = GetFakeWarehouse(1);
            _warehouseRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .ReturnsAsync(false);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(warehouse);

            var model = new UpdateWarehouseVM { Id = 1, Name = "تعديل", SelectedDepartmentIds = new List<int> { 1 } };

            var result = await _controller.Edit(1, model);

            _warehouseRepoMock.Verify(r => r.Update(warehouse), Times.Once);
            _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion

        #region Delete GET Tests
        [Fact]
        public async Task DeleteGet_WorstCase_NullId_ReturnsError()
        {
            var result = await _controller.Delete(null);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteGet_WorstCase_NotFound_ReturnsError()
        {
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Warehouse)null);

            var result = await _controller.Delete(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteGet_FriendlyCase_ReturnsData()
        {
            var warehouse = GetFakeWarehouse(1);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(warehouse);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<WarehouseListVM>(viewResult.Model);
        }
        #endregion

        #region Delete POST Tests
        [Fact]
        public async Task DeletePost_WorstCase_NotFound_ReturnsError()
        {
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Warehouse)null);

            var result = await _controller.DeleteConfirmed(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeletePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var warehouse = GetFakeWarehouse(1);
            _warehouseRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(warehouse);

            var result = await _controller.DeleteConfirmed(1);

            _warehouseRepoMock.Verify(r => r.Remove(warehouse), Times.Once);
            _warehouseRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion
    }
}