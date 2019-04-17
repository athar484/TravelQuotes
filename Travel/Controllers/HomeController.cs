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
            baseUrl = ConfigurationManager.AppSettings["Endpoint"];
        }
            
        public ActionResult Index()
        {
            return View();
        }


        public async Task<JsonResult> GetQuotesAsync()
        {
            List<TravelData> travelData = new List<TravelData>();

            List<Task<List<TravelData>>> tasks = new List<Task<List<TravelData>>>();
            tasks.Add(ExtractQuotesAync("SIN", "KUL"));
            tasks.Add(ExtractQuotesAync("KUL", "SFO"));
            tasks.Add(ExtractQuotesAync("KUL", "SIN"));

            var results = await Task.WhenAll(tasks);
            foreach(var item in results)
            {
                travelData.AddRange(item);
            }            
            
            return Json(travelData, JsonRequestBehavior.AllowGet);
        }

        private async Task<List<TravelData>> ExtractQuotesAync(string originPlace, string detinationPlace)
        {
            List<TravelData> travelData = new List<TravelData>();
            var date = DateTime.Now;

            List<Task<QuotesContract>> tasks = new List<Task<QuotesContract>>();
            for (int i = 0; i < 30; i++)
            {
                date = date.AddDays(1);
                tasks.Add(GetQuoteFromAPIAsync(originPlace, detinationPlace, date));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var item in results)
            {
                if (item.Quotes.Count == 0)
                    continue;

                travelData.Add(new TravelData
                {                    
                    title=$"{item.Places[0].CityName} - {item.Places[1].CityName} (${item.Quotes[0].MinPrice})",
                    start = item.Quotes[0].OutboundLeg.DepartureDate.ToString("yyyy-MM-dd"),
                    end = item.Quotes[0].OutboundLeg.DepartureDate.ToString("yyyy-MM-dd")
                });
            }

            return travelData;
        }

        private async Task<QuotesContract> GetQuoteFromAPIAsync(string originPlace, string destinationPlace, DateTime date)
        {
            string strDate = date.ToString("yyyy-MM-dd");
            string endpoint = $"{baseUrl}/browsequotes/v1.0/US/USD/en-US/{originPlace}-sky/{destinationPlace}-sky/{strDate}";

            var data = await Unirest.get(endpoint)
            .header("X-RapidAPI-Host", rapidApiHost)
            .header("X-RapidAPI-Key", rapidApiKey)
            .asStringAsync();

            var quotes =  JsonConvert.DeserializeObject<QuotesContract>(data.Body);
            return quotes;
        }
    }
}