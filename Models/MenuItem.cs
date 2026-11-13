namespace RestaurantPOS.Models
{
    /// <summary>
    /// Represents a single item on the restaurant's menu.
    /// </summary>
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }

        // Used so this object displays nicely when bound to a ListBox/ComboBox.
        public override string ToString() => Name;
    }
}
