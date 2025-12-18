using AutoMapper;
using CatalogService.DTO;
using CatalogService.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;
        public CategoriesController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] DTO.CreateCategoryRequest request)
        {

            var category = _mapper.Map<Models.Category>(request);
            var result = await _categoryService.CreateAsync(category);
            var dto = _mapper.Map<CategoryDto>(result);

            return Ok(dto);

        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            var dto = _mapper.Map<CategoryDto>(category);
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] DTO.CreateCategoryRequest request)
        {
            var category = _mapper.Map<Models.Category>(request);
            var updated = await _categoryService.UpdateAsync(id, category);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var deleted = await _categoryService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
