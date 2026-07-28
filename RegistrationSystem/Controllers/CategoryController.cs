using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Models;
using RegistrationSystem.Services;

namespace RegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _categoryService.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _categoryService.Add(category);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _categoryService.Update(category);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _categoryService.Delete(id);
            return RedirectToAction("Index");
        }

        // GET: /Category/Archive
        [HttpGet]
        public IActionResult Archive()
        {
            return RedirectToAction("Index", "Archive"); // sends old Book archive links to the one combined Archive page
        }

        // POST: /Category/Restore/5
        [HttpPost]
        public IActionResult Restore(int id)
        {
            _categoryService.Restore(id); // restores the archived category

            return RedirectToAction("Index", "Archive"); // stays on the combined Archive page after restoring
        }
    }
}