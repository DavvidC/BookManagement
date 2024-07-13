using BookManagement.Data;
using BookManagement.Models; // Dodaj tę linię
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BookManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Where(b => b.UserId == null)
                .OrderBy(r => EF.Functions.Random())
                .Take(6)
                .ToListAsync();

            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            if (ViewBag.IsLoggedIn)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = await _context.Users.FindAsync(userId);
                ViewBag.Username = user?.FirstName;
            }

            return View(books);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rejestracja przebiegła pomyślnie! Możesz się teraz zalogować.";
                return RedirectToAction("Login");
            }

            TempData["ErrorMessage"] = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage));
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                TempData["SuccessMessage"] = "Zalogowano pomyślnie.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło");
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> AddToMyBooks(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == null);
            if (book != null)
            {
                book.UserId = userId.Value;
                _context.Update(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
