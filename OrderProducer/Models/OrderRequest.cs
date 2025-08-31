namespace OrderProducer.Models
{
    public class OrderRequest
    {
        //create properties for OrderRequest id, productName, quantity, price
        public int OrderId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
