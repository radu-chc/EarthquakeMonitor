using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

namespace EarthquakeMonitor {

    class EarthquakeSearch {

        public static async Task<List<Earthquake>> GetEarthquakes(DateTime starttime, DateTime endtime) {
            string apiUrl = "http://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson";
            apiUrl += "&starttime=" + starttime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            apiUrl += "&endtime=" + endtime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

            using (var client = new HttpClient()) {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode) {
                    string result = await response.Content.ReadAsStringAsync();

                    var rootResult = JsonConvert.DeserializeObject<RootObject>(result);
                    return rootResult.earthquakes;
                }
                else {
                    return null;
                }
            }
        }
    }
}
