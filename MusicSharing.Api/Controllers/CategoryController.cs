using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/category
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    // GET: api/category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    // POST: api/category
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category category)
    {
        var created = await _categoryService.CreateAsync(category);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/category/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Category updated)
    {
        var category = await _categoryService.UpdateAsync(id, updated);
        if (category == null) return NotFound();
        return Ok(category);
    }

    // DELETE: api/category/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}