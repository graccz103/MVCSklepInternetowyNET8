﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly OnlineShopContext _context;

    public CategoryController(OnlineShopContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kategoria została pomyślnie dodana.";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            // Collect the validation errors and store them in ViewData
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewData["ModelErrors"] = errors;
        }

        TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania kategorii.";
        ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", category.ParentCategoryId);
        return View(category);
    }




    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", category.ParentCategoryId);
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.CategoryId) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", category.ParentCategoryId);
        return View(category);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.Include(c => c.ParentCategory).FirstOrDefaultAsync(m => m.CategoryId == id);
        if (category == null) return NotFound();

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
