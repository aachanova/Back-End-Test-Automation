using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DemoAPITesting
{
    class Program
    {
        static void Main(string[] args)
        {
            //  built-in System.Text.Json nuget package
            WeatherForcast forcast = new WeatherForcast();

            string weatherInfo = JsonSerializer.Serialize(forcast);
            Console.WriteLine(weatherInfo);

            string jsonString = File.ReadAllText("D:\\Ani\\Back-End-Test-Automation-oth\\DemoData.json");

            WeatherForcast forcastFromJson = JsonSerializer.Deserialize<WeatherForcast>(jsonString);

            //  newtonsoft json package
            WeatherForcast forcastNS = new WeatherForcast();

            string weatherInfoNS = JsonConvert.SerializeObject(forcastNS, Formatting.Indented);
            Console.WriteLine(weatherInfoNS);

            jsonString = File.ReadAllText("D:\\Ani\\Back-End-Test-Automation-oth\\DemoData.json");

            WeatherForcast weatherInfoFromNS = JsonConvert.DeserializeObject<WeatherForcast>(jsonString);

            //  working with anonymous objects
            var json = @"{ 'firstName': 'Svetlin',
                        'lastName': 'Nakov',
                        'jobTitle': 'Technical Trainer' }";

            var template = new
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                JobTitle = string.Empty
            };

            var person = JsonConvert.DeserializeAnonymousType(json, template);

            //  applying naming conventionto the class properties
            WeatherForcast weatherForcastResolver = new WeatherForcast();

            DefaultContractResolver contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            string snakeCaseJson = JsonConvert.SerializeObject(weatherForcastResolver, new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
            });

            Console.WriteLine(snakeCaseJson);

            //  Jobject
            var jsonAsString = JObject.Parse(@"{'products': [
                {'name': 'Fruits', 'products': ['apple', 'banana']},
                {'name': 'Vegetables', 'products': ['cucumber']}]}");

            var products = jsonAsString["products"].Select(t =>
            string.Format("{0} ({1})",
            t["name"],
            string.Join(", ", t["products"])
            ));
            //
        }
    }
}
