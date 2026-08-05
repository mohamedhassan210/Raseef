using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Rassef.Common.Interfaces;
using Rassef.Common.Interfaces.Services;
using Rassef.Controllers;
using Rassef.Models;
using Rassef.Models.Entities;
using Rassef.Models.StatusesAndActions;
using Rassef.ViewModels;
using Rassef.ViewModels.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Rassef.Test.Controllers
{
    public class CheckOutControllerTests
    {
        private readonly Mock<ICheckOutRepository> _checkOutRepoMock;
        private readonly Mock<IRepository<QueueTicket>> _ticketRepoMock;
        private readonly Mock<IRepository<ExitTypes>> _exitTypeRepoMock;
        private readonly CheckOutController _controller;

        public CheckOutControllerTests()
        {
            _checkOutRepoMock = new Mock<ICheckOutRepository>();
            _ticketRepoMock = new Mock<IRepository<QueueTicket>>();
            _exitTypeRepoMock = new Mock<IRepository<ExitTypes>>();

            _controller = new CheckOutController(
                _checkOutRepoMock.Object,
                _ticketRepoMock.Object,
                _exitTypeRepoMock.Object
            );
        }

        #region Helper Methods

        private static ExitTypes CreateExitTypeWithName(string name)
        {
            var exitType = new ExitTypes();
            var property = typeof(ExitTypes).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            property?.SetValue(exitType, name);
            return exitType;
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithMappedViewModels()
        {
            // Arrange
            var checkOuts = new List<CheckOut>
            {
                new CheckOut
                {
                    QueueTicket = new QueueTicket { TicketNumber = "101" },
                    ExitType = CreateExitTypeWithName("عادي"),
                    ExitTime = DateTime.Now
                },
                new CheckOut
                {
                    QueueTicket = new QueueTicket { TicketNumber = "102" },
                    ExitType = null,
                    ExitTime = DateTime.Now
                }
            };

            _checkOutRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(checkOuts);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CheckOutListVM>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal("غير محدد", model.Last().ExitTypeName);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_ExistingId_ReturnsViewWithModel()
        {
            // Arrange
            var checkOut = new CheckOut
            {
                QueueTicket = new QueueTicket { TicketNumber = "101" },
                ExitType = CreateExitTypeWithName("طوارئ"),
                ExitTime = DateTime.Now
            };

            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(checkOut);

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CheckOutDetailsVM>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("101", model.TicketNumber);
            Assert.Equal("النظام", model.CreatedBy);
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsViewWithModelStateError()
        {
            // Arrange
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((CheckOut)null);

            // Act
            var result = await _controller.Details(99);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("تسجيل الخروج"));
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_Get_ReturnsViewWithPopulatedDropdowns()
        {
            // Arrange
            _ticketRepoMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<QueueTicket> { new QueueTicket { TicketNumber = "100" } });
            _exitTypeRepoMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<ExitTypes> { CreateExitTypeWithName("خروج نهائي") });

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateCheckOutVM>(viewResult.Model);
            Assert.Single(model.Tickets);
            Assert.Single(model.ExitTypes);
        }

        [Fact]
        public async Task Create_Post_ValidModel_AddsAndRedirectsToIndex()
        {
            // Arrange
            var createVm = new CreateCheckOutVM
            {
                TicketId = 1,
                ExitTypeId = 2,
                ExitTime = DateTime.Now
            };

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            _checkOutRepoMock.Verify(repo => repo.AddAsync(It.IsAny<CheckOut>()), Times.Once);
            _checkOutRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithDropdowns()
        {
            // Arrange
            _controller.ModelState.AddModelError("TicketId", "Required");
            _ticketRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _exitTypeRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<ExitTypes>());

            var createVm = new CreateCheckOutVM();

            // Act
            var result = await _controller.Create(createVm);

            // Assert
            _checkOutRepoMock.Verify(repo => repo.AddAsync(It.IsAny<CheckOut>()), Times.Never);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_Get_ExistingId_ReturnsViewWithPopulatedModel()
        {
            // Arrange
            var checkOut = new CheckOut { TicketId = 10, ExitTypeId = 2, ExitTime = DateTime.Now };
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(checkOut);
            _ticketRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _exitTypeRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<ExitTypes>());

            // Act
            var result = await _controller.Update(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateCheckOutVM>(viewResult.Model);
            Assert.NotNull(model.Tickets);
        }

        [Fact]
        public async Task Update_Get_NonExistingId_ReturnsViewWithModelStateError()
        {
            // Arrange
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((CheckOut)null);

            // Act
            var result = await _controller.Update(99);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_Post_ValidModel_UpdatesAndRedirectsToIndex()
        {
            // Arrange
            var updateVm = new UpdateCheckOutVM { Id = 1, TicketId = 5, ExitTypeId = 3, ExitTime = DateTime.Now };
            var existingCheckOut = new CheckOut();

            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existingCheckOut);

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            _checkOutRepoMock.Verify(repo => repo.Update(It.Is<CheckOut>(c => c.TicketId == 5)), Times.Once);
            _checkOutRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Update_Post_InvalidModel_ReturnsViewWithDropdowns()
        {
            // Arrange
            _controller.ModelState.AddModelError("TicketId", "Required");
            _ticketRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<QueueTicket>());
            _exitTypeRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<ExitTypes>());

            var updateVm = new UpdateCheckOutVM { Id = 1 };

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            _checkOutRepoMock.Verify(repo => repo.Update(It.IsAny<CheckOut>()), Times.Never);
            _checkOutRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Update_Post_NotFoundCheckOut_ReturnsViewWithModelStateError()
        {
            // Arrange
            var updateVm = new UpdateCheckOutVM { Id = 99 };
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((CheckOut)null);

            // Act
            var result = await _controller.Update(updateVm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _checkOutRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Get_ExistingId_ReturnsView()
        {
            // Arrange
            var checkOut = new CheckOut { QueueTicket = new QueueTicket { TicketNumber = "505" } };
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(checkOut);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CheckOutDetailsVM>(viewResult.Model);
            Assert.Equal("505", model.TicketNumber);
        }

        [Fact]
        public async Task Delete_Get_NonExistingId_ReturnsViewWithModelStateError()
        {
            // Arrange
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((CheckOut)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingId_RemovesAndRedirects()
        {
            // Arrange
            var checkOut = new CheckOut();
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(checkOut);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            _checkOutRepoMock.Verify(repo => repo.Remove(checkOut), Times.Once);
            _checkOutRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_NonExistingId_ReturnsViewWithModelStateError()
        {
            // Arrange
            _checkOutRepoMock.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((CheckOut)null);

            // Act
            var result = await _controller.DeleteConfirmed(99);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _checkOutRepoMock.Verify(repo => repo.Remove(It.IsAny<CheckOut>()), Times.Never);
        }

        #endregion
    }
}