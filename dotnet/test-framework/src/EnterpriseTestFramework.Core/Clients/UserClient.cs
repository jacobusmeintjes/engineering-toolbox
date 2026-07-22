using EnterpriseFramework.Core.Models;
using EnterpriseFramework.Core.Telemetry;
using RestAssured.Request.Builders;
using RestAssured.Response;
using static RestAssured.Dsl;

namespace EnterpriseFramework.Core.Clients
{
    public sealed class UserClient
    {
        private readonly RequestSpecification _specification;
        private readonly TelemetryChannelDecorator<UserRequestDto, VerifiableResponse> _createDecorator;
        private readonly TelemetryChannelDecorator<string, VerifiableResponse> _getDecorator;


        public UserClient(RequestSpecification specification)
        {
            _specification = specification;

            _createDecorator = new TelemetryChannelDecorator<UserRequestDto, VerifiableResponse>(
               channelType: "rest",
               inner: userRequest => Given()
                   .Spec(_specification)
                   .Body(userRequest)
                   .When()
                   .Post("/api/users"));

            _getDecorator = new TelemetryChannelDecorator<string, VerifiableResponse>(
                channelType: "rest",
                inner: userId => Given()
                    .Spec(_specification)
                    .When()
                    .Get($"/api/users/{userId}"));
        }

        public VerifiableResponse CreateUser(UserRequestDto userRequest) => _createDecorator.Execute(userRequest);

        public VerifiableResponse GetUser(string userId) => _getDecorator.Execute(userId);
    }    
}
