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
        //public double NDS;
    }
    public class Realization
    {
        public Customer Buyer;
        public DateTime Date;
        public List<SellingPosition> SellingPositions;
        public Realization() {
            SellingPositions = new List<SellingPosition>();
        }
    }
    public class CellAddress {
        public uint Row;
        public uint Col;
        public string IdString;
        public CellAddress(uint row, uint col) {
            this.Row = row;
            this.Col = col;
            this.IdString = string.Format("R{0}C{1}", row, col);
        }
    }
    class CellSameAddress : EqualityComparer<CellAddress>
{

        public override bool Equals(CellAddress c1, CellAddress c2)
	{
		if (c1.Col == c2.Col && c1.Row == c2.Row)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


        public override int GetHashCode(CellAddress c)
	{
        int hCode = Convert.ToInt32(c.Col.ToString() + c.Row.ToString());
		return hCode.GetHashCode();
	}

}
}
