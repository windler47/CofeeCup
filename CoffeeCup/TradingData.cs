using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows;
using System.ComponentModel;


namespace CoffeeCup
{
    public class Customer : INotifyPropertyChanged
    {
        string customerAltName;
        string customerCity;
        string customerRegion;
        bool customerIsUploaded;
        public string City {
            get { return customerCity; }
            set {
                if (value != this.customerCity) {
                    this.customerCity = value;
                    NotifyPropertyChanged("City");
                }
            } 
        }
        public string Region {
            get { return customerRegion; }
            set {
                if (value != this.customerRegion) {
                    this.customerRegion = value;
                    NotifyPropertyChanged("Region");
                }
            } 
        }
        public string Name { get; private set; }
        public string altName {
            get { return this.customerAltName; }
            set {
                if (value != this.customerAltName) {
                    this.customerAltName = value;
                    NotifyPropertyChanged("altName");
                }
            } 
        }
        public bool IsUploaded {
            get { return this.customerIsUploaded; }
            set {
                if (value != this.customerIsUploaded) {
                    this.customerIsUploaded = value;
                    NotifyPropertyChanged("IsUploaded");
                }
            } 
        }
        public Customer(string name)
        {
            Name = name;
            altName = string.Empty;
            City = string.Empty;
            Region = string.Empty;
            IsUploaded = true;
        }
        public Customer(string name, string city, string region )
        {
            Name = name;
            altName = string.Empty;
            City = city;
            Region = region;
            IsUploaded = true;
        }
        
        private Customer() { }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class Product : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public int CupsuleMult {
            get { return this.pCupsuleMult; }
            set {
                if (value != this.pCupsuleMult) {
                    this.pCupsuleMult = value;
                    NotifyPropertyChanged("CupsuleMult");
                }
            }
        }
        public int MachMult {
            get { return this.pMachMult; }
            set {
                if (value != this.pMachMult) {
                    this.pMachMult = value;
                    NotifyPropertyChanged("MachMult");
                }
            }
        }
        public bool IsUploaded {
            get { return this.prodIsUploaded; }
            set {
                if (value != this.prodIsUploaded) {
                    this.prodIsUploaded = value;
                    NotifyPropertyChanged("IsUploaded");
                }
            }
        }
        public Product(string name)
        {
            Name = name;
            CupsuleMult = 1;
        }
        private Product() { }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        int pCupsuleMult;
        int pMachMult;
        bool prodIsUploaded;
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
