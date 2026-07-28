// this tells ApplicationDbContext: "every class that implements this interface has audit info that must be filled automatically"
namespace RegistrationSystem.Models
{
    public interface IAuditableEntity
    {
        string CreatedBy { get; set; } // email of the user who created this record

        DateTime CreatedAt { get; set; } // date and time when this record was created

        string? UpdatedBy { get; set; } // email of the user who last changed this record; null when it has never been updated

        DateTime? UpdatedAt { get; set; } // date and time of the last update; null when it has never been updated
    }
}