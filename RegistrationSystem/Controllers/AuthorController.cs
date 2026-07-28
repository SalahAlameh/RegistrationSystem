using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Models;
using RegistrationSystem.Services;


namespace RegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuthorController : Controller
    {
        private readonly AuthorService _authorService;

        public AuthorController(AuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: /Author/Index
        // Displays the list of all authors.
        [HttpGet]
        public IActionResult Index()
        {
            var authors = _authorService.GetAll();
            return View(authors);
        }

        // GET: /Author/Create
        // Displays an empty form for creating an author.
        [HttpGet]
        public IActionResult Add()
        {
            return View(new Author());
        }

        // POST: /Author/Create
        // Receives the submitted form and creates a new author.
        [HttpPost] 
        public IActionResult Add(Author author)
        {
            if (!ModelState.IsValid)
            {
                return View(author);
            }

            _authorService.Create(author);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Author/Edit/5
        // Loads the selected author into the edit form.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var author = _authorService.GetById(id);

            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: /Author/Edit
        // Receives the edited author and updates it in the database.
        [HttpPost]
        public IActionResult Edit(Author author)
        {
            if (!ModelState.IsValid)
            {
                return View(author);
            }

            var updated = _authorService.Update(author);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Author/Delete/5
        // Deletes the selected author from the database.
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _authorService.Delete(id);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Author/Archive
        [HttpGet]
        public IActionResult Archive()
        {
            return RedirectToAction("Index", "Archive"); // sends old Book archive links to the one combined Archive page
        }

        // POST: /Author/Restore/5
        [HttpPost]
        public IActionResult Restore(int id)
        {
            _authorService.Restore(id); // restores the archived author

            return RedirectToAction("Index", "Archive"); // stays on the combined Archive page after restoring
        }
    }
}