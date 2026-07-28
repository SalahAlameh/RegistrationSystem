using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Models;
using RegistrationSystem.Services;

namespace RegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")] // only administrators can see archived records
    public class ArchiveController : Controller
    {
        private readonly PersonService _personService;
        private readonly BookService _bookService;
        private readonly AuthorService _authorService;
        private readonly CategoryService _categoryService;

        public ArchiveController(
            PersonService personService,
            BookService bookService,
            AuthorService authorService,
            CategoryService categoryService)
        {
            _personService = personService;
            _bookService = bookService;
            _authorService = authorService;
            _categoryService = categoryService;
        }

        // GET: /Archive/Index
        [HttpGet]
        public IActionResult Index()
        {
            var model = new ArchiveViewModel
            {
                Persons = _personService.GetArchived(),
                Books = _bookService.GetArchived(),
                Authors = _authorService.GetArchived(),
                Categories = _categoryService.GetArchived()
            };

            return View(model);
        }
    }
}