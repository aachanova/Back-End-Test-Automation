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

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_CategoryLifecycle()
        {
            // Step 1: Create a new category
            var createCategoryRequest = new RestRequest("/category", Method.Post);
            createCategoryRequest.AddHeader("Authorization", $"Bearer {token}");

            createCategoryRequest.AddJsonBody(new
            {
                name = "Test Category"
            });

            var createResponse = client.Execute(createCategoryRequest);

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var createdCategory = JObject.Parse(createResponse.Content);
            var categoryId = createdCategory["_id"]?.ToString();

            Assert.That(categoryId, Is.Not.Null.Or.Empty);

            // Step 2: Get all categories
            var getAllCategories = new RestRequest("category", Method.Get);
            var getAllCategoriesResponse = client.Execute(getAllCategories);

            Assert.Multiple(() =>
            {
                Assert.That(getAllCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(getAllCategoriesResponse.Content, Is.Not.Empty);

                var categories = JArray.Parse(getAllCategoriesResponse.Content);
                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array));
                Assert.That(categories.Count, Is.GreaterThan(0));

                var createdCategory = categories.FirstOrDefault(c =>
                    c["name"]?.ToString() == "Test Category");
                Assert.That(createdCategory, Is.Not.Null.Or.Empty); //empty ???

            });



            // Step 3: Get category by ID
            var getcategoryById = new RestRequest($"category/{categoryId}", Method.Get);
            var getcategoryByIdResponse = client.Execute(getcategoryById);

            Assert.Multiple(() => {
                Assert.That(getcategoryByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(getcategoryByIdResponse.Content, Is.Not.Empty);

                var category = JObject.Parse(getcategoryByIdResponse.Content);
                Assert.That(category["_id"]?.ToString(), Is.EqualTo(categoryId));
                Assert.That(category["name"]?.ToString(), Is.EqualTo("Test Category"));
            });

            // Step 4: Edit the category
            var editRequest = new RestRequest($"category/{categoryId}", Method.Put);
            editRequest.AddHeader("Authorization", $"Bearer {token}");
            editRequest.AddJsonBody(new
            {
                name = "Updated Test Category"
            });

            var editResponse = client.Execute(editRequest);

            Assert.Multiple(() => { 
                Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            });

            // Step 5: Verify Edit
            var editedCategoryRequest = new RestRequest($"category/{categoryId}", Method.Get);
            
            var editedCategoryResponse = client.Execute(editedCategoryRequest);

            Assert.Multiple(() => {
                Assert.That(editedCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(editedCategoryResponse.Content, Is.Not.Empty);

                var updatedCategory = JObject.Parse(editedCategoryResponse.Content);
                Assert.That(updatedCategory["name"]?.ToString(), Is.EqualTo("Updated Test Category"));
            });


            // Step 6: Delete the category
            var deleteCategory = new RestRequest($"category/{categoryId}", Method.Delete);
            deleteCategory.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteCategory);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Step 7: Verify that the deleted category cannot be found
            var verifyDeleteRequest = new RestRequest($"category/{categoryId}", Method.Get);

            var verifyResponse = client.Execute(verifyDeleteRequest);
            Assert.That(verifyResponse.Content, Is.EqualTo("null"));
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}

