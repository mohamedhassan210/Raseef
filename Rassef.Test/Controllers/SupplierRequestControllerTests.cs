using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.Models.StatusesAndActions;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
// ضيف مسارات الموديلز بتاعتك هنا

namespace Rassef.Tests
{
    public class SupplierRequestControllerTests
    {
        private readonly Mock<ISupplierRequestRepository> _supplierReqRepoMock;
        private readonly Mock<IRepository<Supplier>> _supplierRepoMock;
        private readonly Mock<IRepository<Truck>> _truckRepoMock;
        private readonly Mock<IRepository<Driver>> _driverRepoMock;
        private readonly Mock<IRepository<Department>> _deptRepoMock;
        private readonly Mock<IRepository<PermitTypes>> _permitTypeRepoMock;
        private readonly Mock<IRepository<CommodityTypes>> _commodityTypeRepoMock;
        private readonly Mock<IRepository<RequestStatuses>> _reqStatusRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly SupplierRequestController _controller;

        public SupplierRequestControllerTests()
        {
            _supplierReqRepoMock = new Mock<ISupplierRequestRepository>();
            _supplierRepoMock = new Mock<IRepository<Supplier>>();
            _truckRepoMock = new Mock<IRepository<Truck>>();
            _driverRepoMock = new Mock<IRepository<Driver>>();
            _deptRepoMock = new Mock<IRepository<Department>>();
            _permitTypeRepoMock = new Mock<IRepository<PermitTypes>>();
            _commodityTypeRepoMock = new Mock<IRepository<CommodityTypes>>();
            _reqStatusRepoMock = new Mock<IRepository<RequestStatuses>>();
            _userRepoMock = new Mock<IRepository<User>>();

            _controller = new SupplierRequestController(
                _supplierReqRepoMock.Object, _supplierRepoMock.Object, _truckRepoMock.Object,
                _driverRepoMock.Object, _deptRepoMock.Object, _permitTypeRepoMock.Object,
                _commodityTypeRepoMock.Object, _reqStatusRepoMock.Object, _userRepoMock.Object
            );

            // Setup TempData for redirects
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        // دالة مساعدة عشان ندي قيمة للـ Id بالـ Reflection من غير ما نعدل الـ Model
        private T SetId<T>(T entity, int id)
        {
            entity.GetType().GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }

        private void SetupUserClaims(string userId)
        {
            var claims = new List<Claim>();
            if (userId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private void SetupDropdownsMocks()
        {
            _supplierRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>());
            _truckRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Truck>());
            _driverRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Driver>());
            _deptRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());
            _permitTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PermitTypes>());
            _commodityTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CommodityTypes>());
            _reqStatusRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RequestStatuses>());
        }

        #region Index Tests
        [Fact]
        public async Task Index_FriendlyCase_ReturnsData()
        {
            var request = SetId(new SupplierRequest(), 1);
            _supplierReqRepoMock.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<SupplierRequest> { request });

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<SupplierRequestListVM>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Index_EdgeCase_EmptyData_ReturnsEmptyList()
        {
            _supplierReqRepoMock.Setup(r => r.GetAllWithDetailsAsync())
                .ReturnsAsync(new List<SupplierRequest>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<SupplierRequestListVM>>(viewResult.Model);
            Assert.Empty(model);
        }
        #endregion

        #region Details Tests
        [Fact]
        public async Task Details_WorstCase_NullId_ReturnsError()
        {
            var result = await _controller.Details((int?)null);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_WorstCase_NotFound_ReturnsError()
        {
            _supplierReqRepoMock.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((SupplierRequest)null);

            var result = await _controller.Details(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_FriendlyCase_ReturnsRequest()
        {
            var request = SetId(new SupplierRequest(), 1);
            _supplierReqRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(request);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplierRequestDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }
        #endregion

        #region Create GET Tests
        [Fact]
        public async Task CreateGet_FriendlyCase_PopulatesDropdowns()
        {
            SetupDropdownsMocks();

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateSupplierRequestVM>(viewResult.Model);
        }
        #endregion

        #region Create POST Tests
        [Fact]
        public async Task CreatePost_WorstCase_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Error", "Sample Error");
            SetupDropdownsMocks();

            var result = await _controller.Create(new CreateSupplierRequestVM());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_WorstCase_UserNotLoggedIn_ReturnsError()
        {
            SetupUserClaims(null);
            SetupDropdownsMocks();

            var result = await _controller.Create(new CreateSupplierRequestVM());

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_WorstCase_UserNotFoundInDb_ReturnsError()
        {
            SetupUserClaims("1");
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);
            SetupDropdownsMocks();

            var result = await _controller.Create(new CreateSupplierRequestVM());

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            SetupUserClaims("1");
            var user = SetId(new User(), 1);
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _controller.Create(new CreateSupplierRequestVM());

            _supplierReqRepoMock.Verify(r => r.AddAsync(It.IsAny<SupplierRequest>()), Times.Once);
            _supplierReqRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.NotNull(_controller.TempData["Success"]);
        }
        #endregion

        #region Update GET Tests
        [Fact]
        public async Task UpdateGet_WorstCase_NullId_ReturnsError()
        {
            SetupDropdownsMocks();
            var result = await _controller.Update((int?)null);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task UpdateGet_WorstCase_NotFound_ReturnsError()
        {
            SetupDropdownsMocks();
            _supplierReqRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SupplierRequest)null);

            var result = await _controller.Update(99);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task UpdateGet_FriendlyCase_ReturnsPopulatedVM()
        {
            SetupDropdownsMocks();
            var request = SetId(new SupplierRequest(), 1);
            _supplierReqRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(request);

            var result = await _controller.Update(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateSupplierRequestVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }
        #endregion

        #region Update POST Tests
        [Fact]
        public async Task UpdatePost_WorstCase_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Err", "Err");
            SetupDropdownsMocks();

            var result = await _controller.Update(new UpdateSupplierRequestVM());
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdatePost_WorstCase_NotFound_ReturnsError()
        {
            SetupDropdownsMocks();
            _supplierReqRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((SupplierRequest)null);

            var result = await _controller.Update(new UpdateSupplierRequestVM { Id = 1 });

            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task UpdatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var req = SetId(new SupplierRequest(), 1);
            _supplierReqRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(req);

            var result = await _controller.Update(new UpdateSupplierRequestVM { Id = 1 });

            _supplierReqRepoMock.Verify(r => r.Update(req), Times.Once);
            _supplierReqRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion

        #region Delete GET Tests
        [Fact]
        public async Task DeleteGet_WorstCase_NullId_ReturnsError()
        {
            var result = await _controller.Delete((int?)null);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DeleteGet_WorstCase_NotFound_ReturnsError()
        {
            _supplierReqRepoMock.Setup(r => r.GetByIdWithDetailsAsync(99)).ReturnsAsync((SupplierRequest)null);
            var result = await _controller.Delete(99);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DeleteGet_FriendlyCase_ReturnsData()
        {
            var req = SetId(new SupplierRequest(), 1);
            _supplierReqRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(req);
            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<SupplierRequestDetailsVM>(viewResult.Model);
        }
        #endregion

        #region Delete POST Tests
        [Fact]
        public async Task DeletePost_WorstCase_NotFound_ReturnsError()
        {
            _supplierReqRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SupplierRequest)null);
            var result = await _controller.DeleteConfirmed(99);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DeletePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var req = SetId(new SupplierRequest(), 1);
            _supplierReqRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(req);

            var result = await _controller.DeleteConfirmed(1);

            _supplierReqRepoMock.Verify(r => r.Remove(req), Times.Once);
            _supplierReqRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion
    }
}