using RegistrationSystem.Data;
using RegistrationSystem.Models;

namespace RegistrationSystem.Services
{
    public class PersonService
    {
        private readonly ApplicationDbContext _context;

        public PersonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Register(Person person)
        {
            _context.Persons.Add(person);
            _context.SaveChanges();
        }

        public List<Person> GetAll() // here we removed the static that was once present in the Person.cs since its not needed here because of the context of the class and its methods
        {
            return _context.Persons
                .Where(person => !person.IsDeleted)
                .ToList();
        }

        public List<Person> GetArchived()
        {
            return _context.Persons
                .Where(person => person.IsDeleted)
                .OrderByDescending(person => person.DeletedAt)
                .ToList();
        }

        // identity knows the logged in account by its string ID
        // this method searches Persons.IdentityUserId for that ID
        // it also excludes archived persons
        public Person? GetByIdentityUserId(
    string? identityUserId)
        {
            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return null;
            }

            return _context.Persons.FirstOrDefault(person =>
                person.IdentityUserId == identityUserId &&
                !person.IsDeleted);
        }

        public Person? GetById(int id) // this looks up a single row by primary key, it return Person?(nullable) because the id might not exist - a caller has to check for null before using it.
        {                              // so the ? is just saying that this may return either a person or a null as well
            return _context.Persons
                .FirstOrDefault(person =>
                    person.Id == id &&
                    !person.IsDeleted);
        }

        public void Update(Person person)
        {
            var existingPerson = _context.Persons.Find(person.Id);

            if (existingPerson != null)
            {
                existingPerson.FullName = person.FullName;
                existingPerson.Age = person.Age;
                existingPerson.Gender = person.Gender;
                existingPerson.Major = person.Major;

                _context.SaveChanges(); // ApplicationDbContext automatically fills UpdatedBy and UpdatedAt
            }
        }

        public void Delete(int id)
        {
            var person = _context.Persons.Find(id);

            if (person != null)
            {
                person.IsDeleted = true;
                person.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }

        public void Restore(int id)
        {
            var person = _context.Persons.Find(id);

            if (person != null)
            {
                person.IsDeleted = false;
                person.DeletedAt = null;
                _context.SaveChanges();
            }
        }
    }
}