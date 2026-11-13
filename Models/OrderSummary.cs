using System;

namespace RestaurantPOS.Models
{
    /// <summary>
    /// A lightweight view of a saved order, used in the daily sales report
    /// and the admin sales management screen.
    /// </summary>
    public class OrderSummary
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CashierUsername { get; set; }
    }
}
