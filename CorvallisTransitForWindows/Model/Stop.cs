using System;
using static System.Math;

namespace CorvallisTransitForWindows.Model
{
    /// <summary>
    /// Represents a Stop in the Corvallis Transit system.
    /// </summary>
    public class Stop
    {
        public string Name { get; set; }

        public string Road { get; set; }

        public float Breaing { get; set; }

        public bool AdherencePoint { get; set; }

        public double Lat { get; set; }

        public double Long { get; set; }

        public int Id { get; set; }

        public DateTime ExpectedTime { get; set; }

        public DateTime ScheduledTime { get; set; }

        public string ExpectedTimeText { get; set; }

        public string ScheduledTimeText { get; set; }

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
                    text = eta + " m";
                }
                else if (eta < 1 && eta >= 0)
                {
                    if (string.IsNullOrWhiteSpace(ExpectedTimeText))
                    {
                        text = "N/A";
                    }
                    else
                    {
                        int seconds = (ExpectedTime - DateTime.Now).Seconds;

                        if (seconds > 30)
                        {
                            text = seconds + "s";
                        }
                        else
                        {
                            text = "N/A";
                        }
                    }
                }
                else
                {
                    text = "P";
                }

                return text;
            }
        }
    }
}
