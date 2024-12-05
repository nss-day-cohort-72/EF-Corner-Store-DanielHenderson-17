using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models.DTOs
{
    public class CashierDTO
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();

        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}
