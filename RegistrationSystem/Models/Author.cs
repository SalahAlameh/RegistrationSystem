using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    public class Author : IAuditableEntity
    {
        public int Id { get; set; }

        public string CreatedBy { get; set; } = string.Empty; // email of the logged-in user who created this author
        public DateTime CreatedAt { get; set; } // date and time when this author was created
        public string? UpdatedBy { get; set; } // email of the user who last updated this author; null means it has never been updated
        public DateTime? UpdatedAt { get; set; } // date and time when this author was last updated; null means it has never been updated

        public bool IsDeleted { get; set; } = false; // false means the author is active; true means the author is archived
        public DateTime? DeletedAt { get; set; } // stores when the author was archived; null means it has never been archived

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty; // EF Core will make this column NOT NULL in the database, and the [Required] attribute will make it required in the UI. The string.Empty initialization is just to avoid a compiler warning about non-nullable properties not being initialized

        public ICollection<Book> Books { get; set; } = new List<Book>(); // we didnt use IEnumerable like we used in the dropdown because EF Core needs to be able to add/remove individual books from this collection as relationships change, not just read through it once
        // navigation property, it doesnt create a column in the Authors table. instead it tells EF Core "this author is connected to a set of books"
    }
}