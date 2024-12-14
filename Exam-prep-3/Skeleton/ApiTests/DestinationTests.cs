using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using NUnit.Framework;
using System.Xml.Linq;

namespace ApiTests
{
    [TestFixture]
    public class DestinationTests : IDisposable
    {
        private RestClient client;
        private string token;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_GetAllDestinations()
        {
            // Arrange
            var getRequest = new RestRequest("destination", Method.Get);

            // Act
            var getResponse = client.Execute(getRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK");

                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var destinations = JArray.Parse(getResponse.Content);

                Assert.That(destinations.Type, Is.EqualTo(JTokenType.Array), "The response content is not a JSON Array");

                Assert.That(destinations.Count, Is.GreaterThan(0), "Expected destinations are less than zero");

                foreach (var dest in destinations)
                {
                    Assert.That(dest["name"]?.ToString(), Is.Not.Null.Or.Empty, "Property name is not as expected");

                    Assert.That(dest["location"]?.ToString(), Is.Not.Null.Or.Empty, "Property location is not as expected");

                    Assert.That(dest["description"]?.ToString(), Is.Not.Null.Or.Empty, "Property description is not as expected");

                    Assert.That(dest["category"]?.ToString(), Is.Not.Null.Or.Empty, "Property category is not as expected");

                    Assert.That(dest["attractions"]?.Type, Is.EqualTo(JTokenType.Array), "Atraction property is not array");

                    Assert.That(dest["bestTimeToVisit"], Is.Not.Null.Or.Empty, "Property bestTimeToVisit is not as expected");
                }
            });
        }

