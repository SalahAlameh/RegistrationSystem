using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationSystem.Models;
using RegistrationSystem.Services; // here we removed the .Data and replaced it with .Services since we are now using the PersonService class instead of the ApplicationDbContext class directly

namespace RegistrationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PersonController : Controller
    {
        private readonly PersonService _personService;

        public PersonController(PersonService personService)

        {
            _personService = personService;
        }

        // GET: /Person/Register
        [HttpGet]
        public IActionResult Register() // this action doesnt touch the database, just dispays a blank form for the user to fill in, nothing to save of fetch yet
        {
            return View();
        }

        // POST: /Person/Register
        [HttpPost]
        public IActionResult Register(Person person) //this method takes a person unlike the one above it that takes nothing (altho they have the same name)
        {
            if (!ModelState.IsValid) //Modelstate this where the core checks the data annotations 
            {
                return View(person); // send the user back to same refistration form if validation fails
            }

            _personService.Register(person);
            return RedirectToAction("Index");
        }

        // GET: /Person/Index
        [HttpGet]
        public IActionResult Index()
        {
            var allPersons = _personService.GetAll();
            return View(allPersons);
        }
        // also removed the anything related to the ApplicationDbContext class since we are now using the PersonService class instead

        // GET: /Person/Edit/5 (the number 5 is basically an example of the id of the person we want to edit, its also needed here since the IActionResult methos need the id parameter)
        [HttpGet] // this is when we get the form to edit a person
        public IActionResult Edit(int id)
        {
            var person = _personService.GetById(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: /Person/Edit
        [HttpPost] // this is when we submit the form to edit the person
        public IActionResult Edit(Person person)
        {
            if (!ModelState.IsValid)
            {
                return View(person);
            }

            _personService.Update(person);
            return RedirectToAction("Index");
        }

        // POST: /Person/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _personService.Delete(id);
            return RedirectToAction("Index");
        }

        // GET: /Person/Archive
        [HttpGet]
        public IActionResult Archive()
        {
            return RedirectToAction("Index", "Archive"); // sends old Person archive links to the one combined Archive page
        }

        // POST: /Person/Restore/5
        [HttpPost]
        public IActionResult Restore(int id)
        {
            _personService.Restore(id);
            return RedirectToAction("Index", "Archive"); // stays on the combined Archive page after restoring
        }
    }
}