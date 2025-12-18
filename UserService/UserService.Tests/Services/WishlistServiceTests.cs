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
	public class WishlistServiceTests : IDisposable
	{
		private readonly UserDbContext _context;
		private readonly Mock<ICatalogClient> _catalogClientMock;
		private readonly WishlistService _wishlistService;

		public WishlistServiceTests()
		{
			var options = new DbContextOptionsBuilder<UserDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new UserDbContext(options);
			_catalogClientMock = new Mock<ICatalogClient>();

			var logger = new LoggerFactory().CreateLogger<WishlistService>();

			_wishlistService = new WishlistService(
				_context,
				logger,
				_catalogClientMock.Object
			);
		}

		[Fact]
		public async Task GetWishlistAsync_ShouldReturnEmptyList_WhenNoItems()
		{
			// Arrange
			var userId = "user-123";

			// Act
			var result = await _wishlistService.GetWishlistAsync(userId);

			// Assert
			result.Should().BeEmpty();
		}

		[Fact]
		public async Task AddToWishlistAsync_ShouldAddNewItem_WhenProductExists()
		{
			// Arrange
			var userId = "user-123";
			var request = new AddToWishlistRequest { ProductId = 200 };

			_catalogClientMock
				.Setup(x => x.GetProductPreviewAsync(200, It.IsAny<CancellationToken>()))
				.ReturnsAsync((true, "New Product", 149.99m, "new.jpg"));

			// Act
			var response = await _wishlistService.AddToWishlistAsync(userId, request);

			// Assert
			response.Should().NotBeNull();
			response!.ProductId.Should().Be(200);
			response.Name.Should().Be("New Product");
			response.Price.Should().Be(149.99m);

			var itemInDb = await _context.WishlistItems
				.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == 200);
			itemInDb.Should().NotBeNull();
		}

		[Fact]
		public async Task AddToWishlistAsync_ShouldReturnNull_WhenProductDoesNotExist()
		{
			// Arrange
			var userId = "user-123";
			var request = new AddToWishlistRequest { ProductId = 999 };

			_catalogClientMock
				.Setup(x => x.GetProductPreviewAsync(999, It.IsAny<CancellationToken>()))
				.ReturnsAsync((false, (string?)null, (decimal?)null, (string?)null));

			// Act
			var response = await _wishlistService.AddToWishlistAsync(userId, request);

			// Assert
			response.Should().BeNull();
		}

		[Fact]
		public async Task RemoveFromWishlistAsync_ShouldReturnTrue_WhenItemExists()
		{
			// Arrange
			var userId = "user-123";
			var item = new WishlistItem
			{
				Id = 20,
				UserId = userId,
				ProductId = 400,
				AddedAt = DateTime.UtcNow
			};

			_context.WishlistItems.Add(item);
			await _context.SaveChangesAsync();

			// Act
			var result = await _wishlistService.RemoveFromWishlistAsync(userId, 20);

			// Assert
			result.Should().BeTrue();

			var itemInDb = await _context.WishlistItems.FindAsync(20);
			itemInDb.Should().BeNull();
		}

		[Fact]
		public async Task IsInWishlistAsync_ShouldReturnTrue_WhenProductInWishlist()
		{
			// Arrange
			var userId = "user-123";
			var item = new WishlistItem
			{
				UserId = userId,
				ProductId = 500,
				AddedAt = DateTime.UtcNow
			};

			_context.WishlistItems.Add(item);
			await _context.SaveChangesAsync();

			// Act
			var result = await _wishlistService.IsInWishlistAsync(userId, 500);

			// Assert
			result.Should().BeTrue();
		}

		[Fact]
		public async Task ClearWishlistAsync_ShouldRemoveAllItems_AndReturnCount()
		{
			// Arrange
			var userId = "user-123";

			_context.WishlistItems.AddRange(
				new WishlistItem { UserId = userId, ProductId = 1, AddedAt = DateTime.UtcNow },
				new WishlistItem { UserId = userId, ProductId = 2, AddedAt = DateTime.UtcNow },
				new WishlistItem { UserId = userId, ProductId = 3, AddedAt = DateTime.UtcNow }
			);
			await _context.SaveChangesAsync();

			// Act
			var result = await _wishlistService.ClearWishlistAsync(userId);

			// Assert
			result.Should().Be(3);

			var remainingItems = await _context.WishlistItems.CountAsync(x => x.UserId == userId);
			remainingItems.Should().Be(0);
		}

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}