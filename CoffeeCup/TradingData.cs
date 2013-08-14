using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows;


namespace CoffeeCup
{
    public class Customer
    {
        public string City { get; set; }
        public string Region { get; set; }
        public string Name { get; private set; }
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
        private Customer() { }
    }
    public class Product
    {
        public string Name { get; private set; }
        public int CupsuleMult { get; set; }
        public int MachMult { get; set; }
        public bool IsUploaded { get; set; }
        public Product(string name)
        {
            Name = name;
            CupsuleMult = 1;
        }
        private Product() { }
    }
    public class SellingPosition
    {
        public Product Product;
        public int Amount;
        public double Price;
        public double NDS;
    }
    public class Realization
    {
        public Customer Buyer;
        public DateTime Date;
        public List<SellingPosition> SellingPositions;
    }
}
