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
    public class HomeController : Controller
    {
        private string URL;
        private string GUID;
        XmlDocument doc;       
        
        public HomeController()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        public ActionResult Index()
        {
            if (ModelState.IsValid)
            {
                HomeViewModel viewModel = new HomeViewModel();
                viewModel.Offers = new List<Offer>();

                if (Request.Cookies.AllKeys.Contains("IsFacebook") && Request.Cookies["IsFacebook"].Value == "True")
                {
                    var authToken = Request.Cookies["AuthTokens"].Value;
                    viewModel.Offers = GetOffers(authToken);
                }
                else if (Request.Cookies.AllKeys.Contains("AuthTokensHome") && Request.Cookies["AuthTokensHome"].Value != null)
                {
                    var authToken = Request.Cookies["AuthTokensHome"].Value;
                    viewModel.Offers = GetOffers(authToken);
                }
                else
                { 
                    Response.Redirect("/AdoreStoreApp/Login");
                }
                return View(viewModel);
            }

            return View("Error");
        }

        private List<Offer> GetOffers(string authToken)
        {
            List<Offer> Offers = new List<Offer>();

            List<string> returnData = new List<string>();
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var imageDetails = URL + "/connectors/channels/channel.asmx/GetOfferList?GUID=" + GUID + "&AuthToken=" + authToken + "&PageNum=1";
            WebRequest request = WebRequest.Create(imageDetails);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var responseReader = new StreamReader(responseStream);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(responseReader);
            var nodeList = xmlDocument.SelectNodes("/GetOfferList/Offers/Offer");

            foreach (XmlNode no in nodeList)
            {
                Offer tempOffer = new Offer();
                foreach (XmlNode n in no.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "OfferID":
                            tempOffer.OfferId = int.Parse(n.InnerText);
                            break;

                        case "OfferOptedIn":
                            tempOffer.OfferOpted = bool.Parse(n.InnerText);
                            break;
                    }
                }
                var graphic = GetOfferGraphic(authToken, tempOffer.OfferId);
                var details = GetOfferDetails(tempOffer.OfferId);
                tempOffer.OfferName = details[0];
                tempOffer.OfferDescription = details[1];
                tempOffer.OfferGraphic = graphic != "" ? graphic : "";
                Offers.Add(tempOffer);
            }
            return Offers;
        }

        private string GetOfferGraphic(string authToken, int offerId)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var imageDetails = URL + "/connectors/channels/channel.asmx/GetChannelOfferDetails?GUID=" + GUID + "&AuthToken=" + authToken + "&OfferIDsXML=%3CGetChannelOfferDetails%3E%3COfferIDs%3E%3COfferID%3E" + offerId + "%3C/OfferID%3E%3C/OfferIDs%3E%3C/GetChannelOfferDetails%3E";
            WebRequest request = WebRequest.Create(imageDetails);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var responseReader = new StreamReader(responseStream);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(responseReader);
            var nodeList = xmlDocument.SelectNodes("/GetChannelOfferDetails/Offers/Offer/Media/MediaAsset");
            var flag = false;

            foreach (XmlNode no in nodeList)
            {
                foreach (XmlNode n in no.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "MediaFormat":
                            if (n.InnerText == "JPEG")
                            {
                                flag = true;
                            }
                            break;

                        case "MediaData":
                            if (flag)
                            {
                                byte[] data = Convert.FromBase64String(n.InnerText);
                                string decodedString = Encoding.UTF8.GetString(data);
                                var path = Server.MapPath(@"\AdoreStoreApp\Images\") + offerId + ".jpg";
                                if (!System.IO.File.Exists(path))
                                {
                                    System.IO.File.WriteAllBytes(path, data);
                                }
                                return offerId + ".jpg";
                            }
                            break;
                    }
                }
            }
            return "";
        }

        private List<string> GetOfferDetails(int offerId)
        {
            List<string> returnData = new List<string>();

            GUID = doc.SelectSingleNode("/Settings/GUIDs/UnivOffer").InnerText;
            var imageDetails = URL + "/connectors/UniversalOfferConnector.asmx/GetOfferData?GUID=" + GUID + "&ExtInterfaceID=0&EngineID=9&OfferID=" + offerId;
            WebRequest request = WebRequest.Create(imageDetails);
            request.Method = "GET";
            WebResponse response = request.GetResponse();

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
            {
                string Result = reader.ReadToEnd();
                var finalResult = HttpUtility.HtmlDecode(Result);
                var xmlData = "<root>";
                xmlData += finalResult.Split(new char[] { '>' }, 4)[3];
                xmlData = xmlData.Remove(xmlData.Length - 9);
                xmlData += "</root>";
                XDocument Doc = XDocument.Parse(xmlData);

                returnData.Add(Doc.Root.Elements("Offer").Elements("Header").Elements("IncentiveName").FirstOrDefault().Value);
                returnData.Add(Doc.Root.Elements("Offer").Elements("Header").Elements("Description").FirstOrDefault().Value.Split(';')[0]);
                returnData.Add(Doc.Root.Elements("Offer").Elements("Header").Elements("StartDate").FirstOrDefault().Value);
                returnData.Add(Doc.Root.Elements("Offer").Elements("Header").Elements("EndDate").FirstOrDefault().Value);
                var data = Doc.Root.Elements("Offer").Elements("Header").Elements("Description").FirstOrDefault().Value.Split(';');
                if (data.Count() == 3 && data[1] != string.Empty)
                {
                }
            }

            return returnData;
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

        public PartialViewResult OptIn(int id)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var authToken = "";
            if (Request.Cookies["IsFacebook"].Value == "True")
            {
                authToken = Request.Cookies["AuthTokens"].Value;
            }
            else
            {
                authToken = Request.Cookies["AuthTokensHome"].Value;            
            }
            var imageDetails = URL + "/connectors/channels/channel.asmx/OptInToOffer?GUID=" + GUID + "&AuthToken=" + authToken + "&OfferID=" + id;
            WebRequest request = WebRequest.Create(imageDetails);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var responseReader = new StreamReader(responseStream);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(responseReader);
            var nodeList = xmlDocument.SelectSingleNode("/OptInToOffer/Status");
            var ResponseCode = nodeList.Attributes["responsecode"].Value;
            if (ResponseCode == "SUCCESS")
            {
                return PartialView();
            }
            return PartialView();
        }

        public PartialViewResult OptOut(int id)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var authToken = "";
            if (Request.Cookies["IsFacebook"].Value == "True")
            {
                authToken = Request.Cookies["AuthTokens"].Value;
            }
            else
            {
                authToken = Request.Cookies["AuthTokensHome"].Value;
            } 
            var imageDetails = URL + "/connectors/channels/channel.asmx/OptOutOfOffer?GUID=" + GUID + "&AuthToken=" + authToken + "&OfferID=" + id;
            WebRequest request = WebRequest.Create(imageDetails);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var responseReader = new StreamReader(responseStream);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(responseReader);
            var nodeList = xmlDocument.SelectSingleNode("/OptOutOfOffer/Status");
            var ResponseCode = nodeList.Attributes["responsecode"].Value;
            if (ResponseCode == "SUCCESS")
            {
                return PartialView();
            }
            return PartialView();
        }
    }
}
