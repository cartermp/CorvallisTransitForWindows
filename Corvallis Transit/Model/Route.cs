using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corvallis_Transit.Model
{
    public class Route
    {
        public string RouteNo { get; set; }
        public string Lable { get; set; }
        public List<int> Path { get; set; }
        public string Color { get; set; }
        public string Url { get; set; }
        public string Polyline { get; set; }
    }
}
