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
    public static class AppPublicFunctions
    {
        public static Dictionary<int, Product> GetGoods(XElement xmlDoc)
        {
            Dictionary<int, Product> result = new Dictionary<int, Product>();
            IEnumerable<XElement> goods =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "Номенклатура"
                select fobject;

            foreach (XElement el in goods)
            {
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
        public static Dictionary<int, Customer> GetCustomers(XElement xmlDoc)
        {
            Dictionary<int, Customer> result = new Dictionary<int, Customer>();
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "Контрагенты"
                select fobject;
            foreach (XElement el in obj)
            {
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
        public static List<Realization> GetRealisations(XElement xmlDoc, Dictionary<int, Customer> Customers, Dictionary<int, Product> Products)
        {
            List<Realization> result = new List<Realization>();
            IEnumerable<XElement> obj =
                from fobject in xmlDoc.Elements("Объект")
                where (string)fobject.Attribute("ИмяПравила") == "РеализацияТоваровУслуг"
                select fobject;
            foreach (XElement ProductRealisation in obj)
            {
                bool IsDelited = (from prop in ProductRealisation.Elements("Свойство")
                                  where (string)prop.Attribute("Имя") == "ПометкаУдаления" && (string)prop.Element("Значение") == "true"
                                  select prop).Any();

                bool IsCommited = (from prop in ProductRealisation.Elements("Свойство")
                                   where (string)prop.Attribute("Имя") == "Проведен" && (string)prop.Element("Значение") == "true"
                                   select prop).Any();
                if (IsDelited || !IsCommited) continue;
                Realization document = new Realization();
                //TODO: add .single exception handling
                try
                {
                    document.Date = DateTime.Parse((string)(from prop in ProductRealisation.Element("Ссылка").Elements("Свойство")
                                                            where (string)prop.Attribute("Имя") == "Дата"
                                                            select prop).Single().Element("Значение"));
                }
                catch (FormatException)
                {
                    MessageBox.Show("Error while parsing xml date");
                    continue;
                }
                IEnumerable<XElement> Records = (from elements in ProductRealisation.Elements("Табличная часть")
                                                 where (string)elements.Attribute("Имя") == "Товары"
                                                 select elements).Single().Elements();
                #region Selling positions parsing
                foreach (XElement record in Records)
                {
                    SellingPosition sp = new SellingPosition();
                    sp.Amount = Convert.ToInt32((from prop in record.Elements()
                                                 where (string)prop.Attribute("Имя") == "Количество"
                                                 select prop).Single().Element("Значение").Value);
                    int npp = (int)(from prop in record.Elements()
                                    where (string)prop.Attribute("Имя") == "Номенклатура"
                                    select prop).Single().Element("Ссылка").Attribute("Нпп");
                    try
                    {
                        sp.Product = Products[npp];
                    }
                    catch (KeyNotFoundException)
                    {
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
