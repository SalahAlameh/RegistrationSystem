// this is an associative entity since we cant have a many-to-many relationship between Person and Book
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationSystem.Models
{
    public class Borrow : IAuditableEntity
    {
        public const int LoanDurationDays = 14; // every borrow is due fourteen days after it is created
        public int Id { get; set; }

        public string CreatedBy { get; set; } = string.Empty; // email of the logged-in user who created this borrow record
        public DateTime CreatedAt { get; set; } // date and time when this borrow record was created
        public string? UpdatedBy { get; set; } // email of the user who last updated this borrow record; null means it has never been updated
        public DateTime? UpdatedAt { get; set; } // date and time when this borrow record was last updated; null means it has never been updated
        // Foreign key to Person
        public int PersonId { get; set; }

        
        [ForeignKey("PersonId")]
        public Person? Person { get; set; } // navigation property — lets you write borrow.Person.FullName later // instead of separately querying Persons where Id == borrow.PersonId we can do borrow.Person.FullName
                                                                                                                 //   
        // Foreign key to Book                                                                                   //
        public int BookId { get; set; }                                                                          //
                                                                                                                 // a way to access the related row's full data once loaded   
                                                                                                                 //
        [ForeignKey("BookId")]                                                                                   //
        public Book? Book { get; set; } // navigation property — lets you write borrow.Book.Title later          // instead of separately querying Books where Id == borrow.BookId we can do borrow.Book.Title

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; } // calculated as BorrowDate plus the fourteen-day loan period

        public DateTime? ReturnDate { get; set; } // null = still borrowed (active); has a value = returned
    }
}