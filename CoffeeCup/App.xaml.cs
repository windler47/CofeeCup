using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using System.Security;

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
        public OAuth2Parameters parameters = new OAuth2Parameters();
        #endregion
        public string DocUri; //Document key
        public string docPath; //Document Path
        public XElement xmlDoc;
        List<Realization> realizations;
        Dictionary<string, uint> Customer_Row;
        WorksheetEntry TargetWS;
        public string GAuthGetLink() {
            return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
        }
        public void GAuthStep2() {
            OAuthUtil.GetAccessToken(parameters);
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
            realizations = new List<Realization>();
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "РеализацияТоваровУслуг"
                select fobject;
            foreach (XElement ProductRealisation in obj) {
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
                    sp.Price = Convert.ToInt32((from prop in record.Elements()
                                                where (string)prop.Attribute("Имя") == "Сумма"
                                                select prop).Single().Element("Значение").Value);
                    sp.NDS = Convert.ToDouble((from prop in record.Elements()
                                               where (string)prop.Attribute("Имя") == "СуммаНДС"
                                               select prop).Single().Element("Значение").Value);
                    document.SellingPositions.Add(sp);
                }
                #endregion
                realizations.Add(document);
            }
        }
        public bool GQeryCustomers(List<Customer> cust) {
            ///Return false on success
            GOAuth2RequestFactory GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
            SpreadsheetsService GSpreadsheetService = new SpreadsheetsService("CoffeeCup");
            GSpreadsheetService.RequestFactory = GRequestFactory;
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
                if (((SpreadsheetEntry)spr).Title.Text == DocUri) {
                    spreadsheet = (SpreadsheetEntry)spr;
                    break;
                }
            }
            if (spreadsheet == null) {
                MessageBox.Show("No documents found :(");
                return true;
            }
            // Get the first worksheet of the spreadsheet.
            // TODO: Choose a worksheet more intelligently.
            WorksheetFeed wsFeed = spreadsheet.Worksheets;
            TargetWS = (WorksheetEntry)wsFeed.Entries[0];
            // Fetch the cell feed of the worksheet.
            CellQuery cellQuery = new CellQuery(TargetWS.CellFeedLink);
            cellQuery.MinimumColumn = 1;
            cellQuery.MaximumColumn = 3;
            CellFeed cellFeed = GSpreadsheetService.Query(cellQuery);
            Customer_Row = new Dictionary<string, uint>();
            foreach (CellEntry cell in cellFeed.Entries) {
                Customer tcust;
                string city = null;
                string region = null;
                if (cell.Title.Text == "A1") continue;
                switch (cell.Column) {
                    case 1: {
                            city = cell.InputValue;
                            break;
                        }
                    case 2: {
                            region = cell.InputValue;
                            break;
                        }
                    case 3: {
                            try {
                                tcust = cust.Find((e) => e.Name == cell.InputValue);
                            }
                            catch (ArgumentNullException) {
                                break;
                            }
                            tcust.City = city;
                            tcust.Region = region;
                            Customer_Row.Add(tcust.Name, cell.Row);
                            break;
                        }
                }
            }
            return false;
        }
        public bool UploadData() {
            GOAuth2RequestFactory GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
            SpreadsheetsService GSpreadsheetService = new SpreadsheetsService("CoffeeCup");
            GSpreadsheetService.RequestFactory = GRequestFactory;
            CellQuery cellQuery = new CellQuery(TargetWS.CellFeedLink);
            CellFeed cellFeed = GSpreadsheetService.Query(cellQuery);

            Dictionary<CellAddress, string> Address_Value = new Dictionary<CellAddress, string>();
            Dictionary<string, CellEntry> Address_Cell;
            foreach (Realization doc in realizations) {
                if (!doc.Buyer.IsUploaded) continue;

                uint docColOffset = 3 + 6 * ((uint)doc.Date.Month + 12 * ((uint)doc.Date.Year - 2013));
                uint docRow = Customer_Row[doc.Buyer.Name];
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
                            Address_Value[cupSumAddress] += (leadingstr + cupSumstr);
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
                if (batchEntry.InputValue=="") {
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
        public void SaveGRefreshToken(string refreshToken) {
            FileStream fs = new FileStream("cc.bin", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(refreshToken);
            fs.Close();
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
        private static Dictionary<String, CellEntry> GetCellEntryMap(SpreadsheetsService service, CellFeed cellFeed, List<CellAddress> cellAddrs) 
        {
            CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), service);
            foreach (CellAddress cellId in cellAddrs) 
            {
                CellEntry batchEntry = new CellEntry(cellId.Row, cellId.Col, cellId.IdString);
                batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellId.IdString));
                batchEntry.BatchData = new GDataBatchEntryData(cellId.IdString, GDataBatchOperationType.query);
                batchRequest.Entries.Add(batchEntry);
            }

            CellFeed queryBatchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

            Dictionary<String, CellEntry> cellEntryMap = new Dictionary<String, CellEntry>();
            foreach (CellEntry entry in queryBatchResponse.Entries) 
            {
                cellEntryMap.Add(entry.BatchData.Id, entry);
            }
            return cellEntryMap;
        }
        public App() {
            parameters.ClientId = CLIENT_ID;
            parameters.ClientSecret = CLIENT_SECRET;
            parameters.RedirectUri = REDIRECT_URI;
            parameters.Scope = SCOPE;
        }
    }
}

