using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RegistrationSystem.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Display(Name = "BookTitleLabel")]
        [Required(ErrorMessage = "BookTitleRequired")]
        [StringLength(
            150,
            ErrorMessage = "BookTitleMaximumLength")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "CopiesLabel")]
        [Range(
            0,
            1000,
            ErrorMessage = "CopiesRange")]
        public int Copies { get; set; }

        [Display(Name = "CategoryLabel")]
        public int? CategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();


        [Display(Name = "AuthorsLabel")]
        [Required(ErrorMessage = "AuthorSelectionRequired")]
        [MinLength(
            1,
            ErrorMessage = "AuthorSelectionRequired")]
        public List<int> SelectedAuthorIds { get; set; } = new(); // SelectedAuthorsIds stores the IDs of authors selected in the form - List<int> means multiple integer IDs can be stored

        public IEnumerable<SelectListItem> Authors { get; set; } = new List<SelectListItem>();
        // Authors is used to populate the dropdown list in the form - IEnumerable<SelectListItem> means it can be enumerated (looped through) and each item is a SelectListItem, which has a Text and Value property for display and submission
    }
}