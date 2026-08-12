using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Models;
using RegistrationSystem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace RegistrationSystem.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly BookService _bookService;
        private readonly AuthorService _authorService; // added to use the AuthorService
        private readonly CategoryService _categoryService; // added to use the CategoryService
        private readonly IStringLocalizer<SharedResource> _localizer; // added to use the IStringLocalizer for localization
    

        public BookController(BookService bookService, AuthorService authorService, CategoryService categoryService, IStringLocalizer<SharedResource> localizer) //added the later parameters to the constructor to inject the AuthorService and CategoryService
        {
            _bookService = bookService;
            _authorService = authorService; // this was also added to use the AuthorService
            _categoryService = categoryService; // added to use the CategoryService
            _localizer = localizer; // added to use the IStringLocalizer for localization
        } // here the controller receives all 4

        // GET: /Book/Add
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            var model = new BookViewModel(); // creates an empty form model
            PopulateAuthors(model);          // loads all authors from the database
            PopulateCategories(model);       // loads all categories from the database

            return View(model); // sends the book fields, author options, and category options to Add.cshtml
        }

        // POST: /Book/Add
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Add(BookViewModel model) // this receives the submitted form
        {
            if (!ModelState.IsValid) // if validation fails
            {
                PopulateAuthors(model); // the author list must be loaded again because the form is redisplayed with errors
                PopulateCategories(model); // same for categories
                return View(model);
            }

            var book = new Book // converting the ViewModel into a Book
            {
                Title = model.Title,
                CategoryId = model.CategoryId, // Genre was replaced by CategoryId
                Copies = model.Copies
            };

            var saved = _bookService.AddWithAuthors(book, model.SelectedAuthorIds); // this method saves the book and the selected authors in the database


            if (!saved) // handling invalid author IDs
            {
                ModelState.AddModelError(
                        nameof(model.SelectedAuthorIds),
                        _localizer["InvalidAuthorsSelection"]);

                PopulateAuthors(model);
                PopulateCategories(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Index
        [HttpGet]
        public IActionResult Index()
        {
            var allBooks = _bookService.GetAll();

            return View(allBooks);
        }

        // GET: /Book/Edit/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _bookService.GetById(id);

            if (book == null)
            {
                return NotFound();
            }

            var model = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                CategoryId = book.CategoryId, // Genre was replaced by CategoryId
                Copies = book.Copies,
                SelectedAuthorIds = book.Authors
                    .Select(author => author.Id)
                    .ToList()
            };

            PopulateAuthors(model); // loads every author and marks the ones that are already associated with the book as selected
            PopulateCategories(model); // loads every category and marks the one already assigned to the book as selected

            return View(model);
        }
        // POST: /Book/Edit
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Edit(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateAuthors(model);
                PopulateCategories(model);
                return View(model);
            }

            var book = new Book
            {
                Id = model.Id,
                Title = model.Title,
                CategoryId = model.CategoryId, // Genre was replaced by CategoryId
                Copies = model.Copies
            };

            var updated = _bookService.UpdateWithAuthors(book, model.SelectedAuthorIds);


            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
        // POST: /Book/Delete/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _bookService.Delete(id);
            return RedirectToAction("Index");
        }

        private void PopulateAuthors(BookViewModel model) // this converts each Author into a SelectListItem used by the HTML <select> element.
        {
            model.Authors = _authorService.GetAll()
                .Select(author => new SelectListItem
                {
                    Value = author.Id.ToString(),
                    Text = author.FullName,
                    Selected = model.SelectedAuthorIds.Contains(author.Id) // this marks the authors that are already associated with the book as selected
                })
                .ToList();
        }

        private void PopulateCategories(BookViewModel model) // same idea as PopulateAuthors, but one selection instead of many
        {
            model.Categories = _categoryService.GetAll()
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name,
                    Selected = model.CategoryId == category.Id // marks the category already assigned to the book as selected
                })
                .ToList();
        }

        // GET: /Book/Archive
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Archive()
        {
            return RedirectToAction("Index", "Archive"); // sends old Book archive links to the one combined Archive page
        }

        // POST: /Book/Restore/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Restore(int id)
        {
            _bookService.Restore(id); // changes IsDeleted back to false and clears DeletedAt

            return RedirectToAction("Index", "Archive"); // stays on the combined Archive page after restoring
        }
    }
}