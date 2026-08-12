using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    public class Category : IAuditableEntity
    {
        public int Id { get; set; }

        public string CreatedBy { get; set; } = string.Empty; // email of the logged-in user who created this category
        public DateTime CreatedAt { get; set; } // date and time when this category was created
        public string? UpdatedBy { get; set; } // email of the user who last updated this category; null means it has never been updated
        public DateTime? UpdatedAt { get; set; } // date and time when this category was last updated; null means it has never been updated

        public bool IsDeleted { get; set; } = false; // false means the category is active; true means the category is archived
        public DateTime? DeletedAt { get; set; } // stores when the category was archived; null means it has never been archived

        [Display(Name = "CategoryNameLabel")]
        [Required(ErrorMessage = "CategoryNameRequired")]
        [StringLength(
            50,
            ErrorMessage = "CategoryNameMaximumLength")]
        public string Name { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
