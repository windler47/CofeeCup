using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        CCupWSFeed worksheetsFeed;
        public XElement xmlDoc;
        public Dictionary<uint, List<Realization>> realizations;
        public string appPath;
        public string GAuthGetLink() {
            return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);       
        }
        public void GAuthStep2(string accessCode) {
            parameters.AccessCode = accessCode;
            OAuthUtil.GetAccessToken(parameters);
            GOAuth2RequestFactory GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
            GSpreadsheetService.RequestFactory = GRequestFactory;
        }
        public bool WorksheetFeedInit(string spredsheetName) {
            try {
                GOAuth2RequestFactory GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
                GSpreadsheetService.RequestFactory = GRequestFactory;
                // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
                SpreadsheetQuery query = new SpreadsheetQuery();
                SpreadsheetFeed feed = null;
                // Make a request to the API and get all spreadsheets.
                try {
                    feed = GSpreadsheetService.Query(query);
                }
                catch (Exception e) {
                    MessageBox.Show(e.Message);
                    return true;
                }
                if (feed.Entries.Count == 0) {
                    MessageBox.Show("No documents found :(");
                    return true;
                }
                SpreadsheetEntry spreadsheet = null;
                foreach (AtomEntry spr in feed.Entries) {
                    if (((SpreadsheetEntry)spr).Title.Text == spredsheetName) {
                        spreadsheet = (SpreadsheetEntry)spr;
                        break;
                    }
                }
                if (spreadsheet == null) {
                    MessageBox.Show("Документ с таким названием не найден сриди доступных.");
                    return true;
                }
                worksheetsFeed = new CCupWSFeed(spreadsheet.Worksheets, GSpreadsheetService);
                return false;
            }
            catch (Exception e) {
                MessageBox.Show("Error in function WorksheetFeedInit:\n" + e.Message);
                return true;
            }
        }
        public bool InitializexmlDoc(string documentPath) {
            bool result = true;
            try {
                xmlDoc = XElement.Load(documentPath);
                result = false;
            }
            catch (ArgumentNullException) {
                MessageBox.Show("Не указан путь к файлу с данными для выгрузки");
            }
            catch (SecurityException) {
                MessageBox.Show("Недостаточно прав для обращения к файлу по адресу " + documentPath);
            }
            catch (FileNotFoundException) {
                MessageBox.Show("Файл " + documentPath + "не найден.");
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
            try {
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
                        if (Convert.ToInt32(ProductRealisation.Elements("Свойство").Where(
                            (e) => { return (string)e.Attribute("Имя") == "Склад"; }
                            ).Single().Element("Ссылка").Attribute("Нпп").Value) != warehouseCode) {
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
            catch (Exception e) {
                MessageBox.Show("Error in function 'GetRealisations':\n" + e.Message);
            }
        }
        public bool UploadData() {
            bool success = true;
            Dictionary<uint, CCupWSEntry> year_ws = new Dictionary<uint, CCupWSEntry>();
            foreach (CCupWSEntry worksheet in worksheetsFeed.EntriesList) {
                if (!year_ws.ContainsKey(worksheet.worksheetYear) && worksheet.worksheetYear != 0) {
                    year_ws.Add(worksheet.worksheetYear, worksheet);
                }
            }
            foreach (uint year in realizations.Keys) {
                if (year_ws.ContainsKey(year)) {
                    success = success && year_ws[year].UploadData(realizations[year]);
                }
                else {
                    MessageBox.Show(string.Format("Лист для выгрузки документов за {0} год не найден. Данные за этот год не будут выгружены!", year));
                    success = false;
                }
            }
            return success;
        }
        public bool LoadGRefreshToken() {
            FileStream fs;
            try {
                fs = new FileStream(Path.Combine(appPath, "cc.bin"), FileMode.Open);
            }
            catch {
                return true;
            }
#if DEBUG 
            MessageBox.Show("appPtah="+Path.Combine(appPath, "cc.bin"));
#endif
            try {
            BinaryReader br = new BinaryReader(fs);
            parameters.RefreshToken = br.ReadString();
            fs.Close();
            OAuthUtil.RefreshAccessToken(parameters);
            GOAuth2RequestFactory GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
            GSpreadsheetService.RequestFactory = GRequestFactory;
            return parameters.AccessToken == null;
            }
            catch {
                File.Delete(Path.Combine(appPath, "cc.bin"));
                return true;
            }
        }
        public void RemoveSavedRefreshToken() {
            try {
                File.Delete(Path.Combine(appPath, "cc.bin"));
            }
            // Handle exceptions by type ref = http://msdn.microsoft.com/ru-ru/library/system.io.file.delete(v=vs.110).aspx
            catch { }
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
            appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        private void SaveGRefreshToken(string refreshToken) {
            FileStream fs = new FileStream(Path.Combine(appPath, "cc.bin"), FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(refreshToken);
            fs.Close();
        }
    }
}