        [Test]
        public void Test_GetDestinationByName()
        {
            // Arrange
            var getRequest = new RestRequest("destination", Method.Get);

            // Act
            var getResponse = client.Execute(getRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK");

                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var destinations = JArray.Parse(getResponse.Content);
                var destination = destinations.FirstOrDefault(d => d["name"]?.ToString() == "New York City");

                Assert.That(destination["location"]?.ToString(), Is.EqualTo("New York, USA"), "Location property does not have the correct value");

                Assert.That(destination["description"]?.ToString(), Is.EqualTo("The largest city in the USA, known for its skyscrapers, culture, and entertainment."), "Description property does not have the correct value", "Destination property does not have the correct value.");
            });
        }

        [Test]
        public void Test_AddDestination()
        {
            // Arrange
            // Get all categories and extract first category id
            var getCategoriesRequest = new RestRequest("category", Method.Get);

            var getCategoriesResponse = client.Execute(getCategoriesRequest);

            var categories = JArray.Parse(getCategoriesResponse.Content);
            var firstCategory = categories.First();
            var categoryId = firstCategory["_id"]?.ToString();

            // Create new destination
            var addRequest = new RestRequest("destination", Method.Post);
            addRequest.AddHeader("Authorization", $"Bearer {token}");
            var name = "Random Name";
            var location = "New Location";
            var description = "New Description";
            var bestTimeToVisit = "April";
            var attractions = new[] { "Attraction1", "Attraction2", "Attraction3" };
            addRequest.AddJsonBody(new
            {
                name,
                location,
                description,
                bestTimeToVisit,
                attractions,
                category = categoryId
            });

            // Act
            Console.WriteLine("Test log message: Starting request...");
            var addResponse = client.Execute(addRequest);
            Console.WriteLine("Response Status Code: " + addResponse.StatusCode);
            Console.WriteLine("Response Content: " + addResponse.Content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected response status code is not OK");

                Assert.That(addResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");
            });

            var createdDestination = JObject.Parse(addResponse.Content);
            Assert.That(createdDestination["_id"]?.ToString(), Is.Not.Empty);

            var createdDestinationId = createdDestination["_id"]?.ToString();

            // Get destination by id
            var getDestinationRequest = new RestRequest($"/destination/{createdDestinationId}", Method.Get);

            var getResponse = client.Execute(getDestinationRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected response status code is not OK");

                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var destination = JObject.Parse(getResponse.Content);

                Assert.That(destination["name"]?.ToString(), Is.EqualTo(name));

                Assert.That(destination["location"]?.ToString(), Is.EqualTo(location));

                Assert.That(destination["description"]?.ToString(), Is.EqualTo(description));

                Assert.That(destination["bestTimeToVisit"]?.ToString(), Is.EqualTo(bestTimeToVisit));

                Assert.That(destination["category"]?.ToString(), Is.Not.Null.Or.Empty);

                Assert.That(destination["category"]["_id"]?.ToString(), Is.EqualTo(categoryId));

                Assert.That(destination["attractions"].Count, Is.EqualTo(3));

                Assert.That(destination["attractions"]?.Type, Is.EqualTo(JTokenType.Array));

                Assert.That(destination["attractions"][0]?.ToString(), Is.EqualTo("Attraction1"));
                Assert.That(destination["attractions"][1]?.ToString(), Is.EqualTo("Attraction2"));
                Assert.That(destination["attractions"][2]?.ToString(), Is.EqualTo("Attraction3"));
            });
        }

        [Test]
        public void Test_UpdateDestination()
        {
            var getCategoriesRequest = new RestRequest("category", Method.Get);

            var getCategoriesResponse = client.Execute(getCategoriesRequest);

            var categories = JArray.Parse(getCategoriesResponse.Content);
            var firstCategory = categories.First();
            var categoryId = firstCategory["_id"]?.ToString();

            var addRequest = new RestRequest("destination", Method.Post);
            addRequest.AddHeader("Authorization", $"Bearer {token}");
            var name = "Random Name1";
            var location = "New Location";
            var description = "New Description";
            var bestTimeToVisit = "April";
            var attractions = new[] { "Attraction1", "Attraction2", "Attraction3" };
            addRequest.AddJsonBody(new
            {
                name,
                location,
                description,
                bestTimeToVisit,
                attractions,
                category = categoryId
            });

            var addResponse = client.Execute(addRequest);
            var createdDestination = JObject.Parse(addResponse.Content);
            Console.WriteLine(createdDestination.ToString());
            // Arange
            // Get all destinations and extract with name Machu Picchu
            var getRequest = new RestRequest("destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected response status code is not OK");

            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

            var destinations = JArray.Parse(getResponse.Content);
            var destinationToUpdate = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Random Name1");
            Console.WriteLine("Hi hi");
            Console.WriteLine(destinationToUpdate.ToString());

            Assert.That(destinationToUpdate, Is.Not.Null);
            var destinationId = destinationToUpdate["_id"]?.ToString();

            // Create update request
            var updateRequest = new RestRequest($"destination/{destinationId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                name = "UpdatedName",
                bestTimeToVisit = "Winter"
            });

            // Act
            var updateResponse = client.Execute(updateRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected response status code is not OK");

                Assert.That(updateResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var updatedDestination = JObject.Parse(updateResponse.Content);
                Console.WriteLine("After update");
                Console.WriteLine(updatedDestination.ToString());

                //Assert.That(updatedDestination["name"]?.ToString(), Is.EqualTo("UpdatedName1"));
                //Assert.That(updatedDestination["bestTimeToVisit"]?.ToString(), Is.EqualTo("Winter"));

                //Assert.That(updatedDestination["name"]?.ToString(), Is.EqualTo("UpdatedName1"));
                //Assert.That(updatedDestination["bestTimeToVisit"]?.ToString(), Is.EqualTo("Winter"));
            });

        }

        [Test]
        public void Test_DeleteDestination()
        {
            // Arange
            // Get all destinations and extract with name Machu Picchu
            var getRequest = new RestRequest("destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected response status code is not OK");

            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

            var destinations = JArray.Parse(getResponse.Content);
            var destinationToDelete = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Yellowstone National Park");

            Assert.That(destinationToDelete, Is.Not.Null);

            var destinationId = destinationToDelete["_id"]?.ToString();

            // Create delete request
            var deleteRequest = new RestRequest($"destination/{destinationId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            //Act
            var deleteResponse = client.Execute(deleteRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                // Get request to get the destination that we deleted
                var verifyRequest = new RestRequest($"destination/{destinationId}");

                var verifyResponse = client.Execute(verifyRequest);

                Assert.That(verifyResponse.Content, Is.EqualTo("null"));

            });

        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
