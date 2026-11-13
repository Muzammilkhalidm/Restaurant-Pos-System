namespace RestaurantPOS.Models
{
    /// <summary>
    /// Represents a single line item in the current order/cart.
    /// </summary>
    public class OrderItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}
