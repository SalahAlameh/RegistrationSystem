namespace RegistrationSystem.Models
{
    public class ArchiveViewModel
    {
        public List<Person> Persons { get; set; } = new(); // stores all archived persons for the Archive page

        public List<Book> Books { get; set; } = new(); // stores all archived books for the Archive page

        public List<Author> Authors { get; set; } = new(); // stores all archived authors for the Archive page

        public List<Category> Categories { get; set; } = new(); // stores all archived categories for the Archive page
    }
}