using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.Models.Identity;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels.Truck;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
// ضيف مسارات الـ Models والـ ViewModels والـ Interfaces هنا

namespace Rassef.Tests
{
    public class TruckControllerTests
    {
        private readonly Mock<ITruckRepository> _truckRepoMock;
        private readonly Mock<IRepository<TruckTypes>> _truckTypeRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<ISupplierRepository> _supplierRepoMock;
        private readonly TruckController _controller;

        public TruckControllerTests()
        {
            _truckRepoMock = new Mock<ITruckRepository>();
            _truckTypeRepoMock = new Mock<IRepository<TruckTypes>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _supplierRepoMock = new Mock<ISupplierRepository>();

            _controller = new TruckController(
                _truckRepoMock.Object, _truckTypeRepoMock.Object,
                _userRepoMock.Object, _supplierRepoMock.Object
            );

            // Setup TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
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

        private Truck GetFakeTruck(int id, int? supplierId = null)
        {
            var truckType = SetProperty(new TruckTypes(), "Name", "نوع تست");
            var user = SetProperty(new User(), "Name", "مستخدم تست");

            var truck = new Truck
            {
                PlateNumber = "123",
                PlateLetter = "أ ب",
                StorageCapacity = 10,
                IsRefrigerated = true,
                TruckType = truckType,
                CreatedBy = user,
                SupplierRequests = new List<SupplierRequest>()
            };

            if (supplierId.HasValue)
            {
                truck.SupplierRequests.Add(new SupplierRequest { SupplierId = supplierId.Value });
            }

            return SetId(truck, id);
        }
        #endregion

        #region Index Tests
        [Fact]
        public async Task Index_WorstCase_SupplierNotFound_RedirectsToSupplierIndex()
        {
            _supplierRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Supplier)null);

            var result = await _controller.Index(99);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Supplier", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_EdgeCase_NoTrucksForSupplier_ReturnsEmptyList()
        {
            var supplier = SetProperty(new Supplier(), "Name", "مورد تست");
            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

            var truckNotForSupplier = GetFakeTruck(1, 99); // شاحنة تابعة لمورد تاني
            _truckRepoMock.Setup(r => r.GetTruckWithTypeName())
                .ReturnsAsync(new List<Truck> { truckNotForSupplier });

            var result = await _controller.Index(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TruckListVM>>(viewResult.Model);
            Assert.Empty(model);
            Assert.Equal("مورد تست", _controller.ViewBag.SupplierName);
        }

        [Fact]
        public async Task Index_FriendlyCase_ReturnsData()
        {
            var supplier = SetProperty(new Supplier(), "Name", "مورد تست");
            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(supplier);

            var truck = GetFakeTruck(1, 1);
            _truckRepoMock.Setup(r => r.GetTruckWithTypeName())
                .ReturnsAsync(new List<Truck> { truck });

            var result = await _controller.Index(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TruckListVM>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(1, model[0].supplierId);
            Assert.Equal("نوع تست", model[0].TruckTypeName);
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
            _truckRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Truck)null);

            var result = await _controller.Details(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_FriendlyCase_ReturnsRequest()
        {
            var truck = GetFakeTruck(1);
            _truckRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(truck);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TruckDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("نوع تست", model.TruckTypeName);
        }
        #endregion

        #region Create GET Tests
        [Fact]
        public async Task CreateGet_FriendlyCase_LoadsTruckTypes_ReturnsView()
        {
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateTruckVM>(viewResult.Model);
            Assert.NotNull(viewResult.ViewData["TruckTypes"]);
        }
        #endregion

        #region Create POST Tests
        [Fact]
        public async Task CreatePost_WorstCase_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Error", "Sample Error");
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Create(new CreateTruckVM());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_WorstCase_UserNotLoggedIn_ReturnsError()
        {
            SetupUserClaims(null);
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Create(new CreateTruckVM());

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_WorstCase_UserNotFoundInDb_ReturnsError()
        {
            SetupUserClaims("1");
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Create(new CreateTruckVM());

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            SetupUserClaims("1");
            var user = SetId(new User(), 1);
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _controller.Create(new CreateTruckVM { PlateNumber = "123" });

            _truckRepoMock.Verify(r => r.AddAsync(It.IsAny<Truck>()), Times.Once);
            _truckRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
        #endregion

        #region Update GET Tests
        [Fact]
        public async Task UpdateGet_WorstCase_NullId_ReturnsError()
        {
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Update((int?)null);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdateGet_WorstCase_NotFound_ReturnsError()
        {
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());
            _truckRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Truck)null);

            var result = await _controller.Update(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdateGet_FriendlyCase_ReturnsPopulatedVM()
        {
            var truck = GetFakeTruck(1);
            _truckRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(truck);
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Update(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTruckVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.NotNull(viewResult.ViewData["TruckTypes"]);
        }
        #endregion

        #region Update POST Tests
        [Fact]
        public async Task UpdatePost_WorstCase_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Err", "Err");
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());

            var result = await _controller.Update(new UpdateTruckVM());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdatePost_WorstCase_NotFound_ReturnsError()
        {
            _truckTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TruckTypes>());
            _truckRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Truck)null);

            var result = await _controller.Update(new UpdateTruckVM { Id = 1 });

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdatePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var truck = GetFakeTruck(1);
            _truckRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(truck);

            var result = await _controller.Update(new UpdateTruckVM { Id = 1, PlateNumber = "999" });

            _truckRepoMock.Verify(r => r.Update(truck), Times.Once);
            _truckRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

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
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteGet_WorstCase_NotFound_ReturnsError()
        {
            _truckRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Truck)null);

            var result = await _controller.Delete(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteGet_FriendlyCase_ReturnsData()
        {
            var truck = GetFakeTruck(1);
            _truckRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(truck);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<TruckDetailsVM>(viewResult.Model);
        }
        #endregion

        #region Delete POST Tests
        [Fact]
        public async Task DeletePost_WorstCase_NotFound_ReturnsError()
        {
            _truckRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Truck)null);

            var result = await _controller.DeleteConfirmed(99);

            Assert.False(_controller.ModelState.IsValid);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeletePost_FriendlyCase_Success_RedirectsToIndex()
        {
            var truck = GetFakeTruck(1);
            _truckRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(truck);

            var result = await _controller.DeleteConfirmed(1);

            _truckRepoMock.Verify(r => r.Remove(truck), Times.Once);
            _truckRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        #endregion
    }
}