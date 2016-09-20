using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeMonitor.ViewModels {

    class EarthquakeViewModel : INotifyPropertyChanged {
        public DateTime Date { get; set; }
        public Double Magnitude { get; set; }
        public Double Longitude { get; set; }
        public Double Latitude { get; set; }
        public Double Depth { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _closestCities = "Processing...";
        public String ClosestCities {
            get { return _closestCities; }
            set {
                _closestCities = value;
                NotifyPropertyChanged("ClosestCities");
            }
        }
    }
}
