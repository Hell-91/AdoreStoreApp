using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Http;
using AdoreStoreApp.Models;

namespace AMSFacebookApp.App_Code
{
    public class ConnectorHelperClass
    {
        private string URL;
        private string GUID;
        private XmlDocument doc;
        private List<string> returnValues;

        public ConnectorHelperClass()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Users\mv250132\Downloads\AMSApps\AMSFacebookApp\AMSFacebookApp\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        public List<string> GetOffersOptable(string authToken)
        {
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
            var flag = false;

            foreach (XmlNode no in nodeList)
            {
                flag = false;
                var offerid = "";
                var offerflag = "false";
                var offeruserflag = "false";
                foreach (XmlNode n in no.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "OfferID":
                            offerid = n.InnerText;
                            break;

                        case "OfferOptable":
                            if (bool.Parse(n.InnerText))
                            {
                                flag = true;
                                offerflag = n.InnerText;
                            }                            
                            break;

                        case "OfferOptedIn":
                            if (flag)
                            {
                                offeruserflag = n.InnerText;
                            }
                            break;
                    }
                }
                returnData.Add(offerid + "," + offerflag + "," + offeruserflag + ";");
            }
            return returnData;
        }

        /*public OfferDetailsViewModel GetOfferDetails(int offerId, string authToken)
        {
            OfferDetailsViewModel returnData = new OfferDetailsViewModel();

            returnData.OfferDetails = dbConnection.Offers.Where(o => o.ExternalOfferId == offerId).FirstOrDefault();
            returnData.OfferImagePath = null;
            returnData.OfferImageDescription = null;

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
                                if (!System.IO.File.Exists(@"C:\Users\mv250132\Downloads\AMSApps\AMSFacebookApp\AMSFacebookApp\Images\" + offerId + ".jpg"))
                                {
                                    System.IO.File.WriteAllBytes(@"C:\Users\mv250132\Downloads\AMSApps\AMSFacebookApp\AMSFacebookApp\Images\" + offerId + ".jpg", data);
                                }
                                returnData.OfferImagePath = @"\Images\" + offerId + ".jpg";
                            }
                            break;
                    }
                }
            }
            return returnData;
        }
        */

        public List<String> Login(string ExtIdentifier, string Password)
        {
            Login_Logix(ExtIdentifier, Password);
            return returnValues;
        }

        private void Login_Logix(string ExtIdentifier, string Password)
        {
            GUID = doc.SelectSingleNode("/Settings/GUIDs/Channel").InnerText;
            returnValues = new List<string>();

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
        }
    }
}