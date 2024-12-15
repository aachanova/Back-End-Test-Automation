using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;
        private Random random;
        private string name;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");
            random = new Random();

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_CategoryLifecycle_RecipeBook()
        {
            // Step 1: Create a new category
            name = $"categoryName_{random.Next(999, 9999)}";
            var createRequest = new RestRequest("/category", Method.Post);
            createRequest.AddHeader("Authorization", $"Bearer {token}");
            createRequest.AddJsonBody(new { name });

            var createResponse = client.Execute(createRequest);

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");

            var createdCategory = JObject.Parse(createResponse.Content);

            Assert.That(createdCategory["_id"]?.ToString(), Is.Not.Null.Or.Empty, "The response content is null or empty.");



            // Step 2: Get all categories and verify new category is included
            var getAllCategoriesRequest = new RestRequest("/category", Method.Get);
            var getAllResponse = client.Execute(getAllCategoriesRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(getAllResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

                var categories = JArray.Parse(getAllResponse.Content);
                Assert.That(categories?.Type, Is.EqualTo(JTokenType.Array));
                Assert.That(categories.Count(), Is.GreaterThan(0));

            });
            // Step 3: Get category by ID
            var categoryId = createdCategory["_id"]?.ToString();

            var getByIdRequest = new RestRequest($"/category/{categoryId}", Method.Get);
            var getByIdResponse = client.Execute(getByIdRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(getByIdResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

                var categoryById = JObject.Parse(getByIdResponse.Content);
                Assert.That(categoryById["_id"]?.ToString(), Is.EqualTo(categoryId));
                Assert.That(categoryById["name"]?.ToString(), Is.EqualTo(name));
            });

            // Step 4: Edit the category and verify update
            var editRequest = new RestRequest($"/category/{categoryId}", Method.Put);
            name = name + "_updated";
            editRequest.AddHeader("Authorization", $"Bearer {token}");
            editRequest.AddJsonBody(new { name });

            var editResponse = client.Execute(editRequest);

            Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");

            // Step 5: Verify update
            var getUpdatedCategory = new RestRequest($"/category/{categoryId}", Method.Get);
            var verifyResponse = client.Execute(getUpdatedCategory);

            Assert.That(verifyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
            Assert.That(verifyResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

            var verifyCategory = JObject.Parse(verifyResponse.Content);
            Assert.That(verifyCategory["name"]?.ToString() , Is.EqualTo(name));

            // Step 6: Delete the category
            var deleteRequest = new RestRequest($"category/{categoryId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");

            // Step 7: Verify category is deleted
            var verifyDeleteRequest = new RestRequest($"/category/{categoryId}", Method.Get);
            var verifyDeleteResponse = client.Execute(verifyDeleteRequest);

            Assert.That(verifyDeleteResponse.Content, Is.EqualTo("null"));
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
