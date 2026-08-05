using Microsoft.AspNetCore.Mvc;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.TransferRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
// ضيف مسارات الـ Models والـ ViewModels والـ Interfaces هنا

namespace Rassef.Tests
{
    public class TransferRequestControllerTests
    {
        private readonly Mock<ITransferRequestRepository> _repoMock;
        private readonly Mock<ITruckRepository> _truckRepoMock;
        private readonly Mock<IDriverRepository> _driverRepoMock;
        private readonly Mock<IDepartmentRepository> _deptRepoMock;
        private readonly Mock<IPermitTypeRepository> _permitTypeRepoMock;
        private readonly Mock<IRequestStatusRepository> _reqStatusRepoMock;
        private readonly TransferRequestController _controller;

        public TransferRequestControllerTests()
        {
            _repoMock = new Mock<ITransferRequestRepository>();
            _truckRepoMock = new Mock<ITruckRepository>();
            _driverRepoMock = new Mock<IDriverRepository>();
            _deptRepoMock = new Mock<IDepartmentRepository>();
            _permitTypeRepoMock = new Mock<IPermitTypeRepository>();
            _reqStatusRepoMock = new Mock<IRequestStatusRepository>();

            _controller = new TransferRequestController(
                _repoMock.Object, _truckRepoMock.Object, _driverRepoMock.Object,
                _deptRepoMock.Object, _permitTypeRepoMock.Object, _reqStatusRepoMock.Object
            );
        }

        // دالة لضبط أي خاصية مقفولة بـ Reflection
        private T SetProperty<T>(T entity, string propertyName, object value)
        {
            entity.GetType().GetProperty(propertyName)?.SetValue(entity, value);
            return entity;
        }

        private T SetId<T>(T entity, int id)
        {
            return SetProperty(entity, "Id", id);
        }

        private void SetupLoadDataMocks()
        {
            _truckRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Truck>());
            _driverRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Driver>());
            _deptRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());
            _permitTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PermitTypes>());
            _reqStatusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RequestStatuses>());
        }

        private TransferRequest GetFakeTransferRequest(int id)
        {
            var permitType = SetProperty(new PermitTypes(), "Name", "تصريح تست");
            var requestStatus = SetProperty(new RequestStatuses(), "Name", "حالة تست");

            var request = new TransferRequest
            {
                AvizNumber = "12345",
                Truck = new Truck { PlateNumber = "123", PlateLetter = "أ" },
                Driver = new Driver { FullName = "سواق تست" },
                Department = new Department { Name = "قسم تست" },
                PermitType = permitType,
                RequestStatus = requestStatus
            };

            return SetId(request, id);
        }

        #region Index Tests
        [Fact]
        public async Task Index_FriendlyCase_ReturnsData()
        {
            var request = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<TransferRequest> { request });

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TransferRequestListVM>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Index_EdgeCase_EmptyData_ReturnsEmptyList()
        {
            _repoMock.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<TransferRequest>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TransferRequestListVM>>(viewResult.Model);
            Assert.Empty(model);
        }
        #endregion

        #region Details Tests
        [Fact]
        public async Task Details_WorstCase_NotFound_ReturnsError()
        {
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((TransferRequest)null);

            var result = await _controller.Details(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_FriendlyCase_ReturnsRequest()
        {
            var request = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(request);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TransferRequestDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }
        #endregion

        #region Create GET Tests
        [Fact]
        public async Task CreateGet_FriendlyCase_LoadsViewBags_ReturnsView()
        {
            SetupLoadDataMocks();

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["Trucks"]);
        }
        #endregion

        #region Create POST Tests
        [Fact]
        public async Task CreatePost_WorstCase_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Error", "Sample Error");
            SetupLoadDataMocks();

            var result = await _controller.Create(new CreateTransferRequestVM());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_WorstCase_AvizNumberExists_ReturnsError()
        {
            SetupLoadDataMocks();
            _repoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<TransferRequest, bool>>>()))
                     .ReturnsAsync(true);

            var result = await _controller.Create(new CreateTransferRequestVM { AvizNumber = "123" });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            _repoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<TransferRequest, bool>>>()))
                     .ReturnsAsync(false);

            var result = await _controller.Create(new CreateTransferRequestVM { AvizNumber = "123" });

            _repoMock.Verify(r => r.AddAsync(It.IsAny<TransferRequest>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        #endregion

        #region Update GET Tests
        [Fact]
        public async Task UpdateGet_WorstCase_NotFound_ReturnsError()
        {
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((TransferRequest)null);

            var result = await _controller.Update(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdateGet_FriendlyCase_ReturnsPopulatedVM()
        {
            SetupLoadDataMocks();
            var request = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(request);

            var result = await _controller.Update(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTransferRequestVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.NotNull(viewResult.ViewData["Trucks"]);
        }
        #endregion

        #region Update POST Tests
        [Fact]
        public async Task UpdatePost_WorstCase_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Err", "Err");
            SetupLoadDataMocks();

            var result = await _controller.Update(new UpdateTransferRequestVM());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdatePost_WorstCase_NotFound_ReturnsError()
        {
            SetupLoadDataMocks();
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TransferRequest)null);

            var result = await _controller.Update(new UpdateTransferRequestVM { Id = 1 });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdatePost_WorstCase_AvizNumberExistsForAnotherRecord_ReturnsError()
        {
            SetupLoadDataMocks();
            var request = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);

            _repoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<TransferRequest, bool>>>()))
                     .ReturnsAsync(true);

            var result = await _controller.Update(new UpdateTransferRequestVM { Id = 1, AvizNumber = "123" });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var req = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(req);
            _repoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<TransferRequest, bool>>>()))
                     .ReturnsAsync(false);

            var result = await _controller.Update(new UpdateTransferRequestVM { Id = 1, AvizNumber = "999" });

            _repoMock.Verify(r => r.Update(req), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion

        #region Delete GET Tests
        [Fact]
        public async Task DeleteGet_WorstCase_NotFound_ReturnsError()
        {
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((TransferRequest)null);

            var result = await _controller.Delete(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteGet_FriendlyCase_ReturnsData()
        {
            var req = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(req);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<TransferRequestDetailsVM>(viewResult.Model);
        }
        #endregion

        #region Delete POST Tests
        [Fact]
        public async Task DeletePost_WorstCase_NotFound_ReturnsError()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TransferRequest)null);

            var result = await _controller.DeleteConfirmed(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeletePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var req = GetFakeTransferRequest(1);
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(req);

            var result = await _controller.DeleteConfirmed(1);

            _repoMock.Verify(r => r.Remove(req), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion
    }
}