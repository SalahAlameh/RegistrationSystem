using Microsoft.EntityFrameworkCore;
using RegistrationSystem.Data;
using RegistrationSystem.Models;
using System.Linq;

namespace RegistrationSystem.Services
{
    public class BorrowService
    {
        private readonly ApplicationDbContext _context;

        public BorrowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Borrow(Borrow borrow)
        {
            _context.Borrows.Add(borrow);
            _context.SaveChanges();
        }

        public List<Borrow> GetAll()
        {
            return _context.Borrows
                .Include(b => b.Person)
                .Include(b => b.Book)
                .ToList();
        }

        public int GetActiveBorrowCountForPerson(int personId) // helper method to count how many active borrows a person has (ReturnDate == null means still borrowed)
        {
            return _context.Borrows.Count(b => b.PersonId == personId && b.ReturnDate == null);
        }

        public Borrow? GetById(int id) // looks up the row by primary key and returns it if it exists, otherwise returns null
        {
            return _context.Borrows.Find(id);
        }

        public void ReturnBook(Borrow borrow) // takes the already-fetched borrow object (not just id), because .Find() in the line above return a tracked entity
        {
            borrow.ReturnDate = DateTime.Now; // changes the return date to now
            _context.SaveChanges();
        }
    }
}