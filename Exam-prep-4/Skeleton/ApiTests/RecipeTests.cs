using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class RecipeTests : IDisposable
    {
        private RestClient client;
        private string token;
        private Random random;
        private string title;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");
            random = new Random();

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");

        }

        [Test, Order(1)]
        public void Test_GetAllRecipes()
        {
            // Arrange
            var request = new RestRequest("/recipe", Method.Get);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response does not have the correct status.");
                Assert.That(response.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

                var recipes = JArray.Parse(response.Content);
                //Console.WriteLine(recipes);
                Assert.That(recipes.Type, Is.EqualTo(JTokenType.Array), "The response content is not an array.");
                Assert.That(recipes.Count(), Is.GreaterThan(0), "The recipes count is below than one.");

                foreach (var recipe in recipes)
                {
                    Assert.That(recipe["title"]?.ToString(), Is.Not.Null.Or.Empty, "The recipe title is null or empty.");
                    Assert.That(recipe["ingredients"], Is.Not.Null.Or.Empty, "The recipe ingredients are null or empty.");
                    Assert.That(recipe["instructions"], Is.Not.Null.Or.Empty, "The recipe instructions are null or empty.");
                    Assert.That(recipe["cookingTime"], Is.Not.Null.Or.Empty, "The recipe cookingTime is null or empty.");
                    Assert.That(recipe["servings"], Is.Not.Null.Or.Empty, "The recipe servings are null or empty.");
                    Assert.That(recipe["category"], Is.Not.Null.Or.Empty, "The recipe category is null or empty.");

                    Assert.That(recipe["ingredients"]?.Type, Is.EqualTo(JTokenType.Array), "The ingredients do not have the correct type.");
                    Assert.That(recipe["instructions"]?.Type, Is.EqualTo(JTokenType.Array), "The instructions do not have the correct type.");


                }
            });
        }

        [Test, Order(2)]
        public void Test_GetRecipeByTitle()
        {
            // Arrange
            // Get request for all recipes
            var request = new RestRequest("/recipe", Method.Get);

            var titleToGet = "Chocolate Chip Cookies";
            var expectedCookingTime = 25;
            var expectedServings = 24;
            var expectedIngredients = 9;
            var expectedInstructions = 7;

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(response.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

                var recipes = JArray.Parse(response.Content);
                var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == titleToGet);

                Assert.That(recipe, Is.Not.Null, $"The recipe with title {titleToGet} does not exist.");

                Assert.That(recipe["cookingTime"].Value<int>(), Is.EqualTo(expectedCookingTime), "The cooking time is not as expected.");

                Assert.That(recipe["servings"].Value<int>(), Is.EqualTo(expectedServings), "The servings are not as expected.");

                Assert.That(recipe["ingredients"].Count(), Is.EqualTo(expectedIngredients), "The ingredients are not as expected.");

                Assert.That(recipe["instructions"].Count(), Is.EqualTo(expectedInstructions), "The instructions are not as expected.");
            });
        }

        [Test, Order(3)]
        public void Test_AddRecipe()
        {
            // Arrange
            // Get all categories
            var getAllCategories = new RestRequest("/category", Method.Get);
            var getAllCategoriesResponse = client.Execute(getAllCategories);

            Assert.Multiple(() =>
            {
                Assert.That(getAllCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response does not have the correct status.");
                Assert.That(getAllCategoriesResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");
            });

            var categories = JArray.Parse(getAllCategoriesResponse.Content);

            // Extract the first category id
            var categoryId = categories.First()["_id"]?.ToString();

            // Create request for creating recipe
            var createRecipeRequest = new RestRequest("/recipe", Method.Post);
            createRecipeRequest.AddHeader("Authorization", $"Bearer {token}");
            title = $"recipeTitle_{random.Next(999, 9999)}";
            var description = "Test description";
            var cookingTime = 50;
            var servings = 4;
            var ingredients = new[] { new { name = "Test", quantity = "10g" } };
            var instructions = new[] { new { step = "test" } };

            createRecipeRequest.AddJsonBody(new
            {
                title,
                description,
                cookingTime,
                servings,
                ingredients,
                instructions,
                category = categoryId
            });

            // Act
            var createResponse = client.Execute(createRecipeRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(createResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");
            });

            var recipe = JObject.Parse(createResponse.Content);
            var recipeId = recipe["_id"]?.ToString();
            //title = recipe["title"]?.ToString();

            // Get request for getting by id
            var getByIdRequest = new RestRequest($"/recipe/{recipeId}", Method.Get);
            var getByIdResponse = client.Execute(getByIdRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(getByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(getByIdResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

                var createdRecipe = JObject.Parse(getByIdResponse.Content);

                Assert.That(createdRecipe["title"]?.ToString(), Is.EqualTo(title));
                Console.WriteLine(title);
                Assert.That(createdRecipe["cookingTime"].Value<int>, Is.EqualTo(cookingTime));
                Assert.That(createdRecipe["servings"].Value<int>, Is.EqualTo(servings));
                Assert.That(createdRecipe["category"]?["_id"]?.ToString(), Is.EqualTo(categoryId));

                // Asserts for ingrediants
                Assert.That(createdRecipe["ingredients"].Count(), Is.EqualTo(ingredients.Count()));

                Assert.That(createdRecipe["ingredients"]?[0]["name"]?.ToString(), Is.EqualTo(ingredients[0].name));
                Assert.That(createdRecipe["ingredients"]?[0]["quantity"]?.ToString(), Is.EqualTo(ingredients[0].quantity));

                // Asserts for instructions
                Assert.That(createdRecipe["instructions"].Count(), Is.EqualTo(instructions.Count()));

                Assert.That(createdRecipe["instructions"]?[0]["step"]?.ToString(), Is.EqualTo(instructions[0].step));
            });
        }

        [Test, Order(4)]
        public void Test_UpdateRecipe()
        {  

            // Arrange
            // Get by title
            var updateRequest = new RestRequest("/recipe", Method.Get);

            // Act
            var updateResponse = client.Execute(updateRequest);
            Console.WriteLine(updateResponse.Content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(updateResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

            });

            var recipes = JArray.Parse(updateResponse.Content);
            Console.WriteLine(title);
            
            var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == title);

            Assert.That(recipe, Is.Not.Null, $"The recipe with title {title} does not exist.");

            var recipeId = recipe["_id"]?.ToString();

            // Create update request
            var updateRecipeRequest = new RestRequest($"/recipe/{recipeId}", Method.Put);
            updateRecipeRequest.AddHeader("Authorization", $"Bearer {token}");
            title = title + "_updated";
            var updatedServings = 30;

            updateRecipeRequest.AddJsonBody(new
            {
                title,
                servings = updatedServings
            });

            // Act
            var updateRecipeResponse = client.Execute(updateRecipeRequest);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(updateRecipeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(updateRecipeResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");

                var updatedRecipe = JObject.Parse(updateRecipeResponse.Content);
                Assert.That(updatedRecipe["title"]?.ToString(), Is.EqualTo(title));
                Assert.That(updatedRecipe["servings"].Value<int>, Is.EqualTo(updatedServings));
            });
        }


        [Test, Order(5)]
        public void Test_DeleteRecipe()
        {
            // Arrange
            // Get by title
            var updateRequest = new RestRequest("/recipe", Method.Get);

            // Act
            var updateResponse = client.Execute(updateRequest);
            Console.WriteLine(updateResponse.Content);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");
                Assert.That(updateResponse.Content, Is.Not.Null.Or.Empty, "The response content is null or empty.");
            });

            var recipes = JArray.Parse(updateResponse.Content);
            Console.WriteLine(title);

            var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == title);

            Assert.That(recipe, Is.Not.Null, $"The recipe with title {title} does not exist.");

            var recipeId = recipe["_id"]?.ToString();

            // Create delete request
            var deleteRequest = new RestRequest($"/recipe/{recipeId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            // Act
            var deleteResponse = client.Execute(deleteRequest);

            // Assert
            Assert.Multiple(() => {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response  does not have the correct status.");

                // Get request by id
                var verifyRequest = new RestRequest($"/recipe/{recipeId}", Method.Get);
                var verifyResponse = client.Execute(verifyRequest);

                Assert.That(verifyResponse.Content, Is.EqualTo("null"), "The response content is not null.");

            });
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
