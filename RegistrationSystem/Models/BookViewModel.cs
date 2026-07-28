using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RegistrationSystem.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Range(0, 1000, ErrorMessage = "Copies must be between 0 and 1000.")]
        public int Copies { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();


        [Display(Name = "Authors")]
        [MinLength(1, ErrorMessage = "Please select at least one author.")] // the 1 means its required at least one author to be selected
        public List<int> SelectedAuthorIds { get; set; } = new(); // SelectedAuthorsIds stores the IDs of authors selected in the form - List<int> means multiple integer IDs can be stored

        public IEnumerable<SelectListItem> Authors { get; set; } = new List<SelectListItem>();
        // Authors is used to populate the dropdown list in the form - IEnumerable<SelectListItem> means it can be enumerated (looped through) and each item is a SelectListItem, which has a Text and Value property for display and submission
    }
}