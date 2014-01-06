using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;
using AdoreStoreApp.Models;
using System.Xml;
using System.Net;
using System.IO;
using System;
using AdoreStoreApp.ViewModels;
using System.Web;
using System.Text;
using System.Xml.Linq;

namespace AdoreStoreApp.Controllers
{
    public class FacebookHomeController : Controller
    {
        private string URL;
        private string GUID;
        XmlDocument doc;       
        
        public FacebookHomeController()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        [FacebookAuthorize("email", "user_photos")]
        public async Task<ActionResult> Index(FacebookContext context)
        {
            if (ModelState.IsValid)
            {
                var user = await context.Client.GetCurrentUserAsync<MyAppUser>();
                var details = Login_Logix(user.Email, "password");

                Response.Cookies["Username"].Value = user.Email;
                Response.Cookies["Username"].Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies["AuthTokens"].Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies["AuthTokens"].Value = details[0];
                Response.Cookies["UserId"].Value = details[1];
                Response.Cookies["UserId"].Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies["IsFacebook"].Value = "True";
                Response.Cookies["IsFacebook"].Expires = DateTime.Now.AddMinutes(10);
                Response.Redirect("/AdoreStoreApp/");
            }

            return View("Error");
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


        // This action will handle the redirects from FacebookAuthorizeFilter when 
        // the app doesn't have all the required permissions specified in the FacebookAuthorizeAttribute.
        // The path to this action is defined under appSettings (in Web.config) with the key 'Facebook:AuthorizationRedirectPath'.
        public ActionResult Permissions(FacebookRedirectContext context)
        {
            if (ModelState.IsValid)
            {
                return View(context);
            }

            return View("Error");
        }


    }
}
