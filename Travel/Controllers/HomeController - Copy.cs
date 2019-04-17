using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Travel.Data;
using unirest_net.http;

namespace Travel.Controllers
{
    public class HomeController : Controller
    {
        private readonly string rapidApiHost;

        public readonly string rapidApiKey;
        public readonly string baseUrl;

        public HomeController()
        {
            rapidApiHost = ConfigurationManager.AppSettings["X-RapidAPI-Host"];
            rapidApiKey=ConfigurationManager.AppSettings["X-RapidAPI-Key"];
            baseUrl = "https://skyscanner-skyscanner-flight-search-v1.p.rapidapi.com/apiservices";
        }
            
        public ActionResult Index()
        {
            ViewBag.Message = "Your page.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
         

            return View();
        }

        public async Task<JsonResult> GetQuotes()
        {
            List<TravelData> travelData = new List<TravelData>();
            var date = DateTime.Now;

            for (int i = 0; i < 5; i++)
            {
                date = date.AddDays(i);
                
                ExtractQuotes("SIN - KUL", await GetQuote("SIN", "KUL", date), ref travelData);
                ExtractQuotes("KUL - SIN", await GetQuote("KUL", "SIN", date), ref travelData);
                ExtractQuotes("KUL - SFO", await GetQuote("KUL", "SFO", date), ref travelData);
            }

            return Json(travelData, JsonRequestBehavior.AllowGet);
        }

        private void ExtractQuotes(string route, Quoates data, ref List<TravelData> travelData)
        {
            if (data.Quotes.Count > 0)
            {
                travelData.Add(new TravelData
                {
                    title = $"{route} (${data.Quotes[0].MinPrice})",
                    start = data.Quotes[0].OutboundLeg.DepartureDate.ToString("yyyy-M-d"),
                    end = data.Quotes[0].OutboundLeg.DepartureDate.ToString("yyyy-M-d")
                });
            }
        }

        public async Task<Quoates> GetQuote(string originPlace, string destinationPlace, DateTime date)
        {
            string strDate = date.ToString("yyyy-MM-dd");
            string endpoint = $"{baseUrl}/browsequotes/v1.0/US/USD/en-US/{originPlace}-sky/{destinationPlace}-sky/{strDate}";

            var data = await Unirest.get(endpoint)
            .header("X-RapidAPI-Host", rapidApiHost)
            .header("X-RapidAPI-Key", rapidApiKey)
            .asStringAsync();

            var quotes =  JsonConvert.DeserializeObject<Quoates>(data.Body);
            return quotes;
        }
    }
}