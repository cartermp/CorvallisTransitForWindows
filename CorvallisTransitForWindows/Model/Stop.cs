using System;
using static System.Math;

namespace CorvallisTransitForWindows.Model
{
    /// <summary>
    /// Represents a Stop in the Corvallis Transit system.
    /// </summary>
    public class Stop
    {
        /// <summary>
        /// Stop name as following CTS conventions (street & cross-street).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The primary road the stop is located on.
        /// </summary>
        public string Road { get; set; }

        /// <summary>
        /// The bearing (as if on a map) of the stop.
        /// </summary>
        public float Bearing { get; set; }

        /// <summary>
        /// Whether or not a bus adheres to the scheduled time for this stop.
        /// </summary>
        public bool AdherencePoint { get; set; }

        /// <summary>
        /// Latitude.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Longitude.
        /// </summary>
        public double Long { get; set; }

        /// <summary>
        /// Unique numeric ID for this stop.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The time one can expect a bus to arrive at this stop.  Not necessarily the scheduled time.
        /// </summary>
        public DateTime ExpectedTime { get; set; }

        /// <summary>
        /// The time that a bus is supposed to arrive at this stop.
        /// </summary>
        public DateTime ScheduledTime { get; set; }

        /// <summary>
        /// The number of minutes one can expect a bus to arrive at this stop.
        /// </summary>
        public int ETA => (ExpectedTime - DateTime.Now).Minutes;

        /// <summary>
        /// Displays the ETA depending on how close the bus is to a stop.
        /// </summary>
        public string ETADisplayText
        {
            get
            {
                int eta = ETA;
                string text = "";

                if (eta > 1)
                {
                    text = eta + " mins";
                }
                else if (eta < 1 && eta >= 0)
                {
                    int seconds = (ExpectedTime - DateTime.Now).Seconds;

                    if (seconds > 30)
                    {
                        text = seconds + "seconds";
                    }
                    else
                    {
                        text = "N/A";
                    }
                }
                else
                {
                    text = "Passed Stop";
                }

                return text;
            }
        }
    }
}
