using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RegistrationSystem.Models
{
    public class Person : IAuditableEntity // this is an enheritence relationship, Person inherits from IAuditableEntity, which means Person is a type of IAuditableEntity
    {
        public int Id { get; set; } // primary key by default (always like this in SQL) auto increment behaviour in SQL server

        public string CreatedBy { get; set; } = string.Empty; // email of the logged-in user who created this person
        public DateTime CreatedAt { get; set; } // date and time when this person was created
        public string? UpdatedBy { get; set; } // email of the user who last updated this person; null means it has never been updated
        public DateTime? UpdatedAt { get; set; } // date and time when this person was last updated; null means it has never been updated

        public bool IsDeleted { get; set; } = false; // false means the person is active; true means the person is archived
        public DateTime? DeletedAt { get; set; } // stores when the person was archived; null means it has never been archived

        // Stores the ID of the AspNetUsers account linked to this person.
        // It is nullable because existing people may not have login accounts yet.
        public string? IdentityUserId { get; set; }

        // Navigation property that gives access to the linked Identity account.
        public IdentityUser? UserAccount { get; set; }



        [Required] // basically mean NOT NULL in SQL and mandatory
        [StringLength(100)] // same thing as VARCHAR(100) in SQL
        public string FullName { get; set; }

        [Required]
        [Range(1, 120)] // this stops users from entering unreasonable ages, caps it between 1 and 120
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [StringLength(100)]
        public string Major { get; set; }

        // Persistence logic lives on the model (fat model, skinny controller)


    }
}