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

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        #region Google staff
        const string CLIENT_ID = "19361090870.apps.googleusercontent.com";
        const string CLIENT_SECRET = "CZuF5r88V_6JGsP3pFlnoYDl";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";
        const string SCOPE = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds/";
        GOAuth2RequestFactory GRequestFactory;
        SpreadsheetsService GSpreadsheetService;
        public OAuth2Parameters parameters = new OAuth2Parameters();
        #endregion
        public string DocUri; //Document key
        public string docPath; //Document Path
        public XElement xmlDoc; 
        public string GetGAuthLink()
        {
            parameters.ClientId = CLIENT_ID;
            parameters.ClientSecret = CLIENT_SECRET;
            parameters.RedirectUri = REDIRECT_URI;
            parameters.Scope = SCOPE;
            return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
        }
        public void GAuthStep2()
        {
            OAuthUtil.GetAccessToken(parameters);
            GRequestFactory = new GOAuth2RequestFactory(null, "CoffeeCup", parameters);
            GSpreadsheetService = new SpreadsheetsService("CoffeeCup");
            GSpreadsheetService.RequestFactory = GRequestFactory;
        }
        public bool InitializedocPath()
        {
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
        public List<Realization> GetRealisations(Dictionary<int, Customer> Customers, Dictionary<int, Product> Products) {
            List<Realization> result = new List<Realization>();
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
                IEnumerable<XElement> Records = (from elements in ProductRealisation.Elements("Табличная часть")
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
                                                where (string)prop.Attribute("Имя") == "Цена"
                                                select prop).Single().Element("Значение").Value);
                    sp.NDS = Convert.ToDouble((from prop in record.Elements()
                                               where (string)prop.Attribute("Имя") == "СуммаНДС"
                                               select prop).Single().Element("Значение").Value);
                    document.SellingPositions.Add(sp);
                }
                #endregion
                result.Add(document);
            }
            return result;
        }
    }
}

