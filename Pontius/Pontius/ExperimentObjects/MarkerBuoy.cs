using System;

namespace Pontius.ExperimentObjects
{
    public class MarkerBuoy
    {
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public MarkerBuoyImageType MarkerBuoyImageType { get; set; }
        public MarkerBuoyType MarkerBuoyType { get; set; }
    }
}

