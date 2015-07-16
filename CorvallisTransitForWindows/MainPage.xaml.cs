using CorvallisTransitForWindows.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;


namespace CorvallisTransitForWindows
{
    /// <summary>
    /// The Main Page, containing the list of Routes and the container for when a Route is selected.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static string ROUTES_URL = "http://www.corvallis-bus.appspot.com/routes";
        private static string ARRIVALS_URL = "http://www.corvallis-bus.appspot.com/arrivals?stops=";

        private HttpClient httpClient = new HttpClient();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var getRoutesTask = Task.Run(() => GetRoutesAsync());
            getRoutesTask.Wait();

            var routes = getRoutesTask.Result;

            foreach (var route in routes)
            {
                route.DisplayName = "Route " + route.Name;
                route.PolyLinePositions = PolylineToLocations(route.Polyline);
            }

            RoutesList.ItemsSource = routes;
        }

        /// <summary>
        /// Sets the location, zooms the map, and loads up the polylines for the routes (a cosmetic feature for when it's first opened).
        /// </summary>
        private async void RouteMap_Loaded(object sender, RoutedEventArgs e)
        {
            var corvallis = GetCorvallisLocation();
            await RouteMap.TrySetViewAsync(corvallis, 13.3, 0, 0, MapAnimationKind.Linear);

            foreach (var route in RoutesList.ItemsSource as List<Route>)
            {
                DrawRoutePolyline(route);
            }
        }

        private Geopoint GetCorvallisLocation()
        {
            return new Geopoint(new BasicGeoposition() { Latitude = 44.565918, Longitude = -123.276417 });
        }

        private void RoutesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox routesListBox = sender as ListBox;
            Route route = routesListBox.SelectedItem as Route;

            if (route != null)
            {
                var getArrivalsTask = Task.Run(() => DetermineExpectedTimesForRoute(route));

                // Clear out anything else, otherwise we end up with one ugly map.
                RouteMap.MapElements.Clear();
                DrawRoutePolyline(route);

                getArrivalsTask.Wait();

                var basicGeo = new BasicGeoposition()
                {
                    Latitude = route.Path.First().Lat,
                    Longitude = route.Path.First().Long
                };

                Task.Run(() => RouteMap.TrySetViewAsync(new Geopoint(basicGeo), 20, 0, 0, MapAnimationKind.Bow));

                foreach (var stop in route.Path)
                {
                    var pin = new MapIcon()
                    {
                        Location = new Geopoint(new BasicGeoposition()
                        {
                            Latitude = stop.Lat,
                            Longitude = stop.Long
                        })
                    };

                    RouteMap.MapElements.Add(pin);
                }
            }
        }

        /// <summary>
        /// Draws the route's GoogleMaps polyline on the map.
        /// </summary>
        private void DrawRoutePolyline(Route route)
        {
            var polyLine = new MapPolyline();
            polyLine.Path = new Geopath(route.PolyLinePositions);

            // Taken from here: http://stackoverflow.com/a/5800540
            uint argb = uint.Parse(route.Color.Replace("#", ""), NumberStyles.HexNumber);

            byte r = (byte)((argb & 0xff0000) >> 0x10);
            byte g = (byte)((argb & 0xff00) >> 8);
            byte b = (byte)(argb & 0xff);

            polyLine.StrokeColor = Color.FromArgb(/* Just make it 100% */ 0xAA, r, g, b);
            polyLine.StrokeThickness = 5;

            RouteMap.MapElements.Add(polyLine);
        }

        /// <summary>
        /// Gets arrival data for each stop in a given route.
        /// </summary>
        private void DetermineExpectedTimesForRoute(Route route)
        {
            Parallel.ForEach(route.Path, stop =>
            {
                var arrivalsTask = Task.Run(() => GetArrivalsAsync(stop.Id));
                arrivalsTask.Wait();

                // Because the server returns arrivals irrespective of route,
                // we need to filer the list down to the route we care about,
                // then take the first item in the list, as that is the arrival we care about.
                var arrival = arrivalsTask.Result.Where(a => a.Route == route.Name)
                              .OrderBy(a => a.Expected)
                              .FirstOrDefault();

                if (arrival == null)
                {
                    stop.ExpectedTime = DateTime.MinValue;
                }
                else
                {
                    stop.ExpectedTime = arrival.Expected;
                }
            });

            route.Path = route.Path.Where(s => s.ExpectedTime != DateTime.MinValue).ToList();
        }

        /// <summary>
        /// Hits the Corvallis Bus server for an updated list of routes.  Returns the deserialized JSON.
        /// </summary>
        private async Task<List<Route>> GetRoutesAsync()
        {
            var uri = new Uri(ROUTES_URL);
            var response = await httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var text = await response.Content.ReadAsStringAsync();

            text = text.Substring(10, text.Length - 11);

            // JsonConvert.DeserializeObjectAsync is strangely deprecated now...
            return await Task.Run(() => JsonConvert.DeserializeObject<List<Route>>(text));
        }

        /// <summary>
        /// Hits the Corvallis Bus server for updated arrivals for a given stop ID.
        /// </summary>
        private async Task<List<Arrival>> GetArrivalsAsync(int stopId)
        {
            var uri = new Uri(ARRIVALS_URL + stopId);
            var response = await httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var text = await response.Content.ReadAsStringAsync();

            text = text.Substring(9, text.Length - 10);

            // As above, the async version of this is strangely deprecated.
            return await Task.Run(() => JsonConvert.DeserializeObject<List<Arrival>>(text));
        }

        /// <summary>
        /// Converts a Google Maps polyline string into a list of Lat/Longs.
        /// 
        /// Copy-pasted from here:
        /// http://www.codeproject.com/Tips/312248/Google-Maps-Direction-API-V-Polyline-Decoder
        /// </summary>
        private static List<BasicGeoposition> PolylineToLocations(string polyLine)
        {
            if (string.IsNullOrWhiteSpace(polyLine))
            {
                return null;
            }

            List<BasicGeoposition> poly = new List<BasicGeoposition>();
            char[] polylinechars = polyLine.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylinechars.Length)
            {
                // calculate next latitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = polylinechars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylinechars.Length);

                if (index >= polylinechars.Length)
                {
                    break;
                }

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                //calculate next longitude
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = polylinechars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylinechars.Length);

                if (index >= polylinechars.Length && next5bits >= 32)
                {
                    break;
                }

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                BasicGeoposition p = new BasicGeoposition();
                p.Latitude = Convert.ToDouble(currentLat) / 100000.0;
                p.Longitude = Convert.ToDouble(currentLng) / 100000.0;
                poly.Add(p);
            }

            return poly;
        }

        /// <summary>
        /// Handles toggling the pane of the Master/Detail view.
        /// </summary>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            MasterDetail.IsPaneOpen = !MasterDetail.IsPaneOpen;
            // possible do more here
        }
    }
}
