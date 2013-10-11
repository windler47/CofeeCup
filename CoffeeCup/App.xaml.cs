using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security;
using System.Windows;
using System.Xml.Linq;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace CoffeeCup {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        #region Google staff
        const string CLIENT_ID = "19361090870.apps.googleusercontent.com";
        const string CLIENT_SECRET = "CZuF5r88V_6JGsP3pFlnoYDl";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";
        const string SCOPE = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds/";
        #endregion
        OAuth2Parameters parameters;
        SpreadsheetsService GSpreadsheetService;
        public string docUri; //Document key
        public string docPath; //Document Path
        public XElement xmlDoc;
        public Dictionary<uint, List<Realization>> realizations;
        CCupWSFeed worksheetsFeed;
        public string GAuthGetLink() {
            return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
        }
        public void GAuthStep2(string accessCode) {
            parameters.AccessCode = accessCode;
            OAuthUtil.GetAccessToken(parameters);
            GOAuth2RequestFactory GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
            GSpreadsheetService.RequestFactory = GRequestFactory;
        }
        public bool WorksheetFeedInit() {
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();
            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = GSpreadsheetService.Query(query);
            if (feed.Entries.Count == 0) {
                MessageBox.Show("No documents found :(");
                return true;
            }
            SpreadsheetEntry spreadsheet = null;
            foreach (AtomEntry spr in feed.Entries) {
                if (((SpreadsheetEntry)spr).Title.Text == docUri) {
                    spreadsheet = (SpreadsheetEntry)spr;
                    break;
                }
            }
            if (spreadsheet == null) {
                MessageBox.Show("No documents found :(");
                return true;
            }
            worksheetsFeed = new CCupWSFeed(spreadsheet.Worksheets, GSpreadsheetService);
            return false;
        }
        public bool InitializexmlDoc() {
            bool result = false;
            try {
                xmlDoc = XElement.Load(docPath);
            }
            catch (ArgumentNullException) {
                MessageBox.Show("Не указан путь к файлу с данными для выгрузки");
                result = true;
            }
            catch (SecurityException) {
                MessageBox.Show("Недостаточно прав для обращения к файлу " + docPath);
                result = true;
            }
            catch (FileNotFoundException) {
                MessageBox.Show("Файл " + docPath + "не найден.");
                result = true;
            }
            return result;
        }
        public Dictionary<int, Product> GetGoods() {
            Dictionary<int, Product> result = new Dictionary<int, Product>();
            IEnumerable<XElement> goods =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "Номенклатура"
                select fobject;

            foreach (XElement el in goods) {
                #region Qery data from Xml
                bool isGroup = (from el1 in el.Element("Ссылка").Elements("Свойство")
                                where (string)el1.Attribute("Имя") == "ЭтоГруппа" && (string)el1.Element("Значение") == "true"
                                select el1).Any();
                if (isGroup) continue;
                Product tproduct = new Product((string)(from el1 in el.Elements("Свойство")
                                                        where (string)el1.Attribute("Имя") == "Наименование"
                                                        select el1).ElementAt(0));
                #endregion
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"), tproduct);
            }
            return result;
        }
        public Dictionary<int, Customer> GetCustomers() {
            Dictionary<int, Customer> result = new Dictionary<int, Customer>();
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "Контрагенты"
                select fobject;
            foreach (XElement el in obj) {
                bool isGroup = (from el1 in el.Element("Ссылка").Elements("Свойство")
                                where (string)el1.Attribute("Имя") == "ЭтоГруппа" && (string)el1.Element("Значение") == "true"
                                select el1).Any();
                if (isGroup) continue;
                Customer customer = new Customer((string)(from el1 in el.Elements("Свойство")
                                                          where (string)el1.Attribute("Имя") == "Наименование"
                                                          select el1).ElementAt(0));
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"), customer);
            }
            return result;
        }
        public void GetRealisations(Dictionary<int, Customer> Customers, Dictionary<int, Product> Products) {
            int? warehouseCode = null;
            XElement company = xmlDoc.Elements("Объект").Where((e) => { return (string)e.Attribute("ИмяПравила") == "Организации"; }).Single();
            if (company.Element("Свойство").Element("Значение").Value == "\"Торговый дом РИКО\"") {
                XElement warehouse = xmlDoc.Elements("Объект").Where((e) =>
                {
                    if ((string)e.Attribute("ИмяПравила") != "Склады") return false;
                    return e.Element("Свойство").Element("Значение").Value == "Основной склад";
                }).Single();
                warehouseCode = Convert.ToInt32(warehouse.Element("Ссылка").Attribute("Нпп").Value);
            }
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "РеализацияТоваровУслуг"
                select fobject;
            foreach (XElement ProductRealisation in obj) {
                if (warehouseCode != null) {
                    if ( Convert.ToInt32(ProductRealisation.Elements("Свойство").Where(
                        (e) => { return (string)e.Attribute("Имя") == "Склад"; }
                        ).Single().Element("Ссылка").Attribute("Нпп").Value) != warehouseCode ) 
                    {
                            continue;
                    }
                }
                bool IsDelited = (from prop in ProductRealisation.Elements("Свойство")
                                  where (string)prop.Attribute("Имя") == "ПометкаУдаления" && (string)prop.Element("Значение") == "true"
                                  select prop).Any();

                bool IsCommited = (from prop in ProductRealisation.Elements("Свойство")
                                   where (string)prop.Attribute("Имя") == "Проведен" && (string)prop.Element("Значение") == "true"
                                   select prop).Any();
                if (IsDelited || !IsCommited) continue;
                Realization document = new Realization();
                //TODO: add .single exception handling
                try {
                    document.Date = DateTime.Parse((string)(from prop in ProductRealisation.Element("Ссылка").Elements("Свойство")
                                                            where (string)prop.Attribute("Имя") == "Дата"
                                                            select prop).Single().Element("Значение"));
                }
                catch (FormatException) {
                    MessageBox.Show("Error while parsing xml date");
                    continue;
                }
                int buyerCode = (int)(from prop in ProductRealisation.Elements("Свойство")
                                  where (string)prop.Attribute("Имя") == "Контрагент"
                                  select prop).Single().Element("Ссылка").Attribute("Нпп");
                document.Buyer = Customers[buyerCode];
                IEnumerable<XElement> Records = (from elements in ProductRealisation.Elements("ТабличнаяЧасть")
                                                 where (string)elements.Attribute("Имя") == "Товары"
                                                 select elements).Single().Elements();
                #region Selling positions parsing
                foreach (XElement record in Records) {
                    SellingPosition sp = new SellingPosition();
                    sp.Amount = Convert.ToInt32((from prop in record.Elements()
                                                 where (string)prop.Attribute("Имя") == "Количество"
                                                 select prop).Single().Element("Значение").Value);
                    int npp = (int)(from prop in record.Elements()
                                    where (string)prop.Attribute("Имя") == "Номенклатура"
                                    select prop).Single().Element("Ссылка").Attribute("Нпп");
                    try {
                        sp.Product = Products[npp];
                    }
                    catch (KeyNotFoundException) {
                        MessageBox.Show("Error in xml: Sellin product was not found in dictionary");
                    }
                    sp.Price = Convert.ToDouble((from prop in record.Elements()
                                                where (string)prop.Attribute("Имя") == "Сумма"
                                                select prop).Single().Element("Значение").Value, new CultureInfo("en-US"));
                    document.SellingPositions.Add(sp);
                }
                #endregion
                if (realizations.ContainsKey((uint)document.Date.Year)) {
                    realizations[(uint)document.Date.Year].Add(document);
                }
                else {
                    List<Realization> newRealizationList = new List<Realization>();
                    newRealizationList.Add(document);
                    realizations.Add((uint)document.Date.Year, newRealizationList);
                }
            }
        }
        //public bool  GetCustomerData(ref List<Customer> customerList) {
        //    // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
        //    SpreadsheetQuery query = new SpreadsheetQuery();
        //    // Make a request to the API and get all spreadsheets.
        //    SpreadsheetFeed feed = GSpreadsheetService.Query(query);
        //    if (feed.Entries.Count == 0) {
        //        MessageBox.Show("No documents found :(");
        //        return true;
        //    }
        //    SpreadsheetEntry spreadsheet = null;
        //    foreach (AtomEntry spr in feed.Entries) {
        //        if (((SpreadsheetEntry)spr).Title.Text == DocUri) {
        //            spreadsheet = (SpreadsheetEntry)spr;
        //            break;
        //        }
        //    }
        //    if (spreadsheet == null) {
        //        MessageBox.Show("No documents found :(");
        //        return true;
        //    }
        //    WorksheetFeed wsFeed = spreadsheet.Worksheets;
        //    foreach (WorksheetEntry ws in wsFeed.Entries)
        //    {
        //        int wsYear;
        //        if(int.TryParse(ws.Title.Text,out wsYear)){
        //            WS_year.Add(wsYear, ws);
        //        }
        //        else {
        //            MessageBox.Show("Название лсита " + ws.Title.Text + " не распознано как год.");
        //        }
        //    }
        //    if (!WS_year.Any()) {
        //        WS_year.Add(2013, (WorksheetEntry)wsFeed.Entries[0]);
        //        MessageBox.Show("Лист " + ((WorksheetEntry)wsFeed.Entries[0]).Title.Text + " считается соотвествующим 2013 году!");
        //    }
        //    // Fetch the cell feed of the worksheet.
        //    CellQuery cellQuery = new CellQuery(TargetWS.CellFeedLink);
        //    cellQuery.MinimumColumn = 1;
        //    cellQuery.MaximumColumn = 3;
        //    CellFeed cellFeed = GSpreadsheetService.Query(cellQuery);
        //    string city = null;
        //    string region = null;
        //    foreach (CellEntry cell in cellFeed.Entries) {
        //        #region Fill in Customer_Row dictionary
        //        if (cell.Title.Text == "A1") continue;
        //        switch (cell.Column) {
        //            case 1: {
        //                    city = cell.InputValue;
        //                    break;
        //                }
        //            case 2: {
        //                    region = cell.InputValue;
        //                    break;
        //                }
        //            case 3: {
        //                Customer tcust = null;
        //                try {tcust = customerList.Find((e) => { return e.Name == cell.InputValue; });}                            
        //                catch (ArgumentNullException) {}
        //                try { tcust = customerList.Find((e) => { return e.altName == cell.InputValue; }); }
        //                catch (ArgumentNullException) {}
        //                if (tcust != null) {
        //                    tcust.City = city;
        //                    tcust.Region = region;
        //                    tcust.altName = cell.InputValue;
        //                }
        //                break;
        //            }
        //        }
        //        #endregion
        //    }
        //    return false;
        //}
        bool UploadData(List<Realization> yRealizations, CCupWSEntry worksheet) {
            CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
            CellFeed cellFeed = GSpreadsheetService.Query(cellQuery);
            CellSameAddress CellEqC = new CellSameAddress();
            Dictionary<CellAddress, string> Address_Value = new Dictionary<CellAddress, string>(CellEqC);
            Dictionary<string, CellEntry> Address_Cell;
            foreach (Realization doc in yRealizations) {
                bool contains_cofee = false;
                foreach (SellingPosition rec in doc.SellingPositions){
                    if (rec.Product.IsUploaded) contains_cofee = true;
                }
                if (!doc.Buyer.IsUploaded || !contains_cofee) continue;

                uint docColOffset = 3 + 6 * (((uint)doc.Date.Month + 12 * ((uint)doc.Date.Year - 2013))-1);
                uint docRow = 0;
                if (worksheet.cust_Row.ContainsKey(doc.Buyer.altName)) docRow = worksheet.cust_Row[doc.Buyer.altName];
                else if (worksheet.cust_Row.ContainsKey(doc.Buyer.Name)) docRow = worksheet.cust_Row[doc.Buyer.Name];
                else {
                    docRow = worksheet.AddNewCustomerRow(doc.Buyer);
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
            Address_Cell = GetCellEntryMap(GSpreadsheetService, cellFeed, Address_Value.Keys.ToList());
            CellFeed batchRequest = new CellFeed(cellQuery.Uri, GSpreadsheetService);
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
            CellFeed batchResponse = (CellFeed)GSpreadsheetService.Batch(batchRequest, new Uri(cellFeed.Batch));
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
        public bool UploadData() {
            bool success = true;
            Dictionary<uint,CCupWSEntry> year_ws = new Dictionary<uint,CCupWSEntry>();
            foreach (CCupWSEntry worksheet in worksheetsFeed.EntriesList){
                year_ws.Add(worksheet.worksheetYear,worksheet);
            }
            foreach (uint year in realizations.Keys) {
                if (year_ws.ContainsKey(year)){
                    success = success && UploadData(realizations[year], year_ws[year]);
                }
                else {
                    MessageBox.Show(string.Format("Лист для выгрузки документов за {0} год не найден. Данные за этот год не будут выгружены!",year));
                    success = false;
                }
            }
            return success;
        }
        public bool LoadGRefreshToken() {
            FileStream fs;
            try {
                fs = new FileStream("cc.bin", FileMode.Open);
            }
            catch {
                return true;
            }
            BinaryReader br = new BinaryReader(fs);
            parameters.RefreshToken = br.ReadString();
            OAuthUtil.RefreshAccessToken(parameters);
            if (parameters.AccessToken == null) return true;
            return false;
        }
        public void GracefulShutdown() {
            SaveGRefreshToken(parameters.RefreshToken);
            this.Shutdown();
        }    
        public App() {
            parameters = new OAuth2Parameters();
            parameters.ClientId = CLIENT_ID;
            parameters.ClientSecret = CLIENT_SECRET;
            parameters.RedirectUri = REDIRECT_URI;
            parameters.Scope = SCOPE;
            GSpreadsheetService = new SpreadsheetsService("CoffeeCup");
            realizations = new Dictionary<uint, List<Realization>>();
        }
        private void SaveGRefreshToken(string refreshToken) {
            FileStream fs = new FileStream("cc.bin", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(refreshToken);
            fs.Close();
        }
        public void SaveProductData(List<Product> prodList) {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
            IsolatedStorageScope.Assembly, null, null);
            string filename = @"C:\Coffee Cup\LocalBase.xml";
            FileStream fstream = null;
            XDocument db = null;
            bool isDBExist = true;
            try{fstream = new FileStream(filename, FileMode.Open);}
            catch {
                isDBExist = false;
                fstream = new FileStream(filename, FileMode.CreateNew);
            }
            if (isDBExist){ db = XDocument.Load(fstream);}
            else { 
                db = new XDocument();
                db.Add(new XElement("Database"));
            }
            XElement prodDB = db.Root.Element("Products");
            if (prodDB == null) {
                db.Root.Add(new XElement("Products"));
                prodDB = db.Root.Element("Products");
            }
            foreach (Product prod in prodList) {
                XElement p = null;
                if (prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == prod.Name; }).Any()) {
                    try {
                        p = prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == prod.Name; }).Single();
                    }
                    catch {
                        p = prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == prod.Name; }).First();
                        foreach (XElement dub in prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == prod.Name; })) {
                            if (dub == p) continue;
                            dub.Remove();
                        }
                    }
                    p.Attribute("IsUploaded").SetValue(prod.IsUploaded);
                    p.Attribute("Mmult").SetValue(prod.MachMult);
                    p.Attribute("Cmult").SetValue(prod.CupsuleMult);
                }
                else { 
                    p = new XElement("Product");
                    p.Add(new XAttribute("Name", prod.Name));
                    p.Add(new XAttribute("IsUploaded", prod.IsUploaded));
                    p.Add(new XAttribute("Mmult", prod.MachMult));
                    p.Add(new XAttribute("Cmult", prod.CupsuleMult));
                    prodDB.Add(p);
                }
            }
            fstream.Seek(0, SeekOrigin.Begin);
            db.Save(fstream);
            fstream.Close();
        }
        public bool LoadProductData(ref List<Product> prodList) {
            string filename = @"C:\Coffee Cup\LocalBase.xml";
            FileStream fstream = null;
            try {
                fstream = new FileStream(filename, FileMode.Open);
            }
            catch (FileNotFoundException) {
                MessageBox.Show("Error: Local database file not found!");
                return true;
            }
            if (fstream.CanRead == false) {
                MessageBox.Show("Database file " + filename +" cannot be read!");
                fstream.Close();
                return true;
            }
            XDocument doc = XDocument.Load(fstream);
            XElement prodDB = doc.Root.Element("Products");
            if (prodDB == null) {
                MessageBox.Show("There are no Product data in local database!");
                fstream.Close();
                return false;
            }
            if (prodDB.IsEmpty) {
                MessageBox.Show("There are no Product data in local database!");
                fstream.Close();
                return false;
            }
            foreach (Product prod in prodList) {
                try {
                    XElement p = (from prodRec in prodDB.Elements()
                                  where (string)prodRec.Attribute("Name") == prod.Name
                                  select prodRec).Single();
                    prod.IsUploaded = bool.Parse(p.Attribute("IsUploaded").Value);
                    prod.CupsuleMult = Convert.ToInt32(p.Attribute("Cmult").Value);
                    prod.MachMult = Convert.ToInt32(p.Attribute("Mmult").Value);
                }
                catch {}
            }
            fstream.Close();
            return false;
            
        }

        public void SaveCustomerData(List<Customer> custList) {
            string filename = @"C:\Coffee Cup\LocalBase.xml";
            FileStream fstream = null;
            XDocument db = null;
            bool isDBExist = true;
            try { fstream = new FileStream(filename, FileMode.Open); }
            catch {
                isDBExist = false;
                fstream = new FileStream(filename, FileMode.CreateNew);
            }
            if (isDBExist) { db = XDocument.Load(fstream); }
            else { 
                db = new XDocument();
                db.Add(new XElement("Database"));
            }
            XElement prodDB = db.Root.Element("Customers");
            if (prodDB == null) {
                db.Root.Add(new XElement("Customers"));
                prodDB = db.Root.Element("Customers");
            }
            foreach (Customer cust in custList) {
                XElement c = null;
                if (prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == cust.Name; }).Any()) {
                    try {
                        c = prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == cust.Name; }).Single();
                    }
                    catch {
                        c = prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == cust.Name; }).First();
                        foreach (XElement dub in prodDB.Elements().Where((e) => { return (string)e.Attribute("Name") == cust.Name; })) {
                            if (dub == c) continue;
                            dub.Remove();
                        }
                    }
                    c.Attribute("AltName").SetValue(cust.altName);
                    c.Attribute("IsUploaded").SetValue(cust.IsUploaded);
                    c.Attribute("City").SetValue(cust.City);
                    c.Attribute("Region").SetValue(cust.Region);
                }
                else { 
                    c = new XElement("Customer");
                    c.Add(new XAttribute("Name", cust.Name));
                    c.Add(new XAttribute("AltName", cust.altName));
                    c.Add(new XAttribute("IsUploaded", cust.IsUploaded));
                    c.Add(new XAttribute("City", cust.City));
                    c.Add(new XAttribute("Region", cust.Region));
                    prodDB.Add(c);
                }
            }
            fstream.Seek(0, SeekOrigin.Begin);
            db.Save(fstream);
            fstream.Close();
        }

        public bool LoadCustomerData(ref List<Customer> custList) {
            string filename = @"C:\Coffee Cup\LocalBase.xml";
            FileStream fstream = null;
            try {
                fstream = new FileStream(filename, FileMode.Open);
            }
            catch (FileNotFoundException) {
                MessageBox.Show("Error: Local database file not found!");
                return true;
            }
            if (fstream.CanRead == false) {
                MessageBox.Show("Database file " + filename + " cannot be read!");
                fstream.Close();
                return true;
            }
            XDocument doc = XDocument.Load(fstream);
            XElement custDB = doc.Root.Element("Customers");
            if (custDB == null) {
                MessageBox.Show("There are no Customer data in local database!");
                fstream.Close();
                return false;
            }
            if (custDB.IsEmpty) {
                MessageBox.Show("There are no Customer data in local database!");
                fstream.Close();
                return false;
            }
            foreach (Customer cust in custList) {
                try {
                    XElement c = (from custRec in custDB.Elements()
                                  where (string)custRec.Attribute("Name") == cust.Name
                                  select custRec).Single();
                    cust.IsUploaded = bool.Parse(c.Attribute("IsUploaded").Value);
                    cust.altName = c.Attribute("AltName").Value;
                    cust.City = c.Attribute("City").Value;
                    cust.Region = c.Attribute("Region").Value;
                }
                catch { }
            }
            fstream.Close();
            return false;
        }
        private static Dictionary<String, CellEntry> GetCellEntryMap(SpreadsheetsService service, CellFeed cellFeed, List<CellAddress> cellAddrs) {
            CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), service);
            foreach (CellAddress cellId in cellAddrs) {
                CellEntry batchEntry = new CellEntry(cellId.Row, cellId.Col, cellId.IdString);
                batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellId.IdString));
                batchEntry.BatchData = new GDataBatchEntryData(cellId.IdString, GDataBatchOperationType.query);
                batchRequest.Entries.Add(batchEntry);
            }

            CellFeed queryBatchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

            Dictionary<String, CellEntry> cellEntryMap = new Dictionary<String, CellEntry>();
            foreach (CellEntry entry in queryBatchResponse.Entries) {
                cellEntryMap.Add(entry.BatchData.Id, entry);
            }
            return cellEntryMap;
        }
    }
}