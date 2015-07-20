using CorvallisTransitForWindows.Controls;
using CorvallisTransitForWindows.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using CorvallisTransitForWindows.Util;
using Windows.ApplicationModel.Core;

namespace CorvallisTransitForWindows
{
    /// <summary>
    /// The Main Page, containing the list of Routes and the container for when a Route is selected.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        //
        // Yes, globals like this suck, but it's the best way (that I can figure out) to handle all these types of events.
        //
        private static Route SelectedRoute;
        private static Stop SelectedStop;

        private static string ROUTES_URL = "http://www.corvallis-bus.appspot.com/routes";
        private static string ARRIVALS_URL = "http://www.corvallis-bus.appspot.com/arrivals?stops=";

        private HttpClient httpClient = new HttpClient();

        public MainPage()
        {
            InitializeComponent();
            Current = this;
        }

        /// <summary>
        /// Grabs route data from the server the instant the page is navigated to.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var getRoutesTask = Task.Run(() => GetRoutesAsync());
            getRoutesTask.Wait();

            var routes = getRoutesTask.Result;

            foreach (var route in routes)
            {
                if (route.Name.Contains("BB"))
                {
                    route.Name = route.Name.Replace("BB", "NO");
                }

                route.PolyLinePositions = PolylineToLocations(route.Polyline);
                route.Label = "CTS Route " + route.Name;
                route.ShortLabel = route.Name;
            }

            NavMenuList.ItemsSource = routes;
        }

        /// <summary>
        /// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        /// using the associated Label of each item.
        /// </summary>
        private void NavMenuList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        /// <summary>
        /// Handles displaying a route on a map when one of the routes is clicked in the Nav Menu.
        /// </summary>
        private void NavMenuList_ItemInvoked(object sender, ListViewItem e)
        {
            var list = sender as NavMenuListView;
            if (list == null)
            {
                // Sweep this problem under the rug by failing silently.
                return;
            }

            var route = (sender as NavMenuListView).ItemFromContainer(e) as Route;
            if (route == null)
            {
                // Sweep this problem under the rug by failing silently.
                return;
            }

            SelectedRoute = route;

            if (route.Path.HasContent())
            {
                // Clear out anything else, otherwise we end up with one ugly map.
                RouteMap.MapElements.Clear();

                DrawRoutePolyline(route);

                var routeCenter = new BasicGeoposition()
                {
                    // Best attempt to center the route on the page is to average Lat/Longs
                    Latitude = route.Path.Select(s => s.Lat).Average(),
                    Longitude = route.Path.Select(s => s.Long).Average()
                };

                Task.Run(() => RouteMap.TrySetViewAsync(new Geopoint(routeCenter), 14, 0, 0, MapAnimationKind.Bow));

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
        /// Sets the location, zooms the map, and loads up the polylines for the routes (a cosmetic feature for when it's first opened).
        /// </summary>
        private async void RouteMap_Loaded(object sender, RoutedEventArgs e)
        {
            var corvallis = GetCorvallisLocation();
            await RouteMap.TrySetViewAsync(corvallis, 13.3, 0, 0, MapAnimationKind.Linear);

            foreach (var route in NavMenuList.ItemsSource as List<Route>)
            {
                DrawRoutePolyline(route);
            }
        }

        private Geopoint GetCorvallisLocation()
        {
            return new Geopoint(new BasicGeoposition() { Latitude = 44.565918, Longitude = -123.276417 });
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
        /// Converts a Google Maps polyline string into a list of Lat/Longs.
        /// 
        /// Copy-pasted from here:
        /// http://www.codeproject.com/Tips/312248/Google-Maps-Direction-API-V-Polyline-Decoder
        /// </summary>
        private static List<BasicGeoposition> PolylineToLocations(string polyLine)
        {
            if (polyLine.IsNullOrWhiteSpace())
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

        private void RouteMap_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            // Just ignore anything that isn't a stop marker.
            if (!args.MapElements.Any(me => me is MapIcon))
            {
                return;
            }

            if (SelectedRoute != null)
            {
                var stop = SelectedRoute.Path.FirstOrDefault(s => AreLocationsTheSame(s, args.Location));
                if (stop != null)
                {
                    var arrivalsTask = Task.Run(() => GetArrivalsAsync(stop.Id));
                    arrivalsTask.Wait();

                    var arrival = arrivalsTask.Result.FirstOrDefault();
                    if (arrival != null)
                    {
                        stop.ExpectedTime = arrival.Expected;

                        ETAItem.Text = stop.ETADisplayText;

                        FlyoutBase.ShowAttachedFlyout(RootSplitView);

                        // Set this here so that Launching Maps for directions has the most current stop.
                        SelectedStop = stop;
                    }
                }
            }
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

        private bool AreLocationsTheSame(Stop s, Geopoint location)
        {
            return Math.Abs(s.Lat - location.Position.Latitude) < 0.001 &&
                   Math.Abs(s.Long - location.Position.Longitude) < 0.001;
        }

        #region Hamburger Button Display Logic

        public Rect TogglePaneButtonRect
        {
            get;
            private set;
        }

        /// <summary>
        /// Callback when the SplitView's Pane is toggled open or close.  When the Pane is not visible
        /// then the floating hamburger may be occluding other content in the app unless it is aware.
        /// </summary>
        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            this.CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        /// An event to notify listeners when the hamburger button may occlude other content in the app.
        /// The custom "PageHeader" user control is using this.
        /// </summary>
        public event TypedEventHandler<MainPage, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        /// Check for the conditions where the navigation pane does not occupy the space under the floating
        /// hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (this.RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                this.RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = this.TogglePaneButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, this.TogglePaneButton.ActualWidth, this.TogglePaneButton.ActualHeight));
                this.TogglePaneButtonRect = rect;
            }
            else
            {
                this.TogglePaneButtonRect = new Rect();
            }

            var handler = this.TogglePaneButtonRectChanged;
            if (handler != null)
            {
                handler(this, this.TogglePaneButtonRect);
            }
        }

        #endregion

        private void DirectionsItem_Click(object sender, RoutedEventArgs e)
        {
            LaunchAndGetDirectionsFromMaps();
        }

        /// <summary>
        /// Launches Maps with walking directions from the selected stop
        /// </summary>
        private static void LaunchAndGetDirectionsFromMaps()
        {
            string uriToLaunch = string.Format("ms-walk-to:?destination.latitude={0}&destination.longitude={1}",
                                                                    SelectedStop.Lat, SelectedStop.Long);
            Task.Run(() => Launcher.LaunchUriAsync(new Uri(uriToLaunch)));
        }
    }
}
