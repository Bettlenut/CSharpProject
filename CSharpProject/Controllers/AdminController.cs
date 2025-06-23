using CSharpProject.Data;
using CSharpProject.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace TequliasRestaurant.Controllers
{
    public class AdminController : Controller
    {
        private Repository<Product> products;
        private Repository<Category> categories;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            products = new Repository<Product>(context);
            categories = new Repository<Category>(context);
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await products.GetAllAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            return View(await products.GetByIdAsync(id, new QueryOptions<Product>()));
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            ViewBag.Categories = await categories.GetAllAsync();
            if (id == 0)
            {
                ViewBag.Operation = "Add";
                return View(new Product());
            }
            else
            {
                Product product = await products.GetByIdAsync(id, new QueryOptions<Product> { Includes = "Category" });
                ViewBag.Operation = "Edit";
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Product product, int catId)
        {
            ViewBag.Categories = await categories.GetAllAsync();
            if (ModelState.IsValid)
            {

                if (product.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = uniqueFileName;
                }

                if (product.ProductId == 0)
                {

                    product.CategoryId = catId;

                    await products.AddAsync(product);
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    var existingProduct = await products.GetByIdAsync(product.ProductId, new QueryOptions<Product>());

                    if (existingProduct == null)
                    {
                        ModelState.AddModelError("", "Product not found.");
                        ViewBag.Categories = await categories.GetAllAsync();
                        return View(product);
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Stock = product.Stock;
                    existingProduct.CategoryId = catId;

                    if (product.ImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(uploadsFolder, existingProduct.ImageUrl);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        existingProduct.ImageUrl = uniqueFileName;
                    }

                    await products.UpdateAsync(existingProduct);
                }
            }
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            return View(await products.GetByIdAsync(id, new QueryOptions<Product>()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Product Product)
        {
            await products.DeleteAsync(Product.ProductId);
            return RedirectToAction("Index");
        }
    }
}