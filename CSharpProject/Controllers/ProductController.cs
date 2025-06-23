using CSharpProject.Data;
using CSharpProject.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;


namespace CSharpProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private Repository<Product> _products;
        private Repository<Order> _orders;
        private readonly UserManager<ApplicationUser> _userManager;
        private Repository<Category> categories;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _products = new Repository<Product>(context);
            _orders = new Repository<Order>(context);
            categories = new Repository<Category>(context);
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _products.GetAllAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _products.GetByIdAsync(id, new QueryOptions<Product> { Includes = "Category" });

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddItem(int prodId, int prodQty)
        {
            var product = await _context.Products.FindAsync(prodId);
            if (product == null)
            {
                return NotFound();
            }

            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _products.GetAllAsync()
            };

            var existingItem = model.OrderItems.FirstOrDefault(oi => oi.ProductId == prodId);

            if (existingItem != null)
            {
                existingItem.Quantity += prodQty;
            }
            else
            {
                model.OrderItems.Add(new OrderItemViewModel
                {
                    ProductId = product.ProductId,
                    Price = product.Price,
                    Quantity = prodQty,
                    ProductName = product.Name
                });
            }

            model.TotalAmount = model.OrderItems.Sum(oi => oi.Price * oi.Quantity);

            HttpContext.Session.Set("OrderViewModel", model);

            return RedirectToAction("Index", model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {

            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");

            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(int id)
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Index");
            }

            Order order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount,
                UserId = _userManager.GetUserId(User)
            };

            foreach (var item in model.OrderItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            await _orders.AddAsync(order);

            HttpContext.Session.Remove("OrderViewModel");

            var product = await _products.GetByIdAsync(id, new QueryOptions<Product>()); // ✅ Await the result
            TempData["TotalAmount"] = (int)(model.TotalAmount); // 💲 Stripe wants cents
            return RedirectToAction("CreateCheckoutSession", "Stripe");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User);

            var userOrders = await _orders.GetAllByIdAsync(userId, "UserId", new QueryOptions<Order>
            {
                Includes = "OrderItems.Product"
            });

            return View(userOrders);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            // Retrieve the OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Index");
            }
            // Find the item to remove
            var itemToRemove = model.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            if (itemToRemove != null)
            {
                model.OrderItems.Remove(itemToRemove);
                // Update the total amount
                model.TotalAmount = model.OrderItems.Sum(oi => oi.Price * oi.Quantity);
                // Save updated OrderViewModel to session
                HttpContext.Session.Set("OrderViewModel", model);
            }
            return RedirectToAction("Cart");
        }
    }
}
