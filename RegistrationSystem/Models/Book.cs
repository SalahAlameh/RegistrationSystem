using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    public class Book : IAuditableEntity
    {
        public int Id { get; set; }

        public string CreatedBy { get; set; } = string.Empty; // email of the logged-in user who created this book
        public DateTime CreatedAt { get; set; } // date and time when this book was created
        public string? UpdatedBy { get; set; } // email of the user who last updated this book; null means it has never been updated
        public DateTime? UpdatedAt { get; set; } // date and time when this book was last updated; null means it has never been updated

        public bool IsDeleted { get; set; } = false; // false means the book is active; true means the book is archived
        public DateTime? DeletedAt { get; set; } // stores when the book was archived; null means it has never been archived

        [Display(Name = "BookTitleLabel")]
        [Required(ErrorMessage = "BookTitleRequired")]
        [StringLength(
            150,
            ErrorMessage = "BookTitleMaximumLength")]
        public string Title { get; set; }


        [Display(Name = "CopiesLabel")]
        [Required]
        [Range(
            0,
            1000,
            ErrorMessage = "CopiesRange")]
        public int Copies { get; set; }

        public int? CategoryId { get; set; } // foreign key to Category, optional because a book may not have a category (by optional i mean ?)
        public Category? Category { get; set; } //navigation property, also nullable to match. This is what lets you write book.Category.Name in code/views once it's assigned

        public ICollection<Author> Authors { get; set; } = new List<Author>();
        // mirror image of what we just added to Author.cs - one book, many authors.
        // having Book.Authors and Author.Books pointing to each other, with no third class in between, tells the EF Core " this is a many to many" it automatically builds a hidden koun table behind the scenes to make that work in a relational databse


    }
}                                                  
