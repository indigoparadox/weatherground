using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WeatherGround {
    public class WeatherGroundUnder {

        public static readonly string URL = "http://mobile.wunderground.com/cgi-bin/findweather/getForecast?brand=mobile&query=xxxxx";

        public string ZipCode { get; set; }

        public WeatherGroundUnder( string zipCodeIn ) {
            this.ZipCode = zipCodeIn;
        }

        private static string GetMatch( string body, string pattern ) {
            bool wholeGroup = true;
            Match windMatch = Regex.Match( body, pattern );
            foreach( Group g in windMatch.Groups ) {
                if( !wholeGroup ) {
                    return g.Value;
                } else {
                    wholeGroup = false;
                }
            }
            return "";
        }

        public WeatherResponse GetWeather() {
            WeatherResponse weatherResponse = new WeatherResponse();
            WebRequest weatherRequest = WebRequest.Create( Regex.Replace( URL, "xxxxx", this.ZipCode ) );
            using( Stream weatherStream = weatherRequest.GetResponse().GetResponseStream() ) {
                using( StreamReader weatherReader = new StreamReader( weatherStream ) ) {
                    string line = "";
                    StringBuilder body = new StringBuilder();
                    while( null != line ) {
                        line = weatherReader.ReadLine();
                        body.AppendLine( line );
                    }

                    weatherResponse.Temperature = GetMatch( body.ToString(), "<td>Temperature<\\/td>\r\n  <td>\r\n  <span class=\"nowrap\"><b>([0-9.]+)<\\/b>" );
                    weatherResponse.Humidity = GetMatch( body.ToString(), "<td>Humidity<\\/td>\r\n<td><b>([0-9%.]+)<\\/b>" );
                    weatherResponse.Wind = String.Format(
                        "{0} at {1}MPH",
                        GetMatch( body.ToString(), "<td>Wind<\\/td>\r\n<td>\r\n<b>([NESW]+|West|North|South|East)<\\/b>" ),
                        GetMatch( body.ToString(), " at\r\n  <span class=\"nowrap\"><b>([0-9.]+)<\\/b>" )
                    );

                    weatherResponse.ConditionsRaw = GetMatch( body.ToString(), "<td>Conditions<\\/td>\r\n<td><b>([A-Za-z\\-. ]+)<\\/b>" );
                    weatherResponse.Conditions = WeatherConditions.Unkown;
                    if( weatherResponse.ConditionsRaw.Contains( "Cloudy" ) ) {
                        weatherResponse.Conditions = WeatherConditions.PartlyCloudy;
                    } else if( weatherResponse.ConditionsRaw.Contains( "Overcast" ) ) {
                        weatherResponse.Conditions = WeatherConditions.Overcast;
                    }else if( weatherResponse.ConditionsRaw.Contains( "Thunder" ) || weatherResponse.ConditionsRaw.Contains( "Storm" ) ) {
                        weatherResponse.Conditions = WeatherConditions.Storm;
                    } else if( weatherResponse.ConditionsRaw.Contains( "Snow" ) || weatherResponse.ConditionsRaw.Contains( "Flurr" ) ) {
                        weatherResponse.Conditions = WeatherConditions.Snow;
                    } else if( weatherResponse.ConditionsRaw.Contains( "Rain" ) || weatherResponse.ConditionsRaw.Contains( "Shower" ) ) {
                        weatherResponse.Conditions = WeatherConditions.Rain;
                    } else if( weatherResponse.ConditionsRaw.Contains( "Sun" ) || weatherResponse.ConditionsRaw.Contains( "Clear" ) ) {
                        weatherResponse.Conditions = WeatherConditions.Clear;
                    }
                }
            }
            return weatherResponse;
        }
    }
}
