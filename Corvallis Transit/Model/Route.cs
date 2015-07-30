using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Corvallis_Transit.Model
{
    public class Route : NavMenuItem
    {
        public string RouteNo { get; set; }
        public string Lable { get; set; }
        public List<int> Path { get; set; }
        public string Color { get; set; }
        public string Url { get; set; }
        public string Polyline { get; set; }
        public List<BasicGeoposition> PolyLinePositions { get; internal set; }
    }
}
