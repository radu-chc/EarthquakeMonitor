using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace EarthquakeMonitor {
    class City {
        public GeoCoordinate geocoordinate { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Name { get; set; }

        public City(double latitude, double longitude, string name) {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Name = name;

            this.geocoordinate = new GeoCoordinate(latitude, longitude);
        }

        public static List<City> ReadCities() {
            List<City> cities = new List<City>();
            try {
                using (CsvReader reader = new CsvReader("worldcities.csv")) {
                    foreach (string[] values in reader.RowEnumerator) {
                        Double longitude;
                        Double latitude;

                        bool result1 = Double.TryParse(values[7], out latitude);
                        bool result2 = Double.TryParse(values[8], out longitude);
                        if (result1 && result2) {
                            cities.Add(new City(latitude, longitude, values[6]));
                        }

                    }
                }
            }
            catch (Exception e) {
                Console.Write(e);
            }
            return cities;
        }

        public static List<City> GetClosestCities(double longitude, double latitude, List<City> cities) {
            List<City> closestCities = new List<City>();
            closestCities.Add(cities[0]);
            closestCities.Add(cities[1]);
            closestCities.Add(cities[2]);

            return closestCities;
        }
    }
}
