using RegistrationSystem.Data;
using RegistrationSystem.Models;

namespace RegistrationSystem.Services
{
    public class AuthorService
    {
        private readonly ApplicationDbContext _context;

        public AuthorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Author> GetAll() // retrieves all active authors
        {
            return _context.Authors
                .Where(author => !author.IsDeleted)
                .OrderBy(author => author.FullName)
                .ToList();
        }

        public Author? GetById(int id) // retrieves 1 active author by id
        {
            return _context.Authors
                .FirstOrDefault(author =>
                    author.Id == id &&
                    !author.IsDeleted);
        }

        public void Create(Author author) // inserts a new author
        {
            _context.Authors.Add(author);
            _context.SaveChanges();
        }

        public bool Update(Author author) // finds existing author by id and updates the author if found, returns true if updated, false if not found
        {
            var existingAuthor = _context.Authors.Find(author.Id);

            if (existingAuthor == null)
            {
                return false;
            }

            existingAuthor.FullName = author.FullName;
            _context.SaveChanges();

            return true;
        }

        public bool Delete(int id) // archives the author instead of physically deleting it
        {
            var author = _context.Authors.Find(id);

            if (author == null)
            {
                return false;
            }

            author.IsDeleted = true;
            author.DeletedAt = DateTime.Now;
            _context.SaveChanges();

            return true;
        }

        public List<Author> GetArchived()
        {
            return _context.Authors
                .Where(author => author.IsDeleted)
                .OrderByDescending(author => author.DeletedAt)
                .ToList();
        }

        public void Restore(int id)
        {
            var author = _context.Authors.Find(id);

            if (author != null)
            {
                author.IsDeleted = false;
                author.DeletedAt = null;
                _context.SaveChanges();
            }
        }
    }
}