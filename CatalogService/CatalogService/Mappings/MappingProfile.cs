using CatalogService.DTO;
using CatalogService.Models;

namespace CatalogService.Mappings
{
    public class MappingProfile: AutoMapper.Profile
    {
        public MappingProfile()
        {
            // Category
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<Category, CategoryDto>();

            // Product
            CreateMap<ProductRequest, Product>()
                .ForMember(d => d.ProductSizes, opt => opt.MapFrom(s =>
                 s.Sizes.Select(x => new ProductSize { Size = x.Size, Quantity = x.Quantity })));

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(d => d.Sizes, opt => opt.MapFrom(s =>
                s.ProductSizes.Select(x => new ProductSizeDto { Size = x.Size, Quantity = x.Quantity })))
                .ForMember(dest => dest.AdditionalImageUrls,
                    opt => opt.MapFrom(src => src.AdditionalImages.Select(i => i.ImageUrl)));

            CreateMap<ProductWithImagesRequest, Models.Product>()
                .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalImages, opt => opt.Ignore())
                .ForMember(d => d.ProductSizes, opt => opt.MapFrom(s =>
                 s.Sizes.Select(x => new ProductSize { Size = x.Size, Quantity = x.Quantity })));
        }
    }
}
