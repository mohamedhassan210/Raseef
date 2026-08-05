using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Controllers;
using Rassef.Models.Entities;
using Rassef.ViewModels.Supplier;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class SupplierControllerTests
    {
        private readonly Mock<ISupplierRepository> _supplierRepoMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly SupplierController _controller;

        public SupplierControllerTests()
        {
            _supplierRepoMock = new Mock<ISupplierRepository>();
            _fileServiceMock = new Mock<IFileService>();

            _controller = new SupplierController(
                _supplierRepoMock.Object,
                _fileServiceMock.Object
            );

            SetupHttpContextWithUser("5"); // الافتراضي وجود Claim بحساب ID = 5
        }

        private void SetupHttpContextWithUser(string userIdClaim = null)
        {
            var claims = new List<Claim>();
            if (userIdClaim != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdClaim));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

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
        public async Task Index_ReturnsViewWithSupplierListVMCollection()
        {
            // Arrange
            var s1 = SetPropertyValue(new Supplier
            {
                Name = "Supplier A",
                Phone = "123",
                LogoURL = "logo1.png",
                SupplierRequests = new List<SupplierRequest> { new SupplierRequest() }
            }, "Id", 1);

            var s2 = SetPropertyValue(new Supplier
            {
                Name = "Supplier B",
                Phone = "456",
                LogoURL = "logo2.png",
                SupplierRequests = new List<SupplierRequest>()
            }, "Id", 2);

            _supplierRepoMock.Setup(r => r.GetAllSuppliersWithRequestCountAsync())
                .ReturnsAsync(new List<Supplier> { s1, s2 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<SupplierListVM>>(viewResult.Model);
            var list = model.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].RequestsCount);
            Assert.Equal(0, list[1].RequestsCount);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_SupplierNotFound_ReturnsViewWithNullAndModelError()
        {
            // Arrange
            _supplierRepoMock.Setup(r => r.GetSupplierWithDetailsAsync(1)).ReturnsAsync((Supplier)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Details_SupplierFound_FullDetailsMappedCorrectly()
        {
            // Arrange
            var createdDate = DateTimeOffset.Now;
            var supplier = SetPropertyValue(new Supplier
            {
                Name = "Tech Corp",
                Phone = "0100000008",
                LogoURL = "uploads/tech.png",
                // تمت إزالة CreatedAT من هنا
                CreatedBy = new ApplicationUser { UserName = "admin_user" },
                SupplierRequests = new List<SupplierRequest> { new SupplierRequest(), new SupplierRequest() }
            }, "Id", 10);

            // تعيين القيمة باستخدام Reflection لتجاوز الـ private/protected set
            SetPropertyValue(supplier, "CreatedAT", createdDate);
            // Act
            var result = await _controller.Details(10);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplierDetailsVM>(viewResult.Model);

            Assert.Equal(10, model.Id);
            Assert.Equal("Tech Corp", model.Name);
            Assert.Equal("admin_user", model.CreatedByUserName);
            Assert.Equal(createdDate.LocalDateTime, model.CreatedAt);
            Assert.Equal(2, model.TotalRequestsCount);
        }

        [Fact]
        public async Task Details_SupplierFound_NullCreatedBy_ReturnsDefaultUserName()
        {
            // Arrange
            var supplier = SetPropertyValue(new Supplier
            {
                Name = "Alpha Corp",
                CreatedBy = null,
                SupplierRequests = new List<SupplierRequest>()
            }, "Id", 1);

            _supplierRepoMock.Setup(r => r.GetSupplierWithDetailsAsync(1)).ReturnsAsync(supplier);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SupplierDetailsVM>(viewResult.Model);

            Assert.Equal("غير محدد", model.CreatedByUserName);
        }

        #endregion

        #region Create Tests

        [Fact]
        public void Create_Get_ReturnsViewWithCreateSupplierVM()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateSupplierVM>(viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "اسم المورد مطلوب");
            var model = new CreateSupplierVM();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            _supplierRepoMock.Verify(r => r.AddAsync(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_WithLogoFileAndValidUserClaim_UploadsFileAndSaves()
        {
            // Arrange
            SetupHttpContextWithUser("15");
            var mockFile = new Mock<IFormFile>();
            _fileServiceMock.Setup(f => f.UploadImageAsync(mockFile.Object)).ReturnsAsync("uploads/new_logo.png");

            var model = new CreateSupplierVM
            {
                Name = "New Supplier",
                Phone = "12345",
                SupCode = "SUP-100",
                LogoFile = mockFile.Object
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            _fileServiceMock.Verify(f => f.UploadImageAsync(mockFile.Object), Times.Once);
            _supplierRepoMock.Verify(r => r.AddAsync(It.Is<Supplier>(s =>
                s.Name == "New Supplier" &&
                s.LogoURL == "uploads/new_logo.png" &&
                s.CreatedById == 15 &&
                s.SupCode == "SUP-100"
            )), Times.Once);

            _supplierRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم إضافة المورد بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_WithoutLogoFileAndNoUserClaim_DefaultsCreatedByIdTo1()
        {
            // Arrange
            SetupHttpContextWithUser(userIdClaim: null);

            var model = new CreateSupplierVM
            {
                Name = "No Logo Supplier",
                Phone = "000",
                SupCode = "SUP-200",
                LogoFile = null
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            _fileServiceMock.Verify(f => f.UploadImageAsync(It.IsAny<IFormFile>()), Times.Never);
            _supplierRepoMock.Verify(r => r.AddAsync(It.Is<Supplier>(s =>
                s.LogoURL == string.Empty &&
                s.CreatedById == 1
            )), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ExceptionThrown_ReturnsContentResultWithErrorMessage()
        {
            // Arrange
            var model = new CreateSupplierVM { Name = "Fail Supplier" };
            var innerEx = new Exception("Database Constraint Error");
            var ex = new Exception("Outer Exception", innerEx);

            _supplierRepoMock.Setup(r => r.AddAsync(It.IsAny<Supplier>())).ThrowsAsync(ex);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("السبب الحقيقي للخطأ: Database Constraint Error", contentResult.Content);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_SupplierNotFound_ReturnsViewWithNullAndModelError()
        {
            // Arrange
            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Supplier)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_Get_SupplierFound_ReturnsViewWithUpdateSupplierVM()
        {
            // Arrange
            var supplier = SetPropertyValue(new Supplier { Name = "Existing", Phone = "111", LogoURL = "old.png" }, "Id", 5);
            _supplierRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(supplier);

            // Act
            var result = await _controller.Edit(5);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateSupplierVM>(viewResult.Model);
            Assert.Equal(5, model.Id);
            Assert.Equal("Existing", model.Name);
            Assert.Equal("old.png", model.ExistingLogoURL);
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "مطلوب");
            var model = new UpdateSupplierVM { Id = 1 };

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            _supplierRepoMock.Verify(r => r.Update(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_SupplierNotFoundInDb_ReturnsViewWithNullAndModelError()
        {
            // Arrange
            var model = new UpdateSupplierVM { Id = 99, Name = "Not Found" };
            _supplierRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Supplier)null);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _supplierRepoMock.Verify(r => r.Update(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ValidUpdateWithNewLogo_DeletesOldLogoUploadsNewAndRedirects()
        {
            // Arrange
            var existingSupplier = SetPropertyValue(new Supplier
            {
                Name = "Old Name",
                Phone = "111",
                LogoURL = "uploads/old_logo.png"
            }, "Id", 10);

            _supplierRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existingSupplier);

            var newLogoMock = new Mock<IFormFile>();
            _fileServiceMock.Setup(f => f.UploadImageAsync(newLogoMock.Object)).ReturnsAsync("uploads/new_logo.png");

            var updateVM = new UpdateSupplierVM
            {
                Id = 10,
                Name = "Updated Name",
                Phone = "999",
                LogoFile = newLogoMock.Object
            };

            // Act
            var result = await _controller.Edit(updateVM);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteImage("uploads/old_logo.png"), Times.Once);
            _fileServiceMock.Verify(f => f.UploadImageAsync(newLogoMock.Object), Times.Once);

            _supplierRepoMock.Verify(r => r.Update(It.Is<Supplier>(s =>
                s.Name == "Updated Name" &&
                s.Phone == "999" &&
                s.LogoURL == "uploads/new_logo.png"
            )), Times.Once);

            _supplierRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal("تم تعديل بيانات المورد بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_Post_ValidUpdateWithoutNewLogo_KeepsExistingLogo()
        {
            // Arrange
            var existingSupplier = SetPropertyValue(new Supplier
            {
                Name = "Old Name",
                Phone = "111",
                LogoURL = "uploads/old_logo.png"
            }, "Id", 10);

            _supplierRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existingSupplier);

            var updateVM = new UpdateSupplierVM
            {
                Id = 10,
                Name = "Updated Name",
                Phone = "999",
                LogoFile = null
            };

            // Act
            var result = await _controller.Edit(updateVM);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteImage(It.IsAny<string>()), Times.Never);
            _fileServiceMock.Verify(f => f.UploadImageAsync(It.IsAny<IFormFile>()), Times.Never);

            Assert.Equal("uploads/old_logo.png", existingSupplier.LogoURL);
            _supplierRepoMock.Verify(r => r.Update(existingSupplier), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Post_SupplierNotFound_ReturnsViewWithNullAndModelError()
        {
            // Arrange
            _supplierRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Supplier)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _supplierRepoMock.Verify(r => r.Remove(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task Delete_Post_SupplierFound_DeletesLogoRemovesSupplierAndRedirects()
        {
            // Arrange
            var supplier = SetPropertyValue(new Supplier
            {
                Name = "To Be Deleted",
                LogoURL = "uploads/to_delete.png"
            }, "Id", 7);

            _supplierRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(supplier);

            // Act
            var result = await _controller.Delete(7);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteImage("uploads/to_delete.png"), Times.Once);
            _supplierRepoMock.Verify(r => r.Remove(supplier), Times.Once);
            _supplierRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            Assert.Equal("تم حذف المورد بنجاح!", _controller.TempData["SuccessMessage"]);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion
    }
}