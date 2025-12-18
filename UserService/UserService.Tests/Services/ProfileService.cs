using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;
using Xunit;
using FluentAssertions;

namespace UserService.Tests.Services
{
	public class ProfileServiceTests : IDisposable
	{
		private readonly UserDbContext _context;
		private readonly ProfileService _profileService;

		public ProfileServiceTests()
		{
			var options = new DbContextOptionsBuilder<UserDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new UserDbContext(options);
			_profileService = new ProfileService(_context);
		}

		[Fact]
		public async Task GetProfileAsync_ShouldReturnProfile_WhenUserExists()
		{
			// Arrange
			var userId = "user-123";
			var profile = new UserProfile
			{
				UserId = userId,
				Email = "test@example.com",
				FirstName = "John",
				LastName = "Doe",
				PhoneNumber = "+1234567890",
				Country = "USA",
				City = "New York",
				CreatedAt = DateTime.UtcNow
			};

			_context.UserProfiles.Add(profile);
			await _context.SaveChangesAsync();

			// Act
			var result = await _profileService.GetProfileAsync(userId);

			// Assert
			result.Should().NotBeNull();
			result!.UserId.Should().Be(userId);
			result.Email.Should().Be("test@example.com");
			result.FirstName.Should().Be("John");
			result.LastName.Should().Be("Doe");
		}

		[Fact]
		public async Task GetProfileAsync_ShouldReturnNull_WhenUserDoesNotExist()
		{
			// Arrange
			var userId = "non-existent";

			// Act
			var result = await _profileService.GetProfileAsync(userId);

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task UpdateProfileAsync_ShouldUpdateFields_WhenProfileExists()
		{
			// Arrange
			var userId = "user-123";
			var profile = new UserProfile
			{
				UserId = userId,
				Email = "old@example.com",
				FirstName = "OldName",
				LastName = "OldLastName",
				PhoneNumber = "+0000000000",
				CreatedAt = DateTime.UtcNow
			};

			_context.UserProfiles.Add(profile);
			await _context.SaveChangesAsync();

			var updateRequest = new UpdateProfileRequest
			{
				FirstName = "NewName",
				LastName = "NewLastName",
				PhoneNumber = "+1111111111"
			};

			// Act
			var result = await _profileService.UpdateProfileAsync(userId, updateRequest);

			// Assert
			result.Should().NotBeNull();
			result!.FirstName.Should().Be("NewName");
			result.LastName.Should().Be("NewLastName");
			result.PhoneNumber.Should().Be("+1111111111");
			result.Email.Should().Be("old@example.com"); // Email не изменился
		}

		[Fact]
		public async Task UpdateProfileAsync_ShouldKeepOldValues_WhenRequestFieldsAreNull()
		{
			// Arrange
			var userId = "user-123";
			var profile = new UserProfile
			{
				UserId = userId,
				Email = "test@example.com",
				FirstName = "John",
				LastName = "Doe",
				PhoneNumber = "+1234567890",
				CreatedAt = DateTime.UtcNow
			};

			_context.UserProfiles.Add(profile);
			await _context.SaveChangesAsync();

			var updateRequest = new UpdateProfileRequest
			{
				FirstName = null, // Не обновляем
				LastName = "UpdatedLastName",
				PhoneNumber = null // Не обновляем
			};

			// Act
			var result = await _profileService.UpdateProfileAsync(userId, updateRequest);

			// Assert
			result.Should().NotBeNull();
			result!.FirstName.Should().Be("John"); // Осталось старое значение
			result.LastName.Should().Be("UpdatedLastName");
			result.PhoneNumber.Should().Be("+1234567890"); // Осталось старое значение
		}

		[Fact]
		public async Task UpdateProfileAsync_ShouldReturnNull_WhenProfileDoesNotExist()
		{
			// Arrange
			var userId = "non-existent";
			var updateRequest = new UpdateProfileRequest
			{
				FirstName = "Test",
				LastName = "User"
			};

			// Act
			var result = await _profileService.UpdateProfileAsync(userId, updateRequest);

			// Assert
			result.Should().BeNull();
		}

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}