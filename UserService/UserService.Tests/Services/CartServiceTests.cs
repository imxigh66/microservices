using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;
using Xunit;
using FluentAssertions;

namespace UserService.Tests.Services
{
	public class CartServiceTests : IDisposable
	{
		private readonly UserDbContext _context;
		private readonly Mock<ICatalogClient> _catalogClientMock;
		private readonly CartService _cartService;

		public CartServiceTests()
		{
			var options = new DbContextOptionsBuilder<UserDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new UserDbContext(options);
			_catalogClientMock = new Mock<ICatalogClient>();

			var logger = new LoggerFactory().CreateLogger<CartService>();

			_cartService = new CartService(
				_context,
				logger,
				_catalogClientMock.Object
			);
		}

		[Fact]
		public async Task GetCartAsync_ShouldReturnEmptyCart_WhenNoItems()
		{
			// Arrange
			var userId = "user-123";

			// Act
			var result = await _cartService.GetCartAsync(userId);

			// Assert
			result.Should().NotBeNull();
			result.TotalItems.Should().Be(0);
			result.TotalPrice.Should().Be(0);
			result.Items.Should().BeEmpty();
		}

		[Fact]
		public async Task AddToCartAsync_ShouldAddNewItem_WhenProductExists()
		{
			// Arrange
			var userId = "user-123";
			var request = new AddToCartRequest
			{
				ProductId = 100,
				Quantity = 2,
				Size = "M",
				Price = 50.00m
			};

			// ИСПРАВЛЕНИЕ: добавляем It.IsAny<CancellationToken>()
			_catalogClientMock
				.Setup(x => x.GetProductPreviewAsync(100, It.IsAny<CancellationToken>()))
				.ReturnsAsync((true, "T-Shirt", 50.00m, "shirt.jpg"));

			// Act
			var response = await _cartService.AddToCartAsync(userId, request);

			// Assert
			response.Should().NotBeNull();
			response!.ProductId.Should().Be(100);
			response.Quantity.Should().Be(2);
			response.Price.Should().Be(50.00m);
			response.TotalPrice.Should().Be(100.00m);
		}

		[Fact]
		public async Task AddToCartAsync_ShouldReturnNull_WhenProductDoesNotExist()
		{
			// Arrange
			var userId = "user-123";
			var request = new AddToCartRequest
			{
				ProductId = 999,
				Quantity = 1,
				Size = "L",
				Price = 100.00m
			};

			_catalogClientMock
				.Setup(x => x.GetProductPreviewAsync(999, It.IsAny<CancellationToken>()))
				.ReturnsAsync((false, (string?)null, (decimal?)null, (string?)null));

			// Act
			var response = await _cartService.AddToCartAsync(userId, request);

			// Assert
			response.Should().BeNull();
		}

		[Fact]
		public async Task RemoveFromCartAsync_ShouldReturnTrue_WhenItemExists()
		{
			// Arrange
			var userId = "user-123";
			var item = new CartItem
			{
				Id = 10,
				UserId = userId,
				ProductId = 300,
				Quantity = 1,
				Size = "M",
				Price = 30.00m,
				AddedAt = DateTime.UtcNow
			};

			_context.CartItems.Add(item);
			await _context.SaveChangesAsync();

			// Act
			var result = await _cartService.RemoveFromCartAsync(userId, 10);

			// Assert
			result.Should().BeTrue();

			var itemInDb = await _context.CartItems.FindAsync(10);
			itemInDb.Should().BeNull();
		}

		[Fact]
		public async Task ClearCartAsync_ShouldRemoveAllItems_AndReturnCount()
		{
			// Arrange
			var userId = "user-123";

			_context.CartItems.AddRange(
				new CartItem { UserId = userId, ProductId = 1, Quantity = 1, Size = "S", Price = 10, AddedAt = DateTime.UtcNow },
				new CartItem { UserId = userId, ProductId = 2, Quantity = 2, Size = "M", Price = 20, AddedAt = DateTime.UtcNow },
				new CartItem { UserId = userId, ProductId = 3, Quantity = 1, Size = "L", Price = 30, AddedAt = DateTime.UtcNow }
			);
			await _context.SaveChangesAsync();

			// Act
			var result = await _cartService.ClearCartAsync(userId);

			// Assert
			result.Should().Be(3);

			var remainingItems = await _context.CartItems.CountAsync(x => x.UserId == userId);
			remainingItems.Should().Be(0);
		}

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}