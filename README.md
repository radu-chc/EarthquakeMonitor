#EarthquakeMonitor

WPF Desktop application which monitors and displays, in list format, the following details about ongoing earthquake activity:

* Earthquake date/time
* Earthquake magnitude
* Earthquake coordinates
* A comma-separated list of names of the three cities positioned closest to the earthquake location

When the program is first loaded, it retrieves and displays any earthquake activity from the past hour. The program then continues to monitor and display any additional earthquake activity without additional input from the user.

Refreshes data every **30 seconds**.

CSVReader source:
http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file

SmartDispatcher source:
http://stackoverflow.com/questions/12442622/async-friendly-dispatchertimer-wrapper-subclass
