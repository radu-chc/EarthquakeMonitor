using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EarthquakeMonitor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    

    public partial class MainWindow : Window {

        static DateTime latestStartDate;
        static List<City> cities;
        const int updateIntervalSec = 30;
        const int noHours = 1;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow() {
            InitializeComponent();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            latestStartDate = DateTime.UtcNow.AddHours(-noHours);
            EarthquakeBeat();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e) {
            cities = City.ReadCities();
            foreach (EarthquakeView eqv in Earthquakes.Items) {
                if (String.IsNullOrEmpty(eqv.ClosestCities) || eqv.ClosestCities == "Processing...") {
                    var coord = new GeoCoordinate(eqv.Latitude, eqv.Longitude);
                    eqv.ClosestCities = cities.OrderBy(x => x.geocoordinate.GetDistanceTo(coord))
                           .Take(3).Select(x => x.Name).Aggregate((current, next) => current + ", " + next);
                }
            }

        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e) {
            FileStatusMessage.Content = "World Cities CSV loaded.";
        }

        private void EarthquakeBeat() {
            var timer = new SmartDispatcherTimer();
            timer.IsReentrant = false;
            timer.Interval = TimeSpan.FromSeconds(updateIntervalSec);
            timer.TickTask = async () => {
                await updateUI();
            };
            updateUI();
            timer.Start();
        }


        private async Task updateUI() {
            ApiStatusMessage.Content = "Requesting Earthquake Data...";  // MVVM property
            DateTime endDate = DateTime.UtcNow;
            List<Earthquake> latestEarthquakes = await EarthquakeSearch.GetEarthquakes(latestStartDate, endDate);
            latestStartDate = endDate;

            foreach(Earthquake eq in latestEarthquakes) {
                EarthquakeView eqv = new EarthquakeView();
                DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
                eqv.Date = dtDateTime.AddMilliseconds(eq.features.time).ToLocalTime();
                eqv.Magnitude = eq.features.mag;
                eqv.Longitude = eq.geometry.coordinates[0];
                eqv.Latitude = eq.geometry.coordinates[1];
                eqv.Depth = eq.geometry.coordinates[2];

                var coord = new GeoCoordinate(eqv.Latitude, eqv.Longitude);

                if (cities != null) {
                    eqv.ClosestCities = cities.OrderBy(x => x.geocoordinate.GetDistanceTo(coord))
                           .Take(3).Select(x => x.Name).Aggregate((current, next) => current + ", " + next);
                }
                Earthquakes.Items.Add(eqv);
            }

            ApiStatusMessage.Content = "Earthquake Data Updated at " + DateTime.Now;
        }
    }

    class EarthquakeView : INotifyPropertyChanged {
        public DateTime Date { get; set; }
        public Double Magnitude { get; set; }
        public Double Longitude { get; set; }
        public Double Latitude { get; set; }
        public Double Depth { get; set; }

        private string _closestCities = "Processing...";
        public String ClosestCities{
            get { return _closestCities; }
            set {
                _closestCities = value;
                NotifyPropertyChanged("ClosestCities");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


    }
}
