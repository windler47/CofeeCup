using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using System.Windows;

namespace testApp {
    class Program {
        static void Main(string[] args) {
            string appFileName = Environment.GetCommandLineArgs()[0];
            string directory = Path.GetDirectoryName(appFileName);
            //string envPath = Environment.GetEnvironmentVariable("PATH");
            //Environment.SetEnvironmentVariable(envPath + ";" + yourPath);
            //edits the PATH environment variable for the current process.
            //
            string exeDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileName = System.IO.Path.Combine(exeDirectory, System.IO.Path.Combine("Administration", "adm.txt"));
        }
    }
}