using IdentityService.DTO;
using IdentityService.Events;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;

namespace IdentityService.Tests.Services
{
	public class AuthServiceTests
	{
		private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
		private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
		private readonly Mock<ITokenService> _tokenServiceMock;
		private readonly Mock<IEventPublisher> _eventPublisherMock;
		private readonly Mock<IConfiguration> _configurationMock;
		private readonly Mock<ITwoFactorService> _twoFactorServiceMock;
		private readonly Mock<IMemoryCache> _cacheMock;
		private readonly AuthService _authService;

		public AuthServiceTests()
		{
			// Настройка моков
			_userManagerMock = MockUserManager<ApplicationUser>();
			_signInManagerMock = MockSignInManager();
			_tokenServiceMock = new Mock<ITokenService>();
			_eventPublisherMock = new Mock<IEventPublisher>();
			_configurationMock = new Mock<IConfiguration>();
			_twoFactorServiceMock = new Mock<ITwoFactorService>();
			_cacheMock = new Mock<IMemoryCache>();

			// Настройка конфигурации
			_configurationMock.Setup(c => c["Kafka:Topics:UserRegistered"])
				.Returns("user-registered-topic");

			// Создание сервиса
			_authService = new AuthService(
				_userManagerMock.Object,
				_signInManagerMock.Object,
				_tokenServiceMock.Object,
				_eventPublisherMock.Object,
				_configurationMock.Object,
				_twoFactorServiceMock.Object,
				_cacheMock.Object
			);
		}

		[Fact]
		public async Task RegisterAsync_WithValidData_ShouldReturnSuccessResponse()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "test@example.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!",
				FirstName = "John",
				LastName = "Doe",
				PhoneNumber = "+1234567890"
			};

			_userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
				.ReturnsAsync((ApplicationUser)null!); // Пользователь не существует

			_userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(It.IsAny<ApplicationUser>(), true))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(new List<string> { AppRoles.User });

			_eventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<UserRegisteredEvent>()))
				.Returns(Task.CompletedTask);

			// Act
			var result = await _authService.RegisterAsync(request);

			// Assert
			result.Should().NotBeNull();
			result.Success.Should().BeTrue();
			result.Message.Should().Be("Регистрация успешна");

			// Проверяем, что методы были вызваны
			_userManagerMock.Verify(x => x.CreateAsync(
				It.Is<ApplicationUser>(u =>
					u.Email == request.Email &&
					u.FirstName == request.FirstName &&
					u.LastName == request.LastName
				),
				request.Password),
				Times.Once);

			_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User), Times.Once);
			_userManagerMock.Verify(x => x.SetTwoFactorEnabledAsync(It.IsAny<ApplicationUser>(), true), Times.Once);

			_eventPublisherMock.Verify(x => x.PublishAsync(
				"user-registered-topic",
				It.Is<UserRegisteredEvent>(e => e.Email == request.Email)
			), Times.Once);
		}

		[Fact]
		public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailureResponse()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "existing@example.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!",
				FirstName = "Jane",
				LastName = "Doe"
			};

			var existingUser = new ApplicationUser
			{
				Id = "user-123",
				Email = request.Email,
				UserName = request.Email
			};

			_userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
				.ReturnsAsync(existingUser);

			// Act
			var result = await _authService.RegisterAsync(request);

			// Assert
			result.Should().NotBeNull();
			result.Success.Should().BeFalse();
			result.Message.Should().Be("Пользователь с таким email уже существует");

			// Проверяем, что CreateAsync НЕ был вызван
			_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
			_eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<UserRegisteredEvent>()), Times.Never);
		}

		[Fact]
		public async Task RegisterAsync_WhenUserCreationFails_ShouldReturnFailureResponse()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "test@example.com",
				Password = "weak",
				ConfirmPassword = "weak",
				FirstName = "John",
				LastName = "Doe"
			};

			_userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
				.ReturnsAsync((ApplicationUser)null!);

			var identityErrors = new[]
			{
				new IdentityError { Code = "PasswordTooShort", Description = "Пароль слишком короткий" },
				new IdentityError { Code = "PasswordRequiresDigit", Description = "Пароль должен содержать цифру" }
			};

			_userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
				.ReturnsAsync(IdentityResult.Failed(identityErrors));

			// Act
			var result = await _authService.RegisterAsync(request);

			// Assert
			result.Should().NotBeNull();
			result.Success.Should().BeFalse();
			result.Message.Should().Contain("Ошибка при создании пользователя");
			result.Message.Should().Contain("Пароль слишком короткий");
			result.Message.Should().Contain("Пароль должен содержать цифру");

			// Проверяем, что роль НЕ была добавлена
			_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
			_eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<UserRegisteredEvent>()), Times.Never);
		}

		[Fact]
		public async Task RegisterAsync_WhenKafkaPublishFails_ShouldStillReturnSuccess()
		{
			// Arrange
			var request = new RegisterRequest
			{
				Email = "test@example.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!",
				FirstName = "John",
				LastName = "Doe"
			};

			_userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
				.ReturnsAsync((ApplicationUser)null!);

			_userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(x => x.SetTwoFactorEnabledAsync(It.IsAny<ApplicationUser>(), true))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(new List<string> { AppRoles.User });

			// Kafka публикация падает с ошибкой
			_eventPublisherMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<UserRegisteredEvent>()))
				.ThrowsAsync(new Exception("Kafka недоступен"));

			// Act
			var result = await _authService.RegisterAsync(request);

			// Assert
			result.Should().NotBeNull();
			result.Success.Should().BeTrue(); // Регистрация всё равно успешна
			result.Message.Should().Be("Регистрация успешна");
		}

		// Вспомогательные методы для создания моков UserManager и SignInManager
		private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
		{
			var store = new Mock<IUserStore<TUser>>();
			var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
			mgr.Object.UserValidators.Add(new UserValidator<TUser>());
			mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
			return mgr;
		}

		private Mock<SignInManager<ApplicationUser>> MockSignInManager()
		{
			var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
			var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
			var options = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
			var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>();
			var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
			var confirmation = new Mock<IUserConfirmation<ApplicationUser>>();

			return new Mock<SignInManager<ApplicationUser>>(
				_userManagerMock.Object,
				contextAccessor.Object,
				claimsFactory.Object,
				options.Object,
				logger.Object,
				schemes.Object,
				confirmation.Object);
		}
	}
}