using Erox.ImageService.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ImageService.Tests.Services
{
	public class S3ServiceTests
	{
		[Fact]
		public void Constructor_ShouldThrowException_WhenBucketNameIsNull()
		{
			// Arrange
			var configMock = new Mock<IConfiguration>();
			configMock.Setup(c => c["AWS:Region"]).Returns("us-east-1");
			configMock.Setup(c => c["AWS:BucketName"]).Returns((string?)null);
			configMock.Setup(c => c["AWS:AccessKey"]).Returns("key");
			configMock.Setup(c => c["AWS:SecretKey"]).Returns("secret");

			// Act & Assert
			var act = () => new S3Service(configMock.Object);
			act.Should().Throw<ArgumentNullException>();
		}

		[Fact]
		public void Constructor_ShouldCreateService_WhenConfigIsValid()
		{
			// Arrange
			var configMock = new Mock<IConfiguration>();
			configMock.Setup(c => c["AWS:Region"]).Returns("us-east-1");
			configMock.Setup(c => c["AWS:BucketName"]).Returns("test-bucket");
			configMock.Setup(c => c["AWS:AccessKey"]).Returns("test-key");
			configMock.Setup(c => c["AWS:SecretKey"]).Returns("test-secret");

			// Act
			var service = new S3Service(configMock.Object);

			// Assert
			service.Should().NotBeNull();
		}
	}
}