using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<SharedResource> _localizer;

        public BorrowController(
            BorrowService borrowService,
            PersonService personService,
            BookService bookService,
            CategoryService categoryService,
            IStringLocalizer<SharedResource> localizer)
        {
            _borrowService = borrowService;
            _personService = personService;
            _bookService = bookService;
            _categoryService = categoryService;
            _localizer = localizer;
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
        [ValidateAntiForgeryToken]
        public IActionResult BorrowBook(BorrowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            var result = _borrowService.BorrowBook(
                model.PersonId!.Value,
                model.BookId!.Value,
                User); // User is the authenticated ClaimsPrincipal provided by the base MVC controller

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    _localizer[
                        result.ErrorKey ??
                        "BorrowFailed"].Value);

                PopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] =
                _localizer["BorrowSucceeded"].Value;

            return RedirectToAction(nameof(ViewBorrowedBooks));
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
            var allBorrows = _borrowService.GetAll(User);
            return View(allBorrows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReturnBook(int id) // int id is the borrow record being returned, not the bokk's id or the person's id
        {
            var result = _borrowService.ReturnBook(id,User);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] =
                    _localizer[
                        result.ErrorKey ??
                        "ReturnFailed"].Value;

                return RedirectToAction(nameof(ViewBorrowedBooks));
            }

            TempData["SuccessMessage"] =
                _localizer["ReturnSucceeded"].Value;

            return RedirectToAction(nameof(ViewBorrowedBooks));
        }



        private void PopulateDropdowns(BorrowViewModel model)
        {
            model.CanChoosePerson =
                User.IsInRole("Admin");

            if (model.CanChoosePerson)
            {
                // Admins may choose any active Person.
                model.Persons = _personService.GetAll()
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.FullName
                    });

                model.CanBorrow = true;
            }
            else
            {
                // Regular Users borrow only for the Person linked
                // to their logged-in Identity account.
                var linkedPerson =
                    _borrowService.GetLinkedPerson(User);

                model.Persons =
                    Enumerable.Empty<SelectListItem>();

                if (linkedPerson == null)
                {
                    model.PersonId = 0;
                    model.BorrowingPersonName = null;
                    model.CanBorrow = false;
                }
                else
                {
                    model.PersonId = linkedPerson.Id;
                    model.BorrowingPersonName =
                        linkedPerson.FullName;

                    model.CanBorrow = true;
                }
            }

            model.Books = _bookService.GetAll()
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Title
                });

            model.Categories = _categoryService.GetAll()
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
        }
    }
}
