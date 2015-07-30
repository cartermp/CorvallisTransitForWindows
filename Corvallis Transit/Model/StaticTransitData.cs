using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corvallis_Transit.Model
{
    public class StaticTransitData
    {
        public Dictionary<string, Route> Routes { get; set; }
        public Dictionary<int, Stop> Stops { get; set; }
    }
}
