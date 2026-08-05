using Microsoft.AspNetCore.Mvc;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.ViewModels.Department;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class DepartmentControllerTests
    {
        private readonly Mock<IDepartmentRepository> _repositoryMock;
        private readonly DepartmentController _controller;

        public DepartmentControllerTests()
        {
            _repositoryMock = new Mock<IDepartmentRepository>();
            _controller = new DepartmentController(_repositoryMock.Object);
        }

        // دالة مساعدة لتعيين قيمة Id الموروثة ذات الـ private set
        private static T CreateEntityWithId<T>(int id) where T : new()
        {
            var entity = new T();
            var prop = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(entity, id);
            return entity;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfDepartmentListVM()
        {
            // Arrange
            var dept1 = CreateEntityWithId<Department>(1);
            dept1.Name = "Electronics";
            dept1.Warehouse = new Warehouse { Name = "Main Warehouse" };

            var dept2 = CreateEntityWithId<Department>(2);
            dept2.Name = "Groceries";
            dept2.Warehouse = new Warehouse { Name = "Secondary Warehouse" };

            var departments = new List<Department> { dept1, dept2 };

            _repositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(departments);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<DepartmentListVM>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Electronics", model[0].Name);
            Assert.Equal("Main Warehouse", model[0].WarehouseName);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_DepartmentNotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Department)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Details_DepartmentFound_ReturnsViewResultWithModel()
        {
            // Arrange
            var department = CreateEntityWithId<Department>(1);
            department.Name = "IT";
            department.WarehouseId = 10;
            department.Warehouse = new Warehouse { Name = "Tech Hub" };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(department);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DepartmentDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("IT", model.Name);
            Assert.Equal("Tech Hub", model.WarehouseName);
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
            _controller.ModelState.AddModelError("Name", "Required");
            var createVm = new CreateDepartmentVM();

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(createVm, viewResult.Model);
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_DepartmentNameExists_ReturnsViewWithModelStateError()
        {
            // Arrange
            var createVm = new CreateDepartmentVM { Name = "Existing Name", WarehouseId = 1 };

            _repositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(CreateDepartmentVM.Name)));
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_ValidData_AddsDepartmentAndRedirectsToIndex()
        {
            // Arrange
            var createVm = new CreateDepartmentVM { Name = "New Dept", WarehouseId = 2 };

            _repositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Department>(d => d.Name == createVm.Name && d.WarehouseId == createVm.WarehouseId)), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DepartmentController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_Get_DepartmentNotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Department)null);

            // Act
            var result = await _controller.Update(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Get_DepartmentFound_ReturnsViewWithModel()
        {
            // Arrange
            var department = CreateEntityWithId<Department>(1);
            department.Name = "HR";
            department.WarehouseId = 5;

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(department);

            // Act
            var result = await _controller.Update(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateDepartmentVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("HR", model.Name);
            Assert.Equal(5, model.WarehouseId);
        }

        [Fact]
        public async Task Update_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");
            var updateVm = new UpdateDepartmentVM { Id = 1 };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(updateVm, viewResult.Model);
        }

        [Fact]
        public async Task Update_Post_DepartmentNotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            var updateVm = new UpdateDepartmentVM { Id = 99, Name = "NonExisting" };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(99))
                .ReturnsAsync((Department)null);

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Post_DuplicateName_ReturnsViewWithModelStateError()
        {
            // Arrange
            var updateVm = new UpdateDepartmentVM { Id = 1, Name = "Duplicate Name", WarehouseId = 2 };
            var existingDept = CreateEntityWithId<Department>(1);
            existingDept.Name = "Old Name";
            existingDept.WarehouseId = 2;

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingDept);

            _repositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(UpdateDepartmentVM.Name)));
            _repositoryMock.Verify(repo => repo.Update(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public async Task Update_Post_ValidData_UpdatesDepartmentAndRedirectsToIndex()
        {
            // Arrange
            var updateVm = new UpdateDepartmentVM { Id = 1, Name = "Updated Name", WarehouseId = 3 };
            var existingDept = CreateEntityWithId<Department>(1);
            existingDept.Name = "Old Name";
            existingDept.WarehouseId = 2;

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingDept);

            _repositoryMock
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            Assert.Equal("Updated Name", existingDept.Name);
            Assert.Equal(3, existingDept.WarehouseId);

            _repositoryMock.Verify(repo => repo.Update(existingDept), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DepartmentController.Index), redirectResult.ActionName);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Get_DepartmentNotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Department)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_Get_DepartmentFound_ReturnsViewWithModel()
        {
            // Arrange
            var department = CreateEntityWithId<Department>(1);
            department.Name = "Logistics";
            department.Warehouse = new Warehouse { Name = "North Hub" };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(department);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DepartmentDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Logistics", model.Name);
            Assert.Equal("North Hub", model.WarehouseName);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DepartmentNotFound_ReturnsViewWithModelStateError()
        {
            // Arrange
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Department)null);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _repositoryMock.Verify(repo => repo.Remove(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DepartmentFound_RemovesDepartmentAndRedirectsToIndex()
        {
            // Arrange
            var department = CreateEntityWithId<Department>(1);
            department.Name = "ToDelete";

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(department);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _repositoryMock.Verify(repo => repo.Remove(department), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(DepartmentController.Index), redirectResult.ActionName);
        }

        #endregion
    }
}