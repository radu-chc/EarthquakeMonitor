using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarthquakeMonitor.Utilities;
using EarthquakeMonitor.Models;

namespace EarthquakeMonitor.ViewModels {

     class MainWindowViewModel : INotifyPropertyChanged {
        private DateTime latestStartDate;
        private List<City> cities;
        private const int updateIntervalSec = 30;
        private const int noHours = 1;
        private const int numberOfLinesInCityFile = 291938;
        private bool fileLoaded = false;

        private string _apiStatusMessage;

        public string ApiStatusMessage {
            get { return _apiStatusMessage; }
            set {
                _apiStatusMessage = value;
                OnPropertyChanged("ApiStatusMessage");
            }
        }

        private string _fileStatusMessage;

        public string FileStatusMessage {
            get { return _fileStatusMessage; }
            set {
                _fileStatusMessage = value;
                OnPropertyChanged("FileStatusMessage");
            }
        }

        private Double _fileReadProgress = 0;

        public Double FileReadProgress {
            get { return _fileReadProgress; }
            set {
                _fileReadProgress = value;
                OnPropertyChanged("FileReadProgress");
            }
        }

        private ObservableCollection<EarthquakeViewModel> _earthquakes;

        public ObservableCollection<EarthquakeViewModel> Earthquakes {
            get { return _earthquakes; }
            set {
                _earthquakes = value;
                OnPropertyChanged("Earthquakes");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private BackgroundWorker cityLoader;
        public MainWindowViewModel() {
            Earthquakes = new ObservableCollection<EarthquakeViewModel>();
            latestStartDate = DateTime.UtcNow.AddHours(-noHours);
            cities = new List<City>();

            cityLoader = new BackgroundWorker();
            cityLoader.DoWork += loadCities;
        }

        public void StartProcess() {
            // Read file in the background.
            cityLoader.RunWorkerAsync();

            // SmartDispatched - disallows renetry.
            var timer = new SmartDispatcherTimer();
            timer.IsReentrant = false;
            timer.Interval = TimeSpan.FromSeconds(updateIntervalSec);
            timer.TickTask = async () => {
                 await loadLatestEarthquakes();
            };
            timer.Start();

            // Initial load. fire and forget
            loadLatestEarthquakes();
        }

        private void loadCities(object sender, DoWorkEventArgs e) {
            FileStatusMessage = "Loading World Cities...";
            try {
                // int lineCount = File.ReadLines("Resources\\worldcities.csv").Count();
                using (CsvReader reader = new CsvReader("Resources\\worldcities.csv")) {
                    int noLinesRead = 0;
                    foreach (string[] values in reader.RowEnumerator) {
                        noLinesRead++;
                        FileReadProgress = ((Double) noLinesRead / numberOfLinesInCityFile) * 100;
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
            catch (Exception ex) {
                Console.Write(ex);
            }

            FileStatusMessage = "World Cities CSV loaded.";
            fileLoaded = true;

            // Match existing cities.
            foreach (EarthquakeViewModel eqv in Earthquakes) {
                if (String.IsNullOrEmpty(eqv.ClosestCities) || eqv.ClosestCities == "Processing...") {
                    var coord = new GeoCoordinate(eqv.Latitude, eqv.Longitude);
                    eqv.ClosestCities = cities.OrderBy(x => x.Geocoordinate.GetDistanceTo(coord))
                           .Take(3).Select(x => x.Name).Aggregate((current, next) => current + ", " + next);
                }
            }
        }


        private async Task loadLatestEarthquakes() {
            ApiStatusMessage = "Requesting Earthquake Data...";  // MVVM property
            DateTime endDate = DateTime.UtcNow;
            List<Earthquake> latestEarthquakes = await EarthquakeSearch.GetEarthquakes(latestStartDate, endDate);
            latestStartDate = endDate;

            foreach (Earthquake eq in latestEarthquakes) {
                EarthquakeViewModel eqv = new EarthquakeViewModel();
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                eqv.Date = dtDateTime.AddMilliseconds(eq.features.time).ToLocalTime();
                eqv.Magnitude = eq.features.mag;
                eqv.Longitude = eq.geometry.coordinates[0];
                eqv.Latitude = eq.geometry.coordinates[1];
                eqv.Depth = eq.geometry.coordinates[2];

                var coord = new GeoCoordinate(eqv.Latitude, eqv.Longitude);

                if (fileLoaded) {
                    eqv.ClosestCities = cities.OrderBy(x => x.Geocoordinate.GetDistanceTo(coord))
                           .Take(3).Select(x => x.Name).Aggregate((current, next) => current + ", " + next);
                }


                Earthquakes.Add(eqv);
            }

            ApiStatusMessage = "Earthquake Data Updated at " + DateTime.Now;
        }
    }
}
