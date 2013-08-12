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

namespace CoffeeCup
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        string CLIENT_ID = "19361090870.apps.googleusercontent.com";
        string CLIENT_SECRET = "CZuF5r88V_6JGsP3pFlnoYDl";
        string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";
        string SCOPE = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds/"; 
        public OAuth2Parameters parameters = new OAuth2Parameters();
        public string DocUri; //Document key
        public string wsID; //WorksheetID
        public string docPath; //Document Path
        GOAuth2RequestFactory GRequestFactory;
        SpreadsheetsService GSpreadsheetService;
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
    }
}

