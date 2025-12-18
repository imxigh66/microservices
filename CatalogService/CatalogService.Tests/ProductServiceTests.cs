using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogService.Tests
{
	public class ProductServiceTests : IDisposable
	{
		private readonly ProductDbContext _context;
		private readonly ProductService _productService;

		public ProductServiceTests()
		{
			var options = new DbContextOptionsBuilder<ProductDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new ProductDbContext(options);
			_productService = new ProductService(_context);
		}

		[Fact]
		public async Task CreateAsync_ShouldCreateProduct_AndReturnIt()
		{
			// Arrange
			var category = new Category { Id = 1, Name = "T-Shirts" };
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();

			var product = new Product
			{
				Name = "Cool T-Shirt",
				Description = "Very cool",
				Price = 29.99m,
				Sex = "Male",
				Season = "Summer",
				CategoryId = 1
			};

			// Act
			var result = await _productService.CreateAsync(product);

			// Assert
			result.Should().NotBeNull();
			result.Id.Should().BeGreaterThan(0);
			result.Name.Should().Be("Cool T-Shirt");
			result.Price.Should().Be(29.99m);

			var productInDb = await _context.Products.FindAsync(result.Id);
			productInDb.Should().NotBeNull();
			productInDb!.Name.Should().Be("Cool T-Shirt");
		}

		[Fact]
		public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoProducts()
		{
			// Act
			var result = await _productService.GetAllAsync();

			// Assert
			result.Should().BeEmpty();
		}

		[Fact]
		public async Task GetAllAsync_ShouldReturnAllProducts_WithRelatedData()
		{
			// Arrange
			var category = new Category { Id = 1, Name = "Jeans" };
			_context.Categories.Add(category);

			var product1 = new Product
			{
				Name = "Blue Jeans",
				Description = "Classic blue jeans",
				Price = 59.99m,
				Sex = "Unisex",
				Season = "All",
				CategoryId = 1
			};

			var product2 = new Product
			{
				Name = "Black Jeans",
				Description = "Stylish black jeans",
				Price = 69.99m,
				Sex = "Male",
				Season = "All",
				CategoryId = 1
			};

			_context.Products.AddRange(product1, product2);
			await _context.SaveChangesAsync();

			// Act
			var result = await _productService.GetAllAsync();

			// Assert
			result.Should().HaveCount(2);
			result.Should().Contain(p => p.Name == "Blue Jeans");
			result.Should().Contain(p => p.Name == "Black Jeans");
			result.All(p => p.Category != null).Should().BeTrue();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
		{
			// Arrange
			var category = new Category { Id = 1, Name = "Shoes" };
			_context.Categories.Add(category);

			var product = new Product
			{
				Name = "Sneakers",
				Description = "Comfortable sneakers",
				Price = 89.99m,
				Sex = "Unisex",
				Season = "All",
				CategoryId = 1
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			// Act
			var result = await _productService.GetByIdAsync(product.Id);

			// Assert
			result.Should().NotBeNull();
			result!.Name.Should().Be("Sneakers");
			result.Price.Should().Be(89.99m);
			result.Category.Should().NotBeNull();
			result.Category!.Name.Should().Be("Shoes");
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
		{
			// Act
			var result = await _productService.GetByIdAsync(999);

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task UpdateAsync_ShouldUpdateProduct_WhenExists()
		{
			// Arrange
			var category1 = new Category { Id = 1, Name = "T-Shirts" };
			var category2 = new Category { Id = 2, Name = "Hoodies" };
			_context.Categories.AddRange(category1, category2);

			var product = new Product
			{
				Name = "Old Name",
				Description = "Old Description",
				Price = 25.00m,
				Sex = "Male",
				Season = "Summer",
				CategoryId = 1
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			var updatedProduct = new Product
			{
				Name = "New Name",
				Description = "New Description",
				Price = 35.00m,
				Sex = "Female",
				Season = "Winter",
				CategoryId = 2
			};

			// Act
			var result = await _productService.UpdateAsync(product.Id, updatedProduct);

			// Assert
			result.Should().BeTrue();

			var productInDb = await _context.Products.FindAsync(product.Id);
			productInDb.Should().NotBeNull();
			productInDb!.Name.Should().Be("New Name");
			productInDb.Description.Should().Be("New Description");
			productInDb.Price.Should().Be(35.00m);
			productInDb.Sex.Should().Be("Female");
			productInDb.Season.Should().Be("Winter");
			productInDb.CategoryId.Should().Be(2);
		}

		[Fact]
		public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
		{
			// Arrange
			var updatedProduct = new Product
			{
				Name = "Updated Name",
				Description = "Updated Description",
				Price = 50.00m,
				Sex = "Unisex",
				Season = "Spring",
				CategoryId = 1
			};

			// Act
			var result = await _productService.UpdateAsync(999, updatedProduct);

			// Assert
			result.Should().BeFalse();
		}

		[Fact]
		public async Task DeleteAsync_ShouldDeleteProduct_WhenExists()
		{
			// Arrange
			var category = new Category { Id = 1, Name = "Accessories" };
			_context.Categories.Add(category);

			var product = new Product
			{
				Name = "Hat",
				Description = "Stylish hat",
				Price = 19.99m,
				Sex = "Unisex",
				Season = "All",
				CategoryId = 1
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			// Act
			var result = await _productService.DeleteAsync(product.Id);

			// Assert
			result.Should().BeTrue();

			var productInDb = await _context.Products.FindAsync(product.Id);
			productInDb.Should().BeNull();
		}

		[Fact]
		public async Task DeleteAsync_ShouldReturnFalse_WhenProductDoesNotExist()
		{
			// Act
			var result = await _productService.DeleteAsync(999);

			// Assert
			result.Should().BeFalse();
		}

		[Fact]
		public async Task AddImagesAsync_ShouldAddImages_WhenProductExists()
		{
			// Arrange
			var category = new Category { Id = 1, Name = "Jackets" };
			_context.Categories.Add(category);

			var product = new Product
			{
				Name = "Leather Jacket",
				Description = "Premium leather jacket",
				Price = 199.99m,
				Sex = "Male",
				Season = "Winter",
				CategoryId = 1,
				AdditionalImages = new List<ProductImage>()
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			var imageUrls = new List<string>
			{
				"https://example.com/image1.jpg",
				"https://example.com/image2.jpg",
				"https://example.com/image3.jpg"
			};

			// Act
			var result = await _productService.AddImagesAsync(product.Id, imageUrls);

			// Assert
			result.Should().BeTrue();

			var productInDb = await _context.Products
				.Include(p => p.AdditionalImages)
				.FirstOrDefaultAsync(p => p.Id == product.Id);

			productInDb.Should().NotBeNull();
			productInDb!.AdditionalImages.Should().HaveCount(3);
			productInDb.AdditionalImages.Should().Contain(img => img.ImageUrl == "https://example.com/image1.jpg");
			productInDb.AdditionalImages.Should().Contain(img => img.ImageUrl == "https://example.com/image2.jpg");
			productInDb.AdditionalImages.Should().Contain(img => img.ImageUrl == "https://example.com/image3.jpg");
		}

		[Fact]
		public async Task AddImagesAsync_ShouldReturnFalse_WhenProductDoesNotExist()
		{
			// Arrange
			var imageUrls = new List<string>
			{
				"https://example.com/image1.jpg"
			};

			// Act
			var result = await _productService.AddImagesAsync(999, imageUrls);

			// Assert
			result.Should().BeFalse();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldIncludeProductSizes_WhenTheyExist()
		{
			// Arrange
			var category = new Category { Id = 1, Name = "Shirts" };
			_context.Categories.Add(category);

			var product = new Product
			{
				Name = "Casual Shirt",
				Description = "Comfortable casual shirt",
				Price = 39.99m,
				Sex = "Male",
				Season = "Summer",
				CategoryId = 1,
				ProductSizes = new List<ProductSize>
				{
					new ProductSize { Size = Enums.Size.Size41, Quantity = 10 },
					new ProductSize { Size = Enums.Size.Size40, Quantity = 15 },
					new ProductSize { Size = Enums.Size.Size40, Quantity = 8 }
				}
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			// Act
			var result = await _productService.GetByIdAsync(product.Id);

			// Assert
			result.Should().NotBeNull();
			result!.ProductSizes.Should().HaveCount(3);
			result.ProductSizes.Should().Contain(ps => ps.Size == Enums.Size.Size41 && ps.Quantity == 10);
			result.ProductSizes.Should().Contain(ps => ps.Size == Enums.Size.Size40 && ps.Quantity == 15);
			result.ProductSizes.Should().Contain(ps => ps.Size == Enums.Size.Size40 && ps.Quantity == 8);
		}

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}