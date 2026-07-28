using RegistrationSystem.Data;
using RegistrationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace RegistrationSystem.Services
{
    public class BookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }

        public List<Book> GetAll() // updated this to inluce authors
        {
            return _context.Books
                .Where(book => !book.IsDeleted)
                .Include(book => book.Authors) // loads the authors together with each book
                .Include(book => book.Category) // loads the category together with each book
                .ToList();
        }

        public List<Book> GetByCategory(int? categoryId)
        {
            var query = _context.Books
                .Where(book => !book.IsDeleted) // prevents archived books from appearing after category filtering
                .Include(book => book.Category)
                .AsQueryable(); // makes the queryable so that we can add a filter later

            if (categoryId.HasValue) // only filters if a real category was picked, if not it will return all books
            {
                query = query.Where(book => book.CategoryId == categoryId.Value); // this is the actual filtering, only books whose CategoryId matches
            }

            return query.ToList();
        }

        public Book? GetById(int id) // same thing for this
        {
            return _context.Books
                .Where(book =>
                    book.Id == id &&
                    !book.IsDeleted) // this prevents archived books from being edited or borrowed
                .Include(book => book.Authors)
                .Include(book => book.Category)
                .FirstOrDefault();
        }

        public void Update(Book book)
        {
            _context.Books.Update(book);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var book = _context.Books.Find(id);

            if (book != null)
            {
                book.IsDeleted = true;
                book.DeletedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public List<Book> GetArchived()
        {
            return _context.Books
                .Where(book => book.IsDeleted)
                .Include(book => book.Authors)
                .Include(book => book.Category)
                .OrderByDescending(book => book.DeletedAt)
                .ToList();
        }

        public void Restore(int id)
        {
            var book = _context.Books.Find(id);

            if (book != null)
            {
                book.IsDeleted = false;
                book.DeletedAt = null;
                _context.SaveChanges();
            }
        }

        //                          new book        list of author ids
        public bool AddWithAuthors(Book book, IEnumerable<int> authorIds)
        {
            var ids = (authorIds ?? Enumerable.Empty<int>()) // if authorIds is null, use an empty list instead
                .Distinct() // duplicates are not allowed (removed)
                .ToList();

            var authors = _context.Authors
                .Where(author => ids.Contains(author.Id)) // searches the database for authors whose IDs match the selected IDs
                .ToList();

            if (ids.Count == 0 || authors.Count != ids.Count) // checks if author was selected and if the number of authors found in the database matches the number of selected IDs
            {
                return false; // author selection invalid
            }

            book.Authors = authors; // this assigns the real authors to the book's navigation collection

            _context.Books.Add(book);
            _context.SaveChanges();

            return true; // book was succefully saved
        }

        public bool UpdateWithAuthors(Book book, IEnumerable<int> authorIds)
        {
            var ids = (authorIds ?? Enumerable.Empty<int>())
                .Distinct()
                .ToList();

            var existingBook = _context.Books // this searches the DB for the books being edited
                .Include(existing => existing.Authors) // loads the book's current authors too
                .FirstOrDefault(existing => existing.Id == book.Id);

            if (existingBook == null) // check whether the books exists
            {
                return false;
            }

            var authors = _context.Authors // load selected authors
                .Where(author => ids.Contains(author.Id)) // if the submitted ids are 2 and 4 the result will be the authors with those ids
                .ToList();

            if (ids.Count == 0 || authors.Count != ids.Count)
            {
                return false;
            }
            // here it updates the normal book fields
            existingBook.Title = book.Title;
            existingBook.CategoryId = book.CategoryId; // Genre was replaced by CategoryId
            existingBook.Copies = book.Copies;

            existingBook.Authors.Clear(); // clears the current authors of the book so that we can add the new ones

            foreach (var author in authors) // add newly selected authors to the book's collection
            {
                existingBook.Authors.Add(author);
            }

            _context.SaveChanges();

            return true;
        }
    }
}