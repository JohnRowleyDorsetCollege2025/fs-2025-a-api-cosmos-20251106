namespace fs_2025_a_api_cosmos_20251106.Models
{
    public class Student
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public Customer Customer { get; set; }
        public List<OrderItem> Items { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public Shipping Shipping { get; set; }
    }

    public class Customer
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }

    public class Shipping
    {
        public string Method { get; set; }
        public string TrackingNumber { get; set; }
        public bool Delivered { get; set; }
    }

}
