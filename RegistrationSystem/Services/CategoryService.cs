using RegistrationSystem.Data;
using RegistrationSystem.Models;

namespace RegistrationSystem.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> GetAll()
        {
            return _context.Categories
                .Where(category => !category.IsDeleted)
                .ToList();
        }

        public Category? GetById(int id)
        {
            return _context.Categories
                .FirstOrDefault(category =>
                    category.Id == id &&
                    !category.IsDeleted);
        }
        public void Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void Update(Category category)
        {
            var existingCategory = _context.Categories.Find(category.Id);

            if (existingCategory != null)
            {
                existingCategory.Name = category.Name;

                _context.SaveChanges(); // ApplicationDbContext automatically fills UpdatedBy and UpdatedAt
            }
        }

        public void Delete(int id)
        {
            var category = _context.Categories.Find(id);

            if (category != null)
            {
                category.IsDeleted = true;
                category.DeletedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public List<Category> GetArchived()
        {
            return _context.Categories
                .Where(category => category.IsDeleted)
                .OrderByDescending(category => category.DeletedAt)
                .ToList();
        }

        public void Restore(int id)
        {
            var category = _context.Categories.Find(id);

            if (category != null)
            {
                category.IsDeleted = false;
                category.DeletedAt = null;
                _context.SaveChanges();
            }
        }
    }
}