using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCSklepInternetowyNET8.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MVCSklepInternetowyNET8.Controllers
{
    public class ProductController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Product/ProductList (dla zywklych uzytkownikow na zakupy)
        [AllowAnonymous]
        public async Task<IActionResult> ProductList(string filter = null, string searchQuery = null)
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Filtruj produkty według słów kluczowych
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    p.Description.Contains(searchQuery));
            }

            // Filtruj według nowości
            if (filter == "New")
            {
                productsQuery = productsQuery.OrderByDescending(p => p.CreatedDate).Take(5);
            }

            // Filtruj według promocji
            if (filter == "Promotion")
            {
                productsQuery = productsQuery.Where(p => p.IsOnPromotion && (p.PromotionEndDate == null || p.PromotionEndDate > DateTime.Now));
            }

            var products = await productsQuery
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            // Ustawienie właściwości IsNewest dla pierwszych 5 produktów
            for (int i = 0; i < products.Count; i++)
            {
                products[i].IsNewest = i < 5;
            }

            ViewBag.Filter = filter;
            ViewBag.SearchQuery = searchQuery;
            return View(products);
        }


        // GET: Product
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var onlineShopContext = _context.Products.Include(p => p.Category);
            return View(await onlineShopContext.ToListAsync());
        }

        // GET: Product/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }


        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    CreatedDate = DateTime.Now,
                    LargeImage = await ConvertToBytes(model.LargeImageFile),
                    Thumbnail = await GenerateThumbnail(model.LargeImageFile),
                    IsOnPromotion = model.IsOnPromotion,
                    PromotionEndDate = model.PromotionEndDate,
                    OriginalPrice = model.IsOnPromotion ? model.Price : (decimal?)null // Jeśli promocja, ustaw starą cenę
                };

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Produkt został pomyślnie dodany.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania produktu.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", model.CategoryId);
            return View(model);
        }



        // Konwersja pliku na tablicę bajtów
        private async Task<byte[]> ConvertToBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        // Generowanie miniaturki
        private async Task<byte[]> GenerateThumbnail(IFormFile file)
        {
            using var image = Image.Load(file.OpenReadStream()); // Zmieniono na OpenReadStream
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(100, 100),
                Mode = ResizeMode.Crop
            }));

            using var thumbnailStream = new MemoryStream();
            image.SaveAsJpeg(thumbnailStream);
            return thumbnailStream.ToArray();
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Konwersja Product na ProductViewModel
            var productViewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                LargeImage = product.LargeImage,
                Thumbnail = product.Thumbnail,
                CreatedDate = product.CreatedDate 
            };


            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(productViewModel);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);

                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Aktualizacja danych produktu
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.StockQuantity = model.StockQuantity;
                    product.CategoryId = model.CategoryId;

                    if (model.IsOnPromotion)
                    {
                        if (!product.IsOnPromotion) // Jeśli promocja była wyłączona, zapisz starą cenę
                        {
                            product.OriginalPrice = product.Price;
                        }
                        product.Price = model.Price; // Ustaw nową promocyjną cenę
                    }
                    else
                    {
                        product.OriginalPrice = null; // Usuń starą cenę, jeśli promocja jest wyłączona
                    }

                    product.IsOnPromotion = model.IsOnPromotion;
                    product.PromotionEndDate = model.PromotionEndDate;

                    if (model.LargeImageFile != null)
                    {
                        product.LargeImage = await ConvertToBytes(model.LargeImageFile);
                        product.Thumbnail = await GenerateThumbnail(model.LargeImageFile);
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(model.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["SuccessMessage"] = "Produkt został zaktualizowany.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Wystąpił błąd podczas aktualizacji produktu.";
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", model.CategoryId);
            return View(model);
        }





        // GET: Product/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
