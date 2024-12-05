using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models.DTOs
{
    public class OrderCreateDTO
    {
        public int CashierId { get; set; }
        public DateTime? PaidOnDate { get; set; }
        public List<OrderProductDTO> Products { get; set; } = new List<OrderProductDTO>();
    }
}