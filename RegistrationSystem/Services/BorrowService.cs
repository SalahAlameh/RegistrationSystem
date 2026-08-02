using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegistrationSystem.Data;
using RegistrationSystem.Models;
using System.Linq;
using System.Security.Claims;

namespace RegistrationSystem.Services
{
    public class BorrowService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BorrowService(ApplicationDbContext context,UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Person? GetLinkedPerson(ClaimsPrincipal currentUser) // ClaimsPrincipal (authenticated visitor) is the current logged-in user, and this method looks up the Person entity linked to that user
        {
            var identityUserId =
                _userManager.GetUserId(currentUser); // UserManager reads the Identity account ID from that visitor

            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return null;
            }

            return _context.Persons.FirstOrDefault(person =>
                person.IdentityUserId == identityUserId &&
                !person.IsDeleted);
        }

        public BorrowOperationResult BorrowBook(
    int requestedPersonId, // this is used for Admins to select a Person from the form, but regular Users will have this ignored and their linked Person will be used instead
    int bookId,
    ClaimsPrincipal currentUser)
        {
            Person? person;

            if (currentUser.IsInRole("Admin"))
            {
                // An Admin may borrow for the Person selected
                // from the borrowing form.
                person = _context.Persons.FirstOrDefault(person =>
                    person.Id == requestedPersonId &&
                    !person.IsDeleted);

                if (person == null)
                {
                    return BorrowOperationResult.Failed(
                        "The selected person does not exist or is archived.");
                }
            }
            else
            {
                // A regular User must borrow only for the Person
                // connected to their logged-in Identity account.
                person = GetLinkedPerson(currentUser);

                if (person == null)
                {
                    return BorrowOperationResult.Failed(
                        "Your account is not linked to an active person. Contact an administrator before borrowing.");
                }
            }

            var book = _context.Books.FirstOrDefault(book =>
                book.Id == bookId &&
                !book.IsDeleted);

            if (book == null)
            {
                return BorrowOperationResult.Failed(
                    "The selected book does not exist or is archived.");
            }

            if (book.Copies < 1)
            {
                return BorrowOperationResult.Failed(
                    "This book is out of stock and cannot be borrowed.");
            }

            int activeBorrows =
                GetActiveBorrowCountForPerson(person.Id);

            if (activeBorrows >= 3)
            {
                return BorrowOperationResult.Failed(
                    "This person already has 3 active borrows and cannot borrow more until one is returned.");
            }

            var borrowDate = DateTime.UtcNow;

            var borrow = new Borrow // only reached if everything passed, builds a real Borrow entity from the two IDs
            {
                PersonId = person.Id,
                BookId = bookId,
                BorrowDate = borrowDate
            };

            // the service calculates the due date so every borrow operation
            // follows the same fourteen-day business rule.
            borrow.DueDate = borrowDate.AddDays(
                RegistrationSystem.Models.Borrow.LoanDurationDays);

            _context.Borrows.Add(borrow);
            book.Copies -= 1;

            // The Borrow insert and Book copy update are committed together.
            _context.SaveChanges();

            return BorrowOperationResult.Success();
        }

        public List<Borrow> GetAll(
    ClaimsPrincipal currentUser)
        {
            var query = _context.Borrows
                .Include(borrow => borrow.Person)
                .Include(borrow => borrow.Book)
                .AsQueryable();

            if (currentUser.IsInRole("Admin"))
            {
                // Admins may see every Person's borrowing history.
                return query
                    .OrderByDescending(borrow =>
                        borrow.BorrowDate)
                    .ToList();
            }

            var identityUserId =
                _userManager.GetUserId(currentUser);

            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return new List<Borrow>();
            }

            // Regular Users see only records belonging to
            // the Person linked to their Identity account.
            return query
                .Where(borrow =>
                    borrow.Person != null &&
                    borrow.Person.IdentityUserId ==
                        identityUserId &&
                    !borrow.Person.IsDeleted)
                .OrderByDescending(borrow =>
                    borrow.BorrowDate)
                .ToList();
        }

        public int GetActiveBorrowCountForPerson(int personId) // helper method to count how many active borrows a person has (ReturnDate == null means still borrowed)
        {
            return _context.Borrows.Count(b => b.PersonId == personId && b.ReturnDate == null);
        }

        public Borrow? GetById(int id) // looks up the row by primary key and returns it if it exists, otherwise returns null
        {
            return _context.Borrows
                .Include(borrow => borrow.Person)
                .Include(borrow => borrow.Book)
                .FirstOrDefault(borrow => borrow.Id == id);
        }

        public BorrowOperationResult ReturnBook(
    int id, // int id is the borrow record being returned, not the book's id or the person's id
    ClaimsPrincipal currentUser) 
        {
            var borrow = GetById(id);

            if (borrow == null)
            {
                return BorrowOperationResult.NotFoundResult(
                    "The borrow record does not exist.");
            }

            if (!currentUser.IsInRole("Admin"))
            {
                var identityUserId =
                    _userManager.GetUserId(currentUser);

                var ownsBorrow =
                    !string.IsNullOrWhiteSpace(identityUserId) &&
                    borrow.Person?.IdentityUserId ==
                        identityUserId;

                if (!ownsBorrow)
                {
                    // NotFound avoids revealing whether another
                    // Person's Borrow record exists.
                    return BorrowOperationResult.NotFoundResult(
                        "The borrow record does not exist.");
                }
            }

            if (borrow.ReturnDate != null)
            {
                return BorrowOperationResult.Failed(
                    "This book has already been returned.");
            }

            if (borrow.Book == null)
            {
                return BorrowOperationResult.Failed(
                    "The book connected to this borrow record could not be found.");
            }

            // The old method took the already-fetched Borrow object because
            // Find returned a tracked entity. GetById now loads and tracks
            // both the Borrow and its related Book for this complete workflow.
            borrow.ReturnDate = DateTime.UtcNow; // changes the return date to now
            borrow.Book.Copies += 1;

            // The ReturnDate and Book copy update are committed together.
            _context.SaveChanges();

            return BorrowOperationResult.Success();
        }
    }
}
