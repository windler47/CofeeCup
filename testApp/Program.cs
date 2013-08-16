using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string testxml = "020813РБУ.xml";
            XElement xmlDoc = XElement.Load(testxml);
            Dictionary<int, string> goods = GetGoods(xmlDoc);
            Dictionary<int, Customer> customers = GetCustomers(xmlDoc);
        }
        public static Dictionary<int, string> GetGoods(XElement xmlDoc)
        {
            Dictionary<int,string> result = new Dictionary<int,string>();
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
                string productName = (string)(from el1 in el.Elements("Свойство")
                                              where (string)el1.Attribute("Имя") == "Наименование"
                                              select el1).ElementAt(0);
                #endregion
                //Console.WriteLine("{0} Товар: {1}", el.Element("Ссылка").Attribute("Нпп"), productName);
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"),productName);
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
                result.Add((int)el.Element("Ссылка").Attribute("Нпп"),customer);
            }
            return result;
        }
    }
}
