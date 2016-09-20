using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using EarthquakeMonitor.DesktopClient.Utilities;

namespace EarthquakeMonitor.DesktopClient.Models {

    class City {
        public GeoCoordinate Geocoordinate { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Name { get; set; }

        public City(double latitude, double longitude, string name) {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Name = name;

            this.Geocoordinate = new GeoCoordinate(latitude, longitude);
        }
    }
}
