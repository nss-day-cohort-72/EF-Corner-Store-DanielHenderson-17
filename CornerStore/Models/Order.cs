using System;
using System.Collections.Generic;

namespace CornerStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CashierId { get; set; }
        public Cashier Cashier { get; set; }
        public DateTime? PaidOnDate { get; set; }
        public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
