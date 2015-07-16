using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace CorvallisTransitForWindows.Model
{
    /// <summary>
    /// Represents the route a CTS bus travels on.
    /// </summary>
    public class Route : NavMenuItem
    {
        /// <summary>
        /// Name of the route (1, 2, C#, etc).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name with "Route" prepended.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The list of stops a bus passes, in the order the bus will pass them.
        /// </summary>
        public List<Stop> Path { get; set; }

        public List<BasicGeoposition> PolyLinePositions { get; set; }

        public DateTime LastStopTimeUpdated { get; set; }

        /// <summary>
        /// Polyline encoding for the route as if it were drawn on a map, colored uniquely
        /// </summary>
        public string Polyline { get; set; }

        /// <summary>
        /// Unique color for a given route.  Route colors are determined by CTS.
        /// </summary>
        public string Color { get; set; }
    }
}
