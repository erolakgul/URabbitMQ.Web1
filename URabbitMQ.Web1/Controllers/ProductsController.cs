using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URabbitMQ.Web1.Context;
using URabbitMQ.Web1.Models;
using URabbitMQ.Web1.Services.Events;
using URabbitMQ.Web1.Services.Pubs;

namespace URabbitMQ.Web1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;
        private readonly IWebHostEnvironment _environment;
        public ProductsController(AppDbContext context, RabbitMQPublisher rabbitMQPublisher, IWebHostEnvironment environment)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
            _environment = environment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
              return _context.Products != null ? 
                          View(await _context.Products.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.Products'  is null.");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Price,AvailableStock")] Product product,IFormFile imageFile)
        {
            // data modeli uygu değilse dön
           if (!ModelState.IsValid) return View(product);

            try
            {
                string randomImageName = string.Empty;

                if (imageFile is { Length: > 0 })
                {
                    //random isim oluştur
                    randomImageName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    //Images klasör path ini al
                    var path = Path.Combine(_environment.WebRootPath, "Images",randomImageName);
                   // path = Path.Combine(path, randomImageName);
                    //ilgili yol için filestream i oluştur
                    await using FileStream fileStream = new(path: path, FileMode.Create,FileAccess.ReadWrite);
                    // ve bunu kaydet
                    await imageFile.CopyToAsync(fileStream);

                    // kopyalamadan rabbit i haberdar et
                    _rabbitMQPublisher.Publish(new ProductImagesCreatedEvent() { ImageName = randomImageName });
                }

                // db ye ürünü kaydet
                product.ImageName = randomImageName;
                _context.Add(product);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Price,AvailableStock,PictureUrl")] Product product)
        {
            if (id != product.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'AppDbContext.Products'  is null.");
            }
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
          return (_context.Products?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
