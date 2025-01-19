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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Xml.Linq;

namespace MVCSklepInternetowyNET8.Controllers
{
    public class ProductController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Product/ProductList (dla zwykłych użytkowników na zakupy)
        public async Task<IActionResult> ProductList(
            string filter = null,
            string searchQuery = null,
            int? categoryId = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            int? minStockQuantity = null,
            int pageNumber = 1, // Domyślna strona
            int pageSize = 8    // Domyślna liczba elementów na stronę
        )
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Filtruj według zakresu cen
            if (priceFrom.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= priceFrom.Value);
            }

            if (priceTo.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= priceTo.Value);
            }

            // Filtruj według minimalnej dostępnej ilości produktów
            if (minStockQuantity.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.StockQuantity >= minStockQuantity.Value);
            }

            // Filtruj produkty według słów kluczowych
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    p.Description.Contains(searchQuery));
            }

            // Filtruj według kategorii i jej podkategorii
            if (categoryId.HasValue)
            {
                var subCategoryIds = await GetSubCategoryIds(categoryId.Value);
                productsQuery = productsQuery.Where(p => subCategoryIds.Contains(p.CategoryId));
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

            // Stronnicowanie
            var totalItems = await productsQuery.CountAsync(); // Liczba wszystkich elementów
            var products = await productsQuery
                .Skip((pageNumber - 1) * pageSize) // Pomijamy elementy z poprzednich stron
                .Take(pageSize)                   // Pobieramy elementy na aktualną stronę
                .ToListAsync();


            // Pobranie 5 najnowszych produktów z bazy danych
            var newestProductsIds = await _context.Products
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .Select(p => p.ProductId)
                .ToListAsync();

            // Ustawienie właściwości IsNewest na podstawie globalnej listy najnowszych produktów
            foreach (var product in products)
            {
                product.IsNewest = newestProductsIds.Contains(product.ProductId);
            }


            var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();

            // Dodanie danych do ViewBag
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = pageNumber;                       // Bieżąca strona
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize); // Liczba stron
            ViewBag.PageSize = pageSize;                            // Rozmiar strony
            ViewBag.Filter = filter;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.CategoryId = categoryId;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewBag.MinStockQuantity = minStockQuantity;
            ViewBag.PageSize = pageSize;


            return View(products);
        }

        // Metoda do pobierania ID kategorii i jej podkategorii
        private async Task<List<int>> GetSubCategoryIds(int categoryId)
        {
            var categoryIds = new List<int> { categoryId };
            var subCategories = await _context.Categories.Where(c => c.ParentCategoryId == categoryId).ToListAsync();

            foreach (var subCategory in subCategories)
            {
                categoryIds.AddRange(await GetSubCategoryIds(subCategory.CategoryId));
            }

            return categoryIds;
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePriceListPdf(int categoryId)
        {
            // Pobierz produkty z wybranej kategorii i jej podkategorii
            var subCategoryIds = await GetSubCategoryIds(categoryId);
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => subCategoryIds.Contains(p.CategoryId))
                .ToListAsync();

            // Utwórz dokument PDF
            using (var memoryStream = new MemoryStream())
            {
                var document = new iTextSharp.text.Document(); // Użycie jawne klasy z iTextSharp
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Dodaj tytuł
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph($"Cennik produktów dla kategorii: {products.FirstOrDefault()?.Category.Name}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Dodaj tabelę
                var table = new PdfPTable(3); // 3 kolumny: Nazwa, Cena, Dostępna ilość
                table.WidthPercentage = 100;

                // Nagłówki tabeli
                table.AddCell("Nazwa");
                table.AddCell("Cena");
                table.AddCell("Dostepna ilosc");

                // Dodaj produkty
                foreach (var product in products)
                {
                    table.AddCell(product.Name);
                    table.AddCell(product.Price.ToString("C"));
                    table.AddCell(product.StockQuantity.ToString());
                }

                document.Add(table);
                document.Close();

                // Zwróć plik PDF
                return File(memoryStream.ToArray(), "application/pdf", $"Cennik_{categoryId}.pdf");
            }
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
            using var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()); // Zmieniono na OpenReadStream
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

        // Wyświetlenie szczegółów produktu dla klienta
        [AllowAnonymous]
        public async Task<IActionResult> ClientDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

    }
}
