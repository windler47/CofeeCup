using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Globalization;

namespace testApp {
    class Program {
        static void Main(string[] args) {
            CultureInfo t = new CultureInfo("en-US");
            double d = Convert.ToDouble("1234.5",t);
            Console.Write(d);
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(directory);
            DirectoryInfo dir = new DirectoryInfo(directory); 
            foreach (FileInfo files in dir.GetFiles("*.xml")) {
                if (files.Name.Substring(0, 4) == "test") continue;
                Console.WriteLine(files.Name);
                Console.WriteLine("=============");
                XElement xmlDoc = XElement.Load(files.Name);
                IEnumerable<XElement> org = from fobject in xmlDoc.Elements("Объект")
                                            where (string)fobject.Attribute("ИмяПравила") == "Организации"
                                            select fobject;
                Console.WriteLine("Организации:");
                foreach (XElement organization in org) {
                    Console.WriteLine(organization.Element("Свойство").Element("Значение").Value);
                }
                Console.WriteLine("Склады:");
                IEnumerable<XElement> war = from fobject in xmlDoc.Elements("Объект")
                                            where (string)fobject.Attribute("ИмяПравила") == "Склады"
                                            select fobject;
                foreach (XElement wrah in war) {
                    XElement prop = wrah.Element("Свойство");
                    Console.WriteLine(prop.Element("Значение").Value);
                }
                Console.WriteLine("=============");
            }
            Console.Read();
        }
    }
}