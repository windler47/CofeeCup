using System;
using System.Collections.Generic;
using System.ComponentModel;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using System.Linq;
using System.IO;
using System.Xml.Linq;


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
    public class CCupWSEntry : WorksheetEntry, IComparable<CCupWSEntry> {
        public Dictionary<uint,string> AddNewCustomerRow(List<Customer> newCustomers){
            Dictionary<uint,string> resultDic = new Dictionary<uint,string>();
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = spreadsheetService.Query(listQuery);
            ListEntry example = (ListEntry)listFeed.Entries[3];
            ListEntry total = (ListEntry)listFeed.Entries.Last();
            uint linenumber = (uint)listFeed.TotalResults;
            total.Delete();
            foreach (Customer customer in newCustomers) {
                spreadsheetService.Insert(listFeed, GenerateCustomerRow(customer,example));
                resultDic.Add(++linenumber,customer.Name);
                if (string.IsNullOrWhiteSpace(customer.altName)) {
                    cust_Row.Add(customer.altName, linenumber);
                }
                else {
                    cust_Row.Add(customer.Name, linenumber);
                }
            }
            spreadsheetService.Insert(listFeed, total);
            return resultDic;
        }
        public uint AddNewCustomerRow(Customer newCustomer){
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = spreadsheetService.Query(listQuery);
            ListEntry example = (ListEntry)listFeed.Entries[3];
            ListEntry total = (ListEntry)listFeed.Entries.Last();
            uint linenumber = (uint)listFeed.TotalResults;
            total.Delete();
            spreadsheetService.Insert(listFeed, GenerateCustomerRow(newCustomer,example));
            spreadsheetService.Insert(listFeed, total);
            if (string.IsNullOrWhiteSpace(newCustomer.altName)) {
                cust_Row.Add(newCustomer.altName, ++linenumber);
            }
            else {
                cust_Row.Add(newCustomer.Name, ++linenumber);
            }
            return linenumber;
        }
        ListEntry GenerateCustomerRow(Customer cust, ListEntry example) {
            /// Generate new customer row 
            string medCup = "=IFERROR(R[0]C[-3]/R[0]C[-1]*120;'Нет данных'";
            string machNum = "=R[0]C[-7]+R[0]C[-6]";
            string cupSum = "=R[0]C[3]";
            string machSum = "=R0C[-6]+R0C[-5]";
            ListEntry custRow = new ListEntry();
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[0].LocalName, Value = cust.City});
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[1].LocalName, Value = cust.Region });
            if (string.IsNullOrWhiteSpace(cust.altName)) {
                custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[2].LocalName, Value = cust.altName });
            }
            else custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[2].LocalName, Value = cust.Name });            
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[6].LocalName, Value = medCup });
            //January - try to get mah data from prev year
            if (worksheetYear!=0) {
                CCupWSEntry[] wsList= worksheetFeed.EntriesList.ToArray();
                Array.Sort(wsList);
                int maxindex = Array.FindLastIndex(wsList,ws => ws.worksheetYear < worksheetYear);
                int minindex = Array.FindIndex(wsList, ws => ws.worksheetYear > 0);
                string janMah = "";
                for (int i = maxindex; i >= minindex; i--) {
                    if (wsList[i].cust_Row.ContainsKey(cust.altName)) {
                        janMah = string.Format("=" + wsList[i].Title.Text + "!R{1}C76", wsList[i].cust_Row[cust.altName]);
                        break;
                    }
                    else if (wsList[i].cust_Row.ContainsKey(cust.Name)) {
                        janMah = string.Format("=" + wsList[i].Title.Text + "!R{1}C76", wsList[i].cust_Row[cust.Name]);
                        break;
                    }
                }
                custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[5].LocalName, Value = janMah });
            }
            for (int i = 1; i < 12; i++) {
                custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[6 + i * 6].LocalName, Value = medCup });
                custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[5 + i * 6].LocalName, Value = machNum });
                cupSum += string.Format("+R0C{0}", 3 + i * 6);
            }
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[75].LocalName, Value = cupSum });
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[76].LocalName, Value = machSum });
            return custRow;
        }
        SpreadsheetsService spreadsheetService;
        CCupWSFeed worksheetFeed;
        public Dictionary<string, uint> cust_Row;
        public uint worksheetYear;
        public CCupWSEntry(SpreadsheetsService service, CCupWSFeed feed) {
            spreadsheetService = service;
            worksheetFeed = feed;
            if (!uint.TryParse(Title.Text, out worksheetYear)) worksheetYear = 0;
            cust_Row = new Dictionary<string, uint>();
            if (Rows > 3) {
                CellQuery cellQuery = new CellQuery(CellFeedLink);
                cellQuery.MinimumRow = 3;
                cellQuery.MaximumRow = Rows - 1;
                cellQuery.MinimumColumn = 1;
                cellQuery.MaximumColumn = 1;
                CellFeed cellFeed = spreadsheetService.Query(cellQuery);
                foreach (CellEntry cell in cellFeed.Entries) {
                    cust_Row.Add(cell.InputValue, cell.Row);
                }
            }            
        }
        private CCupWSEntry() { }
        public int CompareTo(CCupWSEntry other) {
            return worksheetYear.CompareTo(other.worksheetYear);
        }
    }
    public class CCupWSFeed {
        public List<CCupWSEntry> EntriesList { get; private set; }
        public CCupWSFeed(WorksheetFeed worksheetFeed, SpreadsheetsService spreadsheetsService) {
            EntriesList = new List<CCupWSEntry>();
            iWorksheetFeed = worksheetFeed;
            iSpreadsheetService = spreadsheetsService;
            foreach (WorksheetEntry wsEntry in worksheetFeed.Entries) {
                EntriesList.Add(new CCupWSEntry(iSpreadsheetService, this));
            }
        }
        WorksheetFeed iWorksheetFeed;
        SpreadsheetsService iSpreadsheetService;
        private CCupWSFeed() { }
    }
    public class LocalDatabase {
        XElement productDatabase;
        XElement customerDatabase;
        public Dictionary<string, Customer> Customers { get; private set; }
        public Dictionary<string, Product> Products { get; private set; }
        public void UpdateDatabase(List<Customer> customerList ) {
            foreach (Customer customer in customerList) {
                if (Customers.ContainsKey(customer.Name)) {
                    UpdateEntryValue(customer);
                }
                else AddNewEntry(customer);
            }
        }
        public void UpdateDatabase(List<Product> productList) {
            foreach (Product product in productList) {
                if (Products.ContainsKey(product.Name)) UpdateEntryValue(product);
                else AddNewEntry(product);
            }
        }
        void UpdateEntryValue(Customer customer){
            Customers[customer.Name] = customer;
            XElement dCustomer = customerDatabase.Elements().Where((c) => c.Attribute("Name").Value == customer.Name).Single();
            dCustomer.Attribute("AltName").SetValue(customer.altName);
            dCustomer.Attribute("City").SetValue(customer.City);
            dCustomer.Attribute("Region").SetValue(customer.Region);
            dCustomer.Attribute("IsUploaded").SetValue(customer.IsUploaded);
        }
        void UpdateEntryValue(Product product) {
            Products[product.Name] = product;
            XElement dProduct = productDatabase.Elements().Where((p) => p.Attribute("Name").Value == product.Name).Single();
            dProduct.Attribute("IsUploaded").SetValue(product.IsUploaded);
            dProduct.Attribute("Cmult").SetValue(product.CupsuleMult);
            dProduct.Attribute("Mmult").SetValue(product.MachMult);
        }
        void AddNewEntry(Customer customer) {
            Customers[customer.Name] = customer;
            XElement dCustomer = new XElement("Customer");
            dCustomer.Add(new XAttribute("Name", customer.Name));
            dCustomer.Add(new XAttribute("AltName", customer.altName));
            dCustomer.Add(new XAttribute("IsUploaded", customer.IsUploaded));
            dCustomer.Add(new XAttribute("City", customer.City));
            dCustomer.Add(new XAttribute("Region", customer.Region));
            customerDatabase.Add(dCustomer);
        }
        void AddNewEntry(Product product) {
            Products[product.Name] = product;
            XElement dProduct = new XElement("Product");
            dProduct.Add(new XAttribute("Name", product.Name));
            dProduct.Add(new XAttribute("IsUploaded", product.IsUploaded));
            dProduct.Add(new XAttribute("Mmult", product.MachMult));
            dProduct.Add(new XAttribute("Cmult", product.CupsuleMult));
            productDatabase.Add(dProduct);
        }
        private LocalDatabase() { }
        public LocalDatabase(string databasePath) {
            FileStream fstream = null;
            fstream = new FileStream(databasePath, FileMode.OpenOrCreate);
            XDocument db = XDocument.Load(fstream);
            if (db.Root == null) {
                db.Add(new XElement("Database"));
            }
            productDatabase = db.Root.Element("Products");
            if (productDatabase == null) {
                db.Root.Add(new XElement("Products"));
                productDatabase = db.Root.Element("Products");
            }
            customerDatabase = db.Root.Element("Customers");
            if (customerDatabase == null) {
                db.Root.Add(new XElement("Customers"));
                customerDatabase = db.Root.Element("Customers");
            }
            foreach (XElement product in productDatabase.Elements()) {
                Product tProduct = new Product(product.Attribute("Name").Value);
                tProduct.IsUploaded = bool.Parse(product.Attribute("IsUploaded").Value);
                tProduct.CupsuleMult = Convert.ToInt32(product.Attribute("Cmult").Value);
                tProduct.MachMult = Convert.ToInt32(product.Attribute("Mmult").Value);
                Products.Add(tProduct.Name, tProduct);
            }
            foreach (XElement customer in customerDatabase.Elements()) {
                Customer tCustomer = new Customer(customer.Attribute("Name").Value);
                tCustomer.IsUploaded = bool.Parse(customer.Attribute("IsUploaded").Value);
                tCustomer.altName = customer.Attribute("AltName").Value;
                tCustomer.City = customer.Attribute("City").Value;
                tCustomer.Region = customer.Attribute("Region").Value;
                Customers.Add(tCustomer.Name, tCustomer);
            }
            fstream.Close();
        }
    }
}

