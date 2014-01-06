using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace AdoreStoreApp.Controllers
{
    public class LoginController : Controller
    {
        private string URL;
        private string GUID;
        XmlDocument doc;       
        
        public LoginController()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        public ActionResult Index()
        {
            return View();
        }

        public void Login()
        {
            if (ModelState.IsValid)
            {
                var details = Login_Logix(Request.Cookies["Username"].Value, "password");
                Response.Cookies["Username"].Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies["AuthTokensHome"].Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies["AuthTokensHome"].Value = details[0];
                Response.Cookies["UserId"].Value = details[1];
                Response.Cookies["UserId"].Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies["IsFacebook"].Value = "False";
                Response.Cookies["IsFacebook"].Expires = DateTime.Now.AddMinutes(10);
                Response.Redirect("/AdoreStoreApp/");
            }
        }

        private List<string> Login_Logix(string ExtIdentifier, string Password)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var returnValues = new List<string>();

            var _url = URL + "/connectors/channels/channel.asmx/Logon?GUID=" + GUID + "&ExtIdentifier=" + ExtIdentifier + "&ExtIDType=6&Password=" + Password;
            WebRequest request = WebRequest.Create(_url);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var responseReader = new StreamReader(responseStream);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(responseStream);
            var nodeList = xmlDocument.SelectNodes("/Logon/Customer");

            foreach (XmlNode no in nodeList)
            {
                foreach (XmlNode n in no.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "AuthToken":
                            returnValues.Add(n.InnerText);
                            break;
                    }
                }
            }

            GUID = doc.SelectSingleNode("/Settings/GUIDs/CustInq").InnerText;
            _url = URL + "/connectors/CustomerInquiry.asmx/GetCustomerRecordWithCardTypes?GUID=" + GUID + "&CardID=" + ExtIdentifier + "&CardTypeID=6";

            request = WebRequest.Create(_url);
            request.Method = "GET";
            response = request.GetResponse();
            responseStream = response.GetResponseStream();
            responseReader = new StreamReader(responseStream);

            xmlDocument = new XmlDocument();
            xmlDocument.Load(responseStream);
            nodeList = xmlDocument.SelectNodes("/Customers/Customer/Cards");

            foreach (XmlNode no in nodeList)
            {
                foreach (XmlElement report in no.SelectNodes("Card"))
                {
                    if (report.GetAttribute("type") == "Customer card")
                    {
                        returnValues.Add(report.GetAttribute("id"));
                    }
                }
            }
            return returnValues;
        }

    }
}
