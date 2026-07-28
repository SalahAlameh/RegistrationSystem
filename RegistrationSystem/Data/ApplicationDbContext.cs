// how it actually connects to the database
using Microsoft.AspNetCore.Http; // gives access to IHttpContextAccessor, which is used to get the currently logged-in user's email address
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RegistrationSystem.Models;

namespace RegistrationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext // previously this class used DbContext now it uses IdenttityDbContext, this knows how to create the tables required for users, passwords, login sessions, and roles.
    {
        private readonly IHttpContextAccessor _httpContextAccessor; // this is used to get the currently logged-in user's email address, which is used to fill in the CreatedBy and UpdatedBy fields in the database

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor; // stores the received accessor in the private field
        } // for this to work, Program.cs must have builder.Services.AddHttpContextAccessor(); in it, which it does

        public DbSet<Person> Persons { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }

        public override int SaveChanges(bool acceptAllChangesOnSuccess) // we normally call SaveChanges() to save changes to the database, but this override lets us add our own code to fill in CreatedBy/CreatedAt or UpdatedBy/UpdatedAt before EF Core saves changes to SQL Server
        {
            ApplyAuditInformation(); // fills in CreatedBy/CreatedAt or UpdatedBy/UpdatedAt before EF Core saves changes to SQL Server

            return base.SaveChanges(acceptAllChangesOnSuccess); // this performs the real save, base means the original SaveChanges
        }

        private void ApplyAuditInformation() // helper method that organizes all audit logic in one place
        {   //                                          web request|user|login info|email  the ?. means continue only if this object exists
            var currentUserEmail = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"; // System means if no user email was found, use System instead
            // tries to get the logged-in email; System is used only if there is no logged-in user, such as during database setup

            var currentDateTime = DateTime.UtcNow;
            // ChangeTracker is an EF Core feature that keeps track of all the entities that have been added, modified, or deleted in the current context. It allows us to inspect and manipulate these entities before they are saved to the database.
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>()) // find all entities that support auditing
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = currentUserEmail;
                    entry.Entity.CreatedAt = currentDateTime;

                    entry.Entity.UpdatedBy = null;
                    entry.Entity.UpdatedAt = null;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedBy = currentUserEmail;
                    entry.Entity.UpdatedAt = currentDateTime;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // a special mehtod EF Core calls once when its figuring out your database structure.
        {
            base.OnModelCreating(modelBuilder); // required so Identity can configure its own database tables, such as AspNetUsers, AspNetRoles, and AspNetUserRoles

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors) // a book has many authors
                .WithMany(a => a.Books)  // and an author has many books
                .UsingEntity(j => j.ToTable("BookAuthors")); // wihtouht this line EF Core would still build a working koun table just with an auto generated name like AuthorBook. This line tells EF Core to use the name BookAuthors instead.

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category) // a book has one category
                .WithMany(c => c.Books) // a category has many books
                .HasForeignKey(b => b.CategoryId) // the foreign key in the Book table that points to the Category table
                .OnDelete(DeleteBehavior.Restrict); //  By default, deleting a Category that still has books assigned to it would either cascade-delete those books (bad) or silently set their CategoryId back to null (sneaky). Restrict instead makes SQL Server block the delete entirely with an error if any books still reference that category — forcing you to consciously reassign or remove those books first, rather than losing data by accident.

            modelBuilder.Entity<Person>()
                .HasOne(person => person.UserAccount) // one person may have one linked Identity account
                .WithOne() // the IdentityUser class does not need a reverse Person property
                .HasForeignKey<Person>(person => person.IdentityUserId) // IdentityUserId is stored in the Persons table
                .OnDelete(DeleteBehavior.SetNull); // if an Identity account is ever removed, keep the Person and clear only the link
        }
    }
}