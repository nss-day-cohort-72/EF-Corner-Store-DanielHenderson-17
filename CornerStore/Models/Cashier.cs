using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models
{
    public class Cashier
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
