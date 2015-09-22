using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherGround {
    public enum WeatherConditions {
        Unkown,
        Rain,
        Storm,
        Clear,
        PartlyCloudy,
        Snow,
        Overcast
    }

    public class WeatherResponse {
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string Wind { get; set; }
        public WeatherConditions Conditions { get; set; }
        public string ConditionsRaw { get; set; }
    }
}
