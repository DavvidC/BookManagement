using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookManagement.Data;
using BookManagement.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookManagement.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookController> _logger;

        public BookController(ApplicationDbContext context, ILogger<BookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var books = from b in _context.Books
                        where b.UserId == null
                        select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString) || b.Author.Contains(searchString));
            }

            var bookList = await books.ToListAsync();
            foreach (var book in bookList)
            {
                var ratings = await _context.Ratings.Where(r => r.BookId == book.Id).ToListAsync();
                book.AverageRating = ratings.Any() ? ratings.Average(r => r.Value) : 0;
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;

            return View(bookList);
        }

        public async Task<IActionResult> MyBooks(string searchString)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var books = _context.Books.Where(b => b.UserId == userId);

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString) || b.Author.Contains(searchString));
            }

            var bookList = await books.ToListAsync();
            foreach (var book in bookList)
            {
                var ratings = await _context.Ratings.Where(r => r.BookId == book.Id).ToListAsync();
                book.AverageRating = ratings.Any() ? ratings.Average(r => r.Value) : 0;
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;

            return View(bookList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                book.IsPublic = true; // Ustawienie IsPublic na true
                book.UserId = null; // Ustawienie UserId na NULL
                _context.Books.Add(book); // Dodanie książki do kontekstu bazy danych
                await _context.SaveChangesAsync(); // Zapisanie zmian w bazie danych
                TempData["SuccessMessage"] = "Książka została dodana pomyślnie.";
                return RedirectToAction("Index"); // Przekierowanie na stronę główną po pomyślnym dodaniu książki
            }

            var errorMessages = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
            _logger.LogError("Błąd podczas dodawania książki: " + errorMessages);
            TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania książki: " + errorMessages;
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View(book); // Powrót do widoku Create w przypadku błędów walidacji
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;

            var ratings = await _context.Ratings.Where(r => r.BookId == id).ToListAsync();
            var averageRating = ratings.Any() ? ratings.Average(r => r.Value) : 0;
            ViewBag.AverageRating = averageRating;

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddRating(int bookId, int rating)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby dodać ocenę.";
                return RedirectToAction("Login", "Home");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Ocena musi być w zakresie od 1 do 5.";
                return RedirectToAction("Index");
            }

            var newRating = new Rating
            {
                Value = rating,
                BookId = bookId
            };

            _context.Ratings.Add(newRating);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ocena została dodana pomyślnie.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddToMyBooks(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby dodać książkę do swojej kolekcji.";
                return RedirectToAction("Login", "Home");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == null);
            if (book != null)
            {
                book.UserId = userId.Value;
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Książka została dodana do Twojej kolekcji.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie udało się dodać książki do Twojej kolekcji.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromMyBooks(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book != null)
            {
                book.UserId = null; // Usunięcie przypisania książki do użytkownika
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Książka została usunięta z Twojej kolekcji.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie udało się usunąć książki z Twojej kolekcji.";
            }

            return RedirectToAction("MyBooks");
        }

        [HttpPost]
        public async Task<IActionResult> ReleaseBook(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book != null)
            {
                book.UserId = null; // Zwalnianie przypisania książki do użytkownika
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Książka została zwolniona.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie udało się zwolnić książki.";
            }

            return RedirectToAction("MyBooks");
        }
    }
}
