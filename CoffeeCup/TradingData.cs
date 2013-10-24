using System;
using System.Collections.Generic;
using System.ComponentModel;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Windows;
using System.Security.Permissions;
using System.Security;


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
    public class CCupWSEntry : IComparable<CCupWSEntry> {
        public void AddNewCustomerRow(List<Customer> newCustomers){
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = iWorksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = spreadsheetService.Query(listQuery);
            ListEntry example = (ListEntry)listFeed.Entries[0];
            ListEntry total = (ListEntry)listFeed.Entries.Last();
            uint linenumber = (uint)listFeed.TotalResults;
            total.Delete();
            foreach (Customer customer in newCustomers) {
                if (cust_Row.ContainsKey(customer.Name) || cust_Row.ContainsKey(customer.altName)) {
                    continue;
                }
                spreadsheetService.Insert(listFeed, GenerateCustomerRow(customer,example));
                if (string.IsNullOrWhiteSpace(customer.altName)) {
                    cust_Row.Add(customer.Name, ++linenumber);
                }
                else {
                    cust_Row.Add(customer.altName, ++linenumber);
                }
            }
            spreadsheetService.Insert(listFeed, total);
        }
        public uint AddNewCustomerRow(Customer newCustomer){
            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = iWorksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = spreadsheetService.Query(listQuery);
            ListEntry example = (ListEntry)listFeed.Entries[0];
            ListEntry total = (ListEntry)listFeed.Entries.Last();
            uint linenumber = (uint)listFeed.TotalResults;
            total.Delete();
            spreadsheetService.Insert(listFeed, GenerateCustomerRow(newCustomer,example));
            spreadsheetService.Insert(listFeed, total);
            if (string.IsNullOrWhiteSpace(newCustomer.altName)) {
                cust_Row.Add(newCustomer.Name, ++linenumber);
            }
            else {
                cust_Row.Add(newCustomer.altName, ++linenumber);
            }
            return linenumber;
        }
        public Dictionary<string, uint> cust_Row;
        public uint worksheetYear;
        public bool UploadData(List<Realization> yRealizations) {
            CellQuery cellQuery = new CellQuery(iWorksheetEntry.CellFeedLink);
            CellFeed cellFeed = spreadsheetService.Query(cellQuery);
            CellSameAddress CellEqC = new CellSameAddress();
            Dictionary<CellAddress, string> Address_Value = new Dictionary<CellAddress, string>(CellEqC);
            Dictionary<string, CellEntry> Address_Cell;
            List<Customer> newCustomers = new List<Customer>();
            List<Realization> RemRealizations = new List<Realization>();
            List<SellingPosition> RemSP = new List<SellingPosition>();
            foreach (Realization doc in yRealizations) {
                if (!doc.Buyer.IsUploaded) {
                    RemRealizations.Add(doc);
                    continue;
                }
                foreach (SellingPosition rec in doc.SellingPositions) {
                    if (!rec.Product.IsUploaded) RemSP.Add(rec);
                }
                if (RemSP.Count!=0) {
                    foreach (SellingPosition sp in RemSP) {
                        doc.SellingPositions.Remove(sp);
                    }
                    if (doc.SellingPositions.Count == 0) {
                        RemRealizations.Add(doc);
                        continue;
                    }
                }
                if (cust_Row.ContainsKey(doc.Buyer.altName) || cust_Row.ContainsKey(doc.Buyer.altName)) {
                    continue;
                }
                else {
                    newCustomers.Add(doc.Buyer);
                }
            }
            foreach (Realization r in RemRealizations) {
                yRealizations.Remove(r);
            }
            if (newCustomers.Count != 0) {
                AddNewCustomerRow(newCustomers);
            }
            foreach (Realization doc in yRealizations) {
                uint docColOffset = 3 + 6 * ((uint)doc.Date.Month - 1);
                uint docRow = 0;
                if (cust_Row.ContainsKey(doc.Buyer.altName)) docRow = cust_Row[doc.Buyer.altName];
                else if (cust_Row.ContainsKey(doc.Buyer.Name)) docRow = cust_Row[doc.Buyer.Name];
                else {
                    docRow = AddNewCustomerRow(doc.Buyer);
                }
                #region Filling Address_Value dictionary
                foreach (SellingPosition rec in doc.SellingPositions) {
                    if (!rec.Product.IsUploaded) continue;
                    CellAddress cupNumAddress = new CellAddress(docRow, docColOffset + 1);
                    CellAddress machNumAddress = new CellAddress(docRow, docColOffset + 2);
                    CellAddress cupSumAddress = new CellAddress(docRow, docColOffset + 5);
                    CellAddress machSumAddress = new CellAddress(docRow, docColOffset + 6);
                    if (rec.Product.MachMult == 0) //This is capsules!
                    {
                        string cupNumstr = (rec.Amount * rec.Product.CupsuleMult).ToString();
                        string cupSumstr = (rec.Price).ToString();
                        string leadingstr;
                        if (Address_Value.ContainsKey(cupNumAddress)) {
                            leadingstr = "+";
                            Address_Value[cupNumAddress] += (leadingstr + cupNumstr);
                            if (Address_Value.ContainsKey(cupSumAddress)) Address_Value[cupSumAddress] += (leadingstr + cupSumstr);
                            else Address_Value[cupSumAddress] = (leadingstr + cupSumstr);
                        }
                        else {
                            leadingstr = string.Empty;
                            Address_Value[cupNumAddress] = (leadingstr + cupNumstr);
                            Address_Value[cupSumAddress] = (leadingstr + cupSumstr);
                        }
                    }
                    else //This is CoffeeMachine or Set 
                    {
                        string machNumstr = (rec.Amount * rec.Product.MachMult).ToString();
                        string machSumstr = (rec.Price).ToString();
                        string leadingstr;
                        if (Address_Value.ContainsKey(machNumAddress)) {
                            leadingstr = "+";
                            Address_Value[machNumAddress] += (leadingstr + machNumstr);
                            Address_Value[machSumAddress] += (leadingstr + machSumstr);
                        }
                        else {
                            leadingstr = string.Empty;
                            Address_Value[machNumAddress] = (leadingstr + machNumstr);
                            Address_Value[machSumAddress] = (leadingstr + machSumstr);
                        }
                        if (rec.Product.CupsuleMult != 0) {
                            string cupNumstr = (rec.Amount * rec.Product.CupsuleMult).ToString();
                            if (Address_Value.ContainsKey(cupNumAddress)) {
                                leadingstr = "+";
                                Address_Value[cupNumAddress] += (leadingstr + cupNumstr);
                            }
                            else {
                                leadingstr = string.Empty;
                                Address_Value[cupNumAddress] = (leadingstr + cupNumstr);
                            }
                        }
                    }
                }
                #endregion
            }
            Address_Cell = GetCellEntryMap(cellFeed, Address_Value.Keys.ToList());
            CellFeed batchRequest = new CellFeed(cellQuery.Uri, spreadsheetService);
            foreach (CellAddress cellID in Address_Value.Keys) {
                CellEntry batchEntry = Address_Cell[cellID.IdString];
                if (batchEntry.InputValue == "") {
                    batchEntry.InputValue = "=" + Address_Value[cellID];
                }
                else {
                    batchEntry.InputValue += "+" + Address_Value[cellID];
                }
                batchEntry.BatchData = new GDataBatchEntryData(cellID.IdString, GDataBatchOperationType.update);
                batchRequest.Entries.Add(batchEntry);
            }
            // Submit the update
            CellFeed batchResponse = (CellFeed)spreadsheetService.Batch(batchRequest, new Uri(cellFeed.Batch));
            // Check the results
            bool isSuccess = true;
            foreach (CellEntry entry in batchResponse.Entries) {
                string batchId = entry.BatchData.Id;
                if (entry.BatchData.Status.Code != 200) {
                    isSuccess = false;
                    GDataBatchStatus status = entry.BatchData.Status;
                    MessageBox.Show(string.Format("{0} failed ({1})", batchId, status.Reason));
                }
            }
            return isSuccess;
        }
        WorksheetEntry iWorksheetEntry;
        private Dictionary<String, CellEntry> GetCellEntryMap(CellFeed cellFeed, List<CellAddress> cellAddrs) {
            CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), spreadsheetService);
            foreach (CellAddress cellId in cellAddrs) {
                CellEntry batchEntry = new CellEntry(cellId.Row, cellId.Col, cellId.IdString);
                batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellId.IdString));
                batchEntry.BatchData = new GDataBatchEntryData(cellId.IdString, GDataBatchOperationType.query);
                batchRequest.Entries.Add(batchEntry);
            }

            CellFeed queryBatchResponse = (CellFeed)spreadsheetService.Batch(batchRequest, new Uri(cellFeed.Batch));

            Dictionary<String, CellEntry> cellEntryMap = new Dictionary<String, CellEntry>();
            foreach (CellEntry entry in queryBatchResponse.Entries) {
                cellEntryMap.Add(entry.BatchData.Id, entry);
            }
            return cellEntryMap;
        }
        ListEntry GenerateCustomerRow(Customer cust, ListEntry example) {
            /// Generate new customer row 
            string medCup = "=IFERROR(R[0]C[-3]/R[0]C[-1]*120;\"Нет данных\")";
            string machNum = "=R[0]C[-7]+R[0]C[-6]";
            string cupSum = "=R[0]C[3]";
            string machSum = "=R0C[-6]+R0C[-5]";
            ListEntry custRow = new ListEntry();
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[0].LocalName, Value = cust.City});
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[1].LocalName, Value = cust.Region });
            if (string.IsNullOrWhiteSpace(cust.altName)) {
                custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[2].LocalName, Value = cust.Name });
            }
            else custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[2].LocalName, Value = cust.altName });            
            custRow.Elements.Add(new ListEntry.Custom() { LocalName = example.Elements[6].LocalName, Value = medCup });
            //January - try to get mah data from prev year
            if (worksheetYear!=0) {
                CCupWSEntry[] wsList= worksheetFeed.EntriesList.ToArray();
                Array.Sort(wsList);
                int maxindex = Array.FindLastIndex(wsList,ws => ws.worksheetYear < worksheetYear);
                int minindex = Array.FindIndex(wsList, ws => ws.worksheetYear > 0);
                string janMah = "";
                if (maxindex > 0) {
                    for (int i = maxindex; i >= minindex; i--) {
                        if (string.IsNullOrWhiteSpace(cust.altName)) {
                            if (wsList[i].cust_Row.ContainsKey(cust.Name)) {
                                janMah = string.Format("='" + wsList[i].worksheetYear.ToString() + "'!R{1}C76", wsList[i].cust_Row[cust.Name]);
                                break;
                            }
                            else if (wsList[i].cust_Row.ContainsKey(cust.Name)) {
                                janMah = string.Format("='" + wsList[i].worksheetYear.ToString() + "'!R{1}C76", wsList[i].cust_Row[cust.Name]);
                                break;
                            }
                        }
                        else {
                            if (wsList[i].cust_Row.ContainsKey(cust.altName)) {
                                janMah = string.Format("='" + wsList[i].worksheetYear.ToString() + "'!R{1}C76", wsList[i].cust_Row[cust.altName]);
                                break;
                            }
                            else if (wsList[i].cust_Row.ContainsKey(cust.Name)) {
                                janMah = string.Format("='" + wsList[i].worksheetYear.ToString() + "'!R{1}C76", wsList[i].cust_Row[cust.Name]);
                                break;
                            }
                        }
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
        public CCupWSEntry(SpreadsheetsService service, CCupWSFeed feed, WorksheetEntry worksheetEntry) {
            iWorksheetEntry = worksheetEntry;
            spreadsheetService = service;
            worksheetFeed = feed;
            if (!uint.TryParse(iWorksheetEntry.Title.Text, out worksheetYear)) worksheetYear = 0;
            cust_Row = new Dictionary<string, uint>();
            if (iWorksheetEntry.Rows > 3) {
                CellQuery cellQuery = new CellQuery(iWorksheetEntry.CellFeedLink);
                cellQuery.MinimumRow = 3;
                cellQuery.MaximumRow = iWorksheetEntry.Rows - 1;
                cellQuery.MinimumColumn = 3;
                cellQuery.MaximumColumn = 3;
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
        WorksheetFeed iWorksheetFeed;
        SpreadsheetsService iSpreadsheetService;
        public CCupWSFeed(WorksheetFeed worksheetFeed, SpreadsheetsService spreadsheetsService) {
            EntriesList = new List<CCupWSEntry>();
            iWorksheetFeed = worksheetFeed;
            iSpreadsheetService = spreadsheetsService;
            foreach (WorksheetEntry wsEntry in worksheetFeed.Entries) {
                EntriesList.Add(new CCupWSEntry(iSpreadsheetService, this, wsEntry));
            }
        }        
        private CCupWSFeed() { }
    }
    public class LocalDatabase {
        XElement productDatabase;
        XElement customerDatabase;
        XDocument database;
        string pathToFile;
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
        public void SyncToDrive() {
            FileStream fstream = null;
            fstream = new FileStream(pathToFile, FileMode.Create);
            database.Save(fstream);
            fstream.Close();
        }
        void UpdateEntryValue(Customer customer){
            Customers[customer.Name] = customer;
            XElement dCustomer;
            try { 
                dCustomer = customerDatabase.Elements().Where((c) => c.Attribute("Name").Value == customer.Name).Single(); 
            }
            catch {
                FixDatabase();
                dCustomer = customerDatabase.Elements().Where((c) => c.Attribute("Name").Value == customer.Name).Single();
            }
            dCustomer.Attribute("AltName").SetValue(customer.altName);
            dCustomer.Attribute("City").SetValue(customer.City);
            dCustomer.Attribute("Region").SetValue(customer.Region);
            dCustomer.Attribute("IsUploaded").SetValue(customer.IsUploaded);
        }
        void UpdateEntryValue(Product product) {
            Products[product.Name] = product;
            XElement dProduct;
            try {
                dProduct = productDatabase.Elements().Where((p) => p.Attribute("Name").Value == product.Name).Single();
            }
            catch {
                FixDatabase();
                dProduct = productDatabase.Elements().Where((p) => p.Attribute("Name").Value == product.Name).Single();
            }
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
        void FixDatabase() {
            foreach (XElement product in productDatabase.Elements()) {
                foreach (XElement dub in productDatabase.Elements().Where((p) => (string)p.Attribute("Name") == product.Name)) {
                    if (product == dub) continue;
                    else dub.Remove();
                }
            }
            foreach (XElement customer in customerDatabase.Elements()) {
                foreach (XElement dub in customerDatabase.Elements().Where((c) => (string)c.Attribute("Name") == customer.Name)) {
                    if (customer == dub) continue;
                    else dub.Remove();
                }
            }
        }
        private LocalDatabase() { }
        public LocalDatabase(string databasePath) {
            pathToFile = databasePath;
            FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, databasePath);
            try {
                permission.Demand();
            }
            catch (SecurityException s) {
                MessageBox.Show(s.Message);
            }
            FileStream fstream = null;
            fstream = new FileStream(databasePath, FileMode.OpenOrCreate);
            try {
                database = XDocument.Load(fstream);
            }
            catch {
                database = new XDocument();
            }
            if (database.Root == null) {
                database.Add(new XElement("Database"));
            }
            productDatabase = database.Root.Element("Products");
            if (productDatabase == null) {
                database.Root.Add(new XElement("Products"));
                productDatabase = database.Root.Element("Products");
            }
            customerDatabase = database.Root.Element("Customers");
            if (customerDatabase == null) {
                database.Root.Add(new XElement("Customers"));
                customerDatabase = database.Root.Element("Customers");
            }
            Products = new Dictionary<string, Product>();
            foreach (XElement product in productDatabase.Elements()) {
                Product tProduct = new Product(product.Attribute("Name").Value);
                tProduct.IsUploaded = bool.Parse(product.Attribute("IsUploaded").Value);
                tProduct.CupsuleMult = Convert.ToInt32(product.Attribute("Cmult").Value);
                tProduct.MachMult = Convert.ToInt32(product.Attribute("Mmult").Value);
                Products.Add(tProduct.Name, tProduct);
            }
            Customers = new Dictionary<string, Customer>();
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

