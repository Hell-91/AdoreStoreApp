using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdoreStoreApp.Models;
using AdoreStoreApp.ViewModels;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace AdoreStoreApp.Controllers
{
    public class OffersController : Controller
    {
        private string URL;
        private string GUID;
        XmlDocument doc;
        
        
        public OffersController()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        public ActionResult Index()
        {
            var userId = Request.Cookies["AuthTokens"].Value;
            var data = GetOffers(userId);            
            return View(data);
        }

        private OffersViewModel GetOffers(string authToken)
        {
            OffersViewModel offers = new OffersViewModel();
            offers.Offers = new List<Offer>();

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
                offers.Offers.Add(tempOffer);
            }
            return offers;
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
                                var path = Server.MapPath(@"\Images\") + offerId + ".jpg";
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

        public ActionResult Details(int id)
        {
            Offer offer = new Offer();
            var authToken = Request.Cookies["AuthTokens"].Value;            
            var data = GetOfferDetails(id);
            var graphic = GetOfferGraphic(authToken, id);
            offer.OfferId = id;
            offer.OfferName = data[0];
            offer.OfferDescription = data[1];
            offer.OfferStartDate = DateTime.Parse(data[2]);
            offer.OfferEndDate = DateTime.Parse(data[3]);
            offer.OfferGraphic = graphic;
            return View(offer);
        }

        public ActionResult OptIn(int id)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var authToken = Request.Cookies["AuthTokens"].Value;
            var imageDetails = URL + "/connectors/channels/channel.asmx/OptInToOffer?GUID="+ GUID +"&AuthToken=" + authToken + "&OfferID=" + id;
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
                Response.Redirect("/AdoreStoreApp/");              
            }
            return View(); 
        }

        public ActionResult OptOut(int id)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            var authToken = Request.Cookies["AuthTokens"].Value;
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
                Response.Redirect("/AdoreStoreApp/");
            }
            return View();
        }
    }
}
