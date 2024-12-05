using System;
using System.Collections.Generic;
using System.Linq;

namespace CornerStore.Models.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }

        public int CashierId { get; set; }
        public Cashier Cashier { get; set; }

        public DateTime? PaidOnDate { get; set; }

        public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

        public decimal Total
        {
            get
            {
                return OrderProducts.Aggregate(0M, (total, op) =>
                {
                    decimal itemTotal = op.Quantity * op.Product.Price;
                    total += itemTotal;
                    return total;
                });
            }
        }
    }
}
