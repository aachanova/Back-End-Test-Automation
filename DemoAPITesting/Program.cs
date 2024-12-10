using DemoAPITesting2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using System.Reflection.Metadata.Ecma335;
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

            string jsonString = File.ReadAllText("D:\\Ani\\00-Back-End-Test-Automation-oth\\DemoData.json");

            WeatherForcast forcastFromJson = JsonSerializer.Deserialize<WeatherForcast>(jsonString);

            //  newtonsoft json package
            WeatherForcast forcastNS = new WeatherForcast();

            string weatherInfoNS = JsonConvert.SerializeObject(forcastNS, Formatting.Indented);
            Console.WriteLine(weatherInfoNS);

            jsonString = File.ReadAllText("D:\\Ani\\00-Back-End-Test-Automation-oth\\DemoData.json");

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

            // Executing simple HTTP Request with RestSharp
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest("/users/softuni/repos", Method.Get);
            var response = client.Execute(request);

            //Console.WriteLine(response.StatusCode);
            //Console.WriteLine(response.Content);

            //  Using URL Segments
            var requestUrlSegments = new RestRequest("/repos/{user}/{repo}/issues/{id}", Method.Get);
            requestUrlSegments.AddUrlSegment("user", "testnakov");
            requestUrlSegments.AddUrlSegment("repo", "test-nakov-repo");
            requestUrlSegments.AddUrlSegment("id", 1);

            var responseUrlSegment = client.Execute(requestUrlSegments);
            Console.WriteLine(responseUrlSegment.StatusCode);
            Console.WriteLine(responseUrlSegment.Content);

            //  Deserializing json response
            var requestDeserializing = new RestRequest("/users/softuni/repos", Method.Get);
            var responseDeserializing = client.Execute(requestDeserializing);
            var repos = JsonConvert.DeserializeObject<List<Repo>>(responseDeserializing.Content);

            //  Http post with authentication
            var clientWithAuthentication = new RestClient(new RestClientOptions("https://api.github.com")
            {
                Authenticator = new HttpBasicAuthenticator("username", "DemoAPITesting-Token")
            });

            var postRequest = new RestRequest("/repos/testnakov/test-nakov-repo/issues", Method.Post);
            postRequest.AddHeader("Content-Type", "application/json");
            postRequest.AddBody(new { title = "SomeTitle", body = "SomeBody" });

            var postResponse = clientWithAuthentication.Execute(postRequest);
            Console.WriteLine(postResponse.StatusCode);Console.ReadLine();
        }
    }
}

