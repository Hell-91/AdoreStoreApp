using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using AdoreStoreApp.ViewModels;
using AdoreStoreApp.Models;

namespace AdoreStoreApp.Controllers
{
    public class PointsController : Controller
    {
        private string URL;
        private string GUID;
        XmlDocument doc;

        public PointsController()
        {
            doc = new XmlDocument();
            doc.Load(@"C:\Settings\Settings.dat");
            URL = "http://" + doc.SelectSingleNode("/Settings/LogixURL").InnerText;
        }

        public ActionResult Index()
        {
            var userId = Request.Cookies["UserId"].Value;
            var data = GetPointsProgramList(userId);
            return View(data);
        }

        private PointsViewModel GetPointsProgramList(string userId)
        {
            PointsViewModel returnData = new PointsViewModel();
            returnData.Points = new List<PointsProgram>();

            GUID = doc.SelectSingleNode("/Settings/GUIDs/CustWeb").InnerText;
            var imageDetails = URL + "/customer/connectors/CustWeb.asmx/PointsBalancesCM?GUID=" + GUID + "&CustomerID=" + userId + "&CustomerTypeID=0";
            WebRequest request = WebRequest.Create(imageDetails);
            request.Method = "GET";
            WebResponse response = request.GetResponse();

            DataSet d = new DataSet();
            d.ReadXml(response.GetResponseStream());

            foreach (var da in d.Tables)
            {
                DataTable dataTable = (DataTable)da;
                if (dataTable.TableName == "PointsProgram")
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        PointsProgram tempPoints = new PointsProgram();
                        tempPoints.PointProgramId = int.Parse(dataRow[0].ToString());
                        tempPoints.PointProgramName = dataRow[1].ToString();
                        tempPoints.PointsAvailable = int.Parse(dataRow[3].ToString());
                        returnData.Points.Add(tempPoints);
                    }
                }
            }
            return returnData;
        }
    }
}
