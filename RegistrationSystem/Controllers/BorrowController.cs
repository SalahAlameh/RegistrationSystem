using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistrationSystem.Models;
using RegistrationSystem.Services;

namespace RegistrationSystem.Controllers
{
    [Authorize]
    public class BorrowController : Controller
    {
        private readonly BorrowService _borrowService; //
        private readonly PersonService _personService; // takes the 4 services to fetch the lists of people/books for the dropdown
        private readonly BookService _bookService;     //  
        private readonly CategoryService _categoryService; //
        public BorrowController(BorrowService borrowService, PersonService personService, BookService bookService, CategoryService categoryService)
        {
            _borrowService = borrowService;
            _personService = personService;
            _bookService = bookService;
            _categoryService = categoryService;
        }

        // GET: /Borrow/BorrowBook
        [HttpGet]
        public IActionResult BorrowBook() // this is basically the dropdown list for the person and book, it fetches the list of people and books from the database and passes it to the view
        { 
            var viewModel = new BorrowViewModel(); // creates new empty box
            PopulateDropdowns(viewModel); // fills in the two compartments (.Persons, .Books) so the dropdowns actually have options
            return View(viewModel);
        }

        // POST: /Borrow/BorrowBook
        [HttpPost]
        public IActionResult BorrowBook(BorrowViewModel model)
        {
            var book = _bookService.GetById(model.BookId);

            if (book != null && book.Copies < 1)
            {
                ModelState.AddModelError("", "This book is out of stock and cannot be borrowed.");
            }

            int activeBorrows = _borrowService.GetActiveBorrowCountForPerson(model.PersonId);
            if (activeBorrows >= 3)
            {
                ModelState.AddModelError("", "This person already has 3 active borrows and cannot borrow more until one is returned.");
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            var borrow = new Borrow // only reached if everything passed, builds a real Borrow entity from the two IDs
            {
                PersonId = model.PersonId,
                BookId = model.BookId,
                BorrowDate = DateTime.UtcNow
            };

            _borrowService.Borrow(borrow);

            book.Copies -= 1;
            _bookService.Update(book);

            return RedirectToAction("ViewBorrowedBooks");
        }

        // GET: /Borrow/GetBooksByCategory?categoryId=3
        [HttpGet]
        public JsonResult GetBooksByCategory(int? categoryId) // unlike other methods in my project that return a view, this return a json (raw data, now HTML) that will be used by the javascript to populate the book dropdown
        {
            var books = _bookService.GetByCategory(categoryId);

            var result = books.Select(b => new
            {
                value = b.Id,
                text = b.Title
            });

            return Json(result); // converts that { value, text } list into a JSON string like [{"value":1,"text":"Sapiens"},...], which is exactly the shape a browser's JavaScript can easily read and loop over
        }

        // GET: /Borrow/ViewBorrowedBooks
        [HttpGet]
        public IActionResult ViewBorrowedBooks()
        {
            var allBorrows = _borrowService.GetAll();
            return View(allBorrows);
        }

        [HttpPost]
        public IActionResult ReturnBook(int id) // int id is the borrow record being returned, not the bokk's id or the person's id
        {
            var borrow = _borrowService.GetById(id);
            if (borrow == null)
            {
                return NotFound();
            }

            if (borrow.ReturnDate == null) // only process the return if it hasnt been returned
            {
                _borrowService.ReturnBook(borrow);

                var book = _bookService.GetById(borrow.BookId);
                if (book != null)
                {
                    book.Copies += 1;
                    _bookService.Update(book);
                }
            }

            return RedirectToAction("ViewBorrowedBooks");
        }



        private void PopulateDropdowns(BorrowViewModel model)
        {
            model.Persons = _personService.GetAll().Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FullName
            });

            model.Books = _bookService.GetAll().Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Title
            });

            model.Categories = _categoryService.GetAll().Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

        }
    }
}