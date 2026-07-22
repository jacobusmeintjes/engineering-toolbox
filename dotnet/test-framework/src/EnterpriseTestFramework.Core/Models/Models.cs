using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EnterpriseFramework.Core.Models
{
    // Request Payload Model
    public sealed record UserRequestDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("job")] string Job
    );

    // Response Payload Model
    public sealed record UserResponseDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("job")] string Job,
        [property: JsonPropertyName("createdAt")] string CreatedAt
    );
}
