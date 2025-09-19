using Microsoft.AspNetCore.Mvc;
using MusicSharing.Api.Models;
using MusicSharing.Api.Services;

namespace MusicSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly BlogService _blogService;

    public BlogController(BlogService blogService)
    {
        _blogService = blogService;
    }

    // GET: api/blog
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var posts = await _blogService.GetAllAsync();
        return Ok(posts);
    }

    // GET: api/blog/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var post = await _blogService.GetByIdAsync(id);
        if (post == null) return NotFound();
        return Ok(post);
    }

    // POST: api/blog
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BlogPost post)
    {
        var created = await _blogService.CreateAsync(post);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/blog/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BlogPost updated)
    {
        var post = await _blogService.UpdateAsync(id, updated);
        if (post == null) return NotFound();
        return Ok(post);
    }

    // DELETE: api/blog/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _blogService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}