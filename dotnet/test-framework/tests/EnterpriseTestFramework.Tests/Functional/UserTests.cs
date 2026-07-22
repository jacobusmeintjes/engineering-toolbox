using EnterpriseFramework.Core.Clients;
using EnterpriseFramework.Core.Models;
using EnterpriseFramework.Tests.Base;


namespace EnterpriseFramework.Tests.Functional
{

    public class UserTests : IClassFixture<TestContextFixture>
    {
        private readonly UserClient _userClient;

        // Constructor handles dependency injection via class fixtures
        public UserTests(TestContextFixture context)
        {
            // Instantiate your Service Client wrapper with the pre-built specification profile
            _userClient = new UserClient(context.ApiSpecification);
        }

        [Fact]
        public void CreateUser_WithValidPayload_ReturnsStatusCode201AndValidData()
        {
            // Arrange - Build target data via strong records
            var requestPayload = new UserRequestDto("Alex Mercer", "Automation Architect");

            // Act - Fire off the isolated service request wrapper
            var response = _userClient.CreateUser(requestPayload);

            // Assert - Step 1: Inline fluent protocol assertions
            response.Then()
                .StatusCode(201);

            // Assert - Step 2: Extract back to structural records for deep domain business logical checks
            var responseBody = (UserResponseDto)response.Extract().Body(nameof(UserResponseDto));

            Assert.NotNull(responseBody.Id);
            Assert.Equal("Alex Mercer", responseBody.Name);
            Assert.Equal("Automation Architect", responseBody.Job);
        }

        [Theory]
        [InlineData("abc", "Janet")]
        [InlineData("def", "George")]
        public void GetUserById_WithValidId_ReturnsExpectedUserName(string userId, string expectedName)
        {
            // Act
            var response = _userClient.GetUser(userId);

            // Assert
            response.Then()
                .StatusCode(200);

            // Extract via direct JSONPath string selector if you don't want to map full objects
            string actualName = "";// response.Extract().Path("$.data.first_name").ToString()!;
            Assert.Equal(expectedName, actualName);
        }
    }
}


