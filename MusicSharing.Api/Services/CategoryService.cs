using Microsoft.EntityFrameworkCore;
using MusicSharing.Api.Data;
using MusicSharing.Api.Models;

namespace MusicSharing.Api.Services;

public class CategoryService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(int id, Category updated)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = updated.Name;
        category.Description = updated.Description;
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}