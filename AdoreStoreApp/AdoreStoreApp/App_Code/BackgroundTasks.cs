using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Net;
using System.IO;
using AdoreStoreApp.Models;

namespace AMSFacebookApp
{
    public class BackgroundTasks
    {

        private string URL;
        private string GUID;
        XmlDocument doc;

        public BackgroundTasks()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Users\mv250132\Downloads\AMSApps\AMSFacebookApp\AMSFacebookApp\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        public void UpdateUserDetails(string userId)
        {
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
            GUID = doc.SelectSingleNode("/Settings/GUIDs/CustInq").InnerText;

            var _url = URL + "/connectors/CustomerInquiry.asmx/GetCustomerRecordWithCardTypes?GUID=" + GUID + "&CardID=" + userId + "&CardTypeID=0";

            var request = WebRequest.Create(_url);
            request.Method = "GET";
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            var responseReader = new StreamReader(responseStream);


            var xmlDocument = new XmlDocument();
            xmlDocument.Load(responseReader);
            var nodeList = xmlDocument.SelectNodes("/Customers/Customer/GeneralInformation");

            /*Profile p = new Profile();

            foreach (XmlNode no in nodeList)
            {
                foreach (XmlNode n in no.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "FirstName":
                            p.FirstName = n.InnerText;
                            break;

                        case "LastName":
                            p.LastName = n.InnerText;
                            break;

                        case "Email":
                            p.Email = n.InnerText;
                            break;

                        case "Phone":
                            p.PhoneNo = n.InnerText;
                            break;

                    }
                }
            }
            u.ModifiedDate = DateTime.Now;
            u.CreatedDate = DateTime.Now;

            bool customerFlag = true;

            if (dbConnection.Users.Count() > 0)
            {
                User tempUser = null;
                if ((tempUser = dbConnection.Users.Where(user => user.CardId == u.CardId).FirstOrDefault()) != null)
                {
                    customerFlag = false;
                    if (tempUser.ModifiedDate != u.ModifiedDate)
                    {
                        tempUser.ModifiedDate = u.ModifiedDate;
                        tempUser.FirstName = u.FirstName;
                        tempUser.LastName = u.LastName;
                        tempUser.PhoneNo = u.PhoneNo;
                        tempUser.Email = u.Email;
                    }
                }

            }

            if (customerFlag)
            {
                dbConnection.Users.Add(u);
                dbConnection.SaveChanges();
            }
            */

        }
    }
}