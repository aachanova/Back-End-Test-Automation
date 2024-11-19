using Newtonsoft.Json;

namespace DemoAPITesting
{
    public class WeatherForcast
    {
        public DateTime Date { get; set; } = DateTime.Now;
        //[JsonProperty("temperature_c")]
        public int Temperature { get; set; } = 30;
        //[JsonIgnore]
        public string Summary { get; set; } = "Hot summer days";
    }
}
