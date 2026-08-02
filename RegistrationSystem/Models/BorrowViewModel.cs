// consider this class as a delivery box that travels between the controller and the view
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // gives access to use SelectListItem

namespace RegistrationSystem.Models
{
    public class BorrowViewModel
    {
        [Required(ErrorMessage = "Please select a person.")] // basically saying "this cant be left empty"
        [Display(Name = "Person")] // shows the label "Person" in the view
        public int PersonId { get; set; } // this and the BookId are what the user is choosing (what actually gets saved)

        // true for an Admin who may select any Person.
        // false for a regular User whose Person is selected automatically.
        public bool CanChoosePerson { get; set; }

        // displays the linked Person's name when a regular User borrows.
        public string? BorrowingPersonName { get; set; }

        // false when the logged-in account has no active linked Person.
        public bool CanBorrow { get; set; } = true;

        [Required(ErrorMessage = "Please select a book.")]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        public int? CategoryId { get; set; }


        public IEnumerable<SelectListItem> Persons { get; set; } = new List<SelectListItem>(); // this is the data needed to build the dropdowns
        public IEnumerable<SelectListItem> Books { get; set; } = new List<SelectListItem>();   // this is the data needed to build the dropdowns
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        // these 3 are used to replace ViewBag.Person and ViewBag.Book
        // SelectListItem is small built in class with two things inside: Value and Text
        // IEnumerable is like List but much more simple and lightweight, it is used when you just need to iterate over a collection of items
    }
}