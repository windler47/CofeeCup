using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace testApp
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument xmlrDoc = new XmlDocument();
            XmlDocument xmlwDoc = new XmlDocument();
            xmlrDoc.Load(args[0]);
            foreach (XmlNode table in xmlrDoc.DocumentElement.ChildNodes)
            {
                Console.Write(table.Name);
                foreach (XmlAttribute attr in table.Attributes)
                {
                    Console.Write(attr.Name); Console.Write(" ");
                }
                Console.WriteLine();
            }
            xmlwDoc.CreateXmlDeclaration("1.0", null, null);

        }
    }
}
