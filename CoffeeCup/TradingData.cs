using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeCup
{
    public class Customer
    {
        public string City { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public bool IsUploaded { get; set; }
        public Customer(string name)
        {
            Name = name;
            IsUploaded = true;
        }
        public Customer(string name, string city, string region )
        {
            Name = name;
            City = city;
            Region = region;
            IsUploaded = true;
        }
    }
    public class Product
    {
        public string Name { get; set; }
        public bool IsUploaded { get; set; }
        public Product(string name)
        {
            Name = name;
        }
    }
    public class SellingPosition
    {
        public Product Product;
        public int Number;
        public double Price;
        public bool NDS;
    }
    public class Realization
    {
        public Customer Buyer;
        public DateTime Date;
        public List<SellingPosition> SellingPositions;
    }
}
