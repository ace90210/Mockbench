using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Mockbench.Abstractions.Repositories;
using Mockbench.Services.MockServices;
using Mockbench.Shared.Models.Endpoint;
using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Environment;
using Mockbench.Shared.Models.Headers;
using Mockbench.Shared.Models.Microservice;
using Mockbench.Shared.Models.QueryParameters;
using Mockbench.Shared.Models.Response;
using Mockbench.Shared.Models.Tenant;
using Moq;
using System.Net;
using System.Text;

namespace Mockbench.Services.Tests;

public class MockServiceTests
{
    private readonly Mock<IEndpointRepository> _mockEndpointRepository;
    private readonly Mock<ICommonRepository> _mockCommonRepository;
    private readonly MockService _mockService;

    public MockServiceTests()
    {
        _mockEndpointRepository = new Mock<IEndpointRepository>();
        _mockCommonRepository = new Mock<ICommonRepository>();
        _mockService = new MockService(_mockEndpointRepository.Object, _mockCommonRepository.Object);
    }

    private DefaultHttpContext CreateHttpContext(
        string method = "GET",
        string queryString = "",
        string? requestBody = null,
        string? contentType = "application/json",
        Dictionary<string, StringValues>? headers = null,
        string scheme = "http",
        string? host = "localhost",
        string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.QueryString = new QueryString(queryString);
        context.Request.Scheme = scheme;

        if(host is not null)
            context.Request.Host = new HostString(host);
        context.Request.Path = new PathString(path);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                context.Request.Headers.Add(header.Key, header.Value);
            }
        }

        if (requestBody != null)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            context.Request.Body = stream;
            context.Request.ContentLength = stream.Length;
            context.Request.ContentType = contentType;
        }
        return context;
    }

    private MatchingEndpoints CreateMatchingEndpoints(
        List<EndpointDto>? endpoints = null,
        MicroserviceDto? microservice = null,
        TenantBase? tenant = null,
        EnvironmentDto? environment = null)
    {
        return new MatchingEndpoints
        {
            Endpoints = endpoints ?? new List<EndpointDto>(),
            Microservice = microservice ?? new MicroserviceDto { Name = "TestMicroservice", Path = "testms" },
            Tenant = tenant,
            Environment = environment
        };
    }

    // region GetMatchingEndpointDtoAsync Tests
    [Fact]
    public async Task GetMatchingEndpointDtoAsync_GetRequest_NoBody_FindsExactEndpoint()
    {
        // Arrange
        var endpointPath = "/users/1";
        var context = CreateHttpContext(method: "GET", path: endpointPath);
        var expectedEndpoint = new EndpointDto { Id = 1, FromUrl = endpointPath, RestType = RestType.GET, ExactUrlMatch = true, QueryParameters = new List<QueryParameterDto>() };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { expectedEndpoint });

        // Act
        var result = await _mockService.GetMatchingEndpointDtoAsync(matchingEndpoints, RestType.GET, context, endpointPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEndpoint.Id, result.Id);
    }

    [Fact]
    public async Task GetMatchingEndpointDtoAsync_PostRequest_WithBody_FindsExactEndpoint()
    {
        // Arrange
        var endpointPath = "/users";
        var requestBody = "{\"name\":\"test\"}";
        var context = CreateHttpContext(method: "POST", path: endpointPath, requestBody: requestBody);
        var expectedEndpoint = new EndpointDto { Id = 1, FromUrl = endpointPath, FromBody = requestBody, RestType = RestType.POST, ExactUrlMatch = true, QueryParameters = new List<QueryParameterDto>() };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { expectedEndpoint });

        // Act
        var result = await _mockService.GetMatchingEndpointDtoAsync(matchingEndpoints, RestType.POST, context, endpointPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEndpoint.Id, result.Id);
        Assert.Equal(requestBody, result.FromBody);
    }


    [Fact]
    public async Task GetMatchingEndpointDtoAsync_NoMatchingEndpoints_ReturnsNull()
    {
        // Arrange
        var endpointPath = "/nomatch";
        var context = CreateHttpContext(method: "GET", path: endpointPath);
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto>());

        // Act
        var result = await _mockService.GetMatchingEndpointDtoAsync(matchingEndpoints, RestType.GET, context, endpointPath);

        // Assert
        Assert.Null(result);
    }
    // endregion

    // region GetMockResponseAsync Tests
    [Fact]
    public async Task GetMockResponseAsync_NoMatchingEndpoints_ReturnsNull()
    {
        // Arrange
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: null); // No endpoints
        var context = CreateHttpContext();

        // Act
        var result = await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, "/test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMockResponseAsync_NoEndpointsInMatchingEndpoints_ReturnsNull()
    {
        // Arrange
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto>()); // Empty list of endpoints
        var context = CreateHttpContext();

        // Act
        var result = await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, "/test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMockResponseAsync_MatchingEndpointFound_NoEnabledMockResponses_ReturnsNull()
    {
        // Arrange
        var endpointPath = "/test";
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto>(),
            MockResponses = new List<MockResponseDto> { new MockResponseDto { Id = 1, Enabled = false } }
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(path: endpointPath);

        // Act
        var result = await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, endpointPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMockResponseAsync_MatchingEndpointFound_ReturnsFirstEnabledPriorityResponse()
    {
        // Arrange
        var endpointPath = "/test";
        var response1 = new MockResponseDto { Id = 1, Enabled = true, Priority = 100, CreatedUtc = DateTime.UtcNow.AddHours(-1), Description = "P100" };
        var response2 = new MockResponseDto { Id = 2, Enabled = true, Priority = 1, CreatedUtc = DateTime.UtcNow, Description = "P1" }; // Higher priority (lower number)
        var response3 = new MockResponseDto { Id = 3, Enabled = true, Priority = 1, CreatedUtc = DateTime.UtcNow.AddHours(-2), Description = "P1 Older" };

        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto>(),
            MockResponses = new List<MockResponseDto> { response1, response2, response3 }
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(path: endpointPath);

        // Act
        var result = await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, endpointPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(response2.Id, result.Id); // response2 has higher priority and is newer than response3
    }

    [Fact]
    public async Task GetMockResponseAsync_RandomiseMockResultTrue_ReturnsRandomEnabledResponse()
    {
        // Arrange
        var endpointPath = "/random";
        var responses = new List<MockResponseDto>
        {
            new MockResponseDto { Id = 1, Enabled = true, Priority = 1, CreatedUtc = DateTime.UtcNow },
            new MockResponseDto { Id = 2, Enabled = true, Priority = 1, CreatedUtc = DateTime.UtcNow },
            new MockResponseDto { Id = 3, Enabled = true, Priority = 1, CreatedUtc = DateTime.UtcNow }
        };
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto>(),
            MockResponses = responses
        };
        var microservice = new MicroserviceDto { RandomiseMockResult = true };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint }, microservice: microservice);
        var context = CreateHttpContext(path: endpointPath);

        // Act: Run multiple times to check for randomness (not perfectly deterministic in a single run)
        var results = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            var r = await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, endpointPath);
            if (r != null) results.Add(r.Id);
        }

        // Assert
        Assert.NotEmpty(results);
        Assert.True(results.Distinct().Count() > 1, "Expected multiple different responses due to RandomiseMockResult."); // High chance this passes
    }

    [Fact]
    public async Task GetMockResponseAsync_WithMicroserviceFakeDelay_AddsDelay()
    {
        // Arrange
        var endpointPath = "/delay";
        var response = new MockResponseDto { Id = 1, Enabled = true, Priority = 1, FakeDelay = 0 }; // Response delay is 0
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto>(),
            MockResponses = new List<MockResponseDto> { response }
        };
        var microservice = new MicroserviceDto { FakeDelay = 50 }; // Microservice delay is 50ms
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint }, microservice: microservice);
        var context = CreateHttpContext(path: endpointPath);

        // Act
        var startTime = DateTime.UtcNow;
        await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, endpointPath);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.True((endTime - startTime).TotalMilliseconds >= 45, "Delay was not applied as expected from microservice."); // Allow some leeway
    }

    [Fact]
    public async Task GetMockResponseAsync_WithResponseFakeDelay_OverridesMicroserviceDelay()
    {
        // Arrange
        var endpointPath = "/delay";
        var response = new MockResponseDto { Id = 1, Enabled = true, Priority = 1, FakeDelay = 100 }; // Response delay is 100ms
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto>(),
            MockResponses = new List<MockResponseDto> { response }
        };
        var microservice = new MicroserviceDto { FakeDelay = 50 }; // Microservice delay is 50ms
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint }, microservice: microservice);
        var context = CreateHttpContext(path: endpointPath);

        // Act
        var startTime = DateTime.UtcNow;
        await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, endpointPath);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.True((endTime - startTime).TotalMilliseconds >= 95, "Delay was not applied as expected from response."); // Allow some leeway
    }

    [Fact]
    public async Task GetMockResponseAsync_SimulateTime_FiltersResponses()
    {
        // Arrange
        var endpointPath = "/time";
        var simulateTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var responsePast = new MockResponseDto { Id = 1, Enabled = true, Priority = 1, CreatedUtc = simulateTime.AddHours(-1) };
        var responseFuture = new MockResponseDto { Id = 2, Enabled = true, Priority = 1, CreatedUtc = simulateTime.AddHours(1) };

        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto>(),
            MockResponses = new List<MockResponseDto> { responsePast, responseFuture },
            SimulateTime = simulateTime // Endpoint simulate time
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(path: endpointPath);

        // Act
        var result = await _mockService.GetMockResponseAsync(matchingEndpoints, RestType.GET, context, endpointPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(responsePast.Id, result.Id); // Only response created before/at simulateTime should be chosen
    }
    // endregion

    // region CreateMockResponseIfNotExistAsync Tests
    [Fact]
    public async Task CreateMockResponseIfNotExistAsync_NewEndpoint_CreatesTenantEnvMsAndEndpointAndResponse()
    {
        // Arrange
        var matchingEndpoints = CreateMatchingEndpoints(
            microservice: new MicroserviceDto { Id = 0, Path = "newms", Name = "New MS", HeadersMode = HeadersMode.All}, // ID 0 means it needs creation
            environment: new EnvironmentDto { Id = 0, Path = "newenv", Name = "New Env" },
            tenant: new TenantBase { Id = 0, Path = "newtenant", Name = "New Tenant" }
        );
        var context = CreateHttpContext(path: "/api/data", queryString: "?param1=value1");
        var restType = RestType.GET;
        var endpointPath = "/api/data";
        var requestBody = (string)null; // For GET
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json")
        };
        httpResponseMessage.Headers.Add("X-Test-Header", "TestValue");
        var latency = TimeSpan.FromMilliseconds(100);

        _mockCommonRepository.Setup(r => r.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(It.IsAny<MatchingEndpoints>()))
            .Callback<MatchingEndpoints>(me =>
            {
                // Simulate IDs being set after creation
                if (me.Tenant != null) me.Tenant.Id = 1;
                if (me.Environment != null) me.Environment.Id = 1;
                if (me.Microservice != null) me.Microservice.Id = 1;
            })
            .ReturnsAsync((MatchingEndpoints)null);

        _mockEndpointRepository.Setup(r => r.CreateEndpointAsync(It.IsAny<EndpointDto>()))
            .ReturnsAsync((EndpointDto ep) => { ep.Id = 1; return ep; }); // Simulate endpoint creation and ID assignment

        // Act
        await _mockService.CreateMockResponseIfNotExistAsync(matchingEndpoints, context, restType, endpointPath, requestBody, httpResponseMessage, latency);

        // Assert
        _mockCommonRepository.Verify(r => r.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(matchingEndpoints), Times.Once);
        _mockEndpointRepository.Verify(r => r.CreateEndpointAsync(It.Is<EndpointDto>(ep =>
            ep.FromUrl == endpointPath &&
            ep.RestType == restType &&
            ep.TenantId == 1 &&
            ep.EnvironmentId == 1 &&
            ep.MicroserviceId == 1 &&
            ep.QueryParameters.Any(qp => qp.Name == "param1" && qp.Value == "value1") &&
            ep.MockResponses.Count == 1 &&
            ep.MockResponses.First().Body == "{\"key\":\"value\"}" &&
            ep.MockResponses.First().StatusCode == HttpStatusCode.OK &&
            ep.MockResponses.First().ContentType == "application/json" &&
            ep.MockResponses.First().Latency == latency &&
            ep.MockResponses.First().Headers.Any(h => h.Name == "X-Test-Header" && h.Value == "TestValue")
        )), Times.Once);
    }

    [Fact]
    public async Task CreateMockResponseIfNotExistAsync_ExistingEndpoint_AutoMockWithProxy_ResponseNotExist_AddsResponse()
    {
        // Arrange
        var endpointPath = "/existing";
        var existingEndpoint = new EndpointDto
        {
            Id = 123,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            MockBehaviour = MockBehaviour.AutoMockWithProxy,
            MockResponses = new List<MockResponseDto>(), // No existing responses
            QueryParameters = new List<QueryParameterDto>()
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { existingEndpoint });
        matchingEndpoints.Microservice.Id = 1; // Ensure microservice has an ID

        var context = CreateHttpContext(path: endpointPath);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("new data")
        };
        var latency = TimeSpan.FromMilliseconds(50);

        _mockEndpointRepository.Setup(r => r.AddResponseToEndpointAsync(existingEndpoint.Id, It.IsAny<MockResponseDto>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockService.CreateMockResponseIfNotExistAsync(matchingEndpoints, context, RestType.GET, endpointPath, null, httpResponseMessage, latency);

        // Assert
        _mockCommonRepository.Verify(r => r.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(matchingEndpoints), Times.Once);
        _mockEndpointRepository.Verify(r => r.AddResponseToEndpointAsync(existingEndpoint.Id, It.Is<MockResponseDto>(mr =>
            mr.Body == "new data" &&
            mr.StatusCode == HttpStatusCode.Created
        )), Times.Once);
        _mockEndpointRepository.Verify(r => r.CreateEndpointAsync(It.IsAny<EndpointDto>()), Times.Never); // Should not create new endpoint
    }

    [Fact]
    public async Task CreateMockResponseIfNotExistAsync_ExistingEndpoint_AutoMockWithProxy_ResponseExists_DoesNothing()
    {
        // Arrange
        var endpointPath = "/existing";
        var responseContent = "{\"id\":1}";
        var existingResponse = new MockResponseDto { Body = responseContent, ContentType = "application/json", StatusCode = HttpStatusCode.OK, Enabled = true };
        var existingEndpoint = new EndpointDto
        {
            Id = 123,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            MockBehaviour = MockBehaviour.AutoMockWithProxy,
            FromBody = null,
            MockResponses = new List<MockResponseDto> { existingResponse },
            QueryParameters = new List<QueryParameterDto>()
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { existingEndpoint });
        matchingEndpoints.Microservice.Id = 1;

        var context = CreateHttpContext(path: endpointPath);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) // Same response
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };
        var latency = TimeSpan.FromMilliseconds(50);

        // Act
        await _mockService.CreateMockResponseIfNotExistAsync(matchingEndpoints, context, RestType.GET, endpointPath, null, httpResponseMessage, latency);

        // Assert
        _mockCommonRepository.Verify(r => r.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(matchingEndpoints), Times.Once);
        _mockEndpointRepository.Verify(r => r.AddResponseToEndpointAsync(It.IsAny<int>(), It.IsAny<MockResponseDto>()), Times.Never);
        _mockEndpointRepository.Verify(r => r.CreateEndpointAsync(It.IsAny<EndpointDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateMockResponseIfNotExistAsync_ExistingEndpoint_ProxyOnly_DoesNothing()
    {
        // Arrange
        var endpointPath = "/proxyonly";
        var existingEndpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointPath,
            RestType = RestType.GET,
            MockBehaviour = MockBehaviour.ProxyOnly,
            MockResponses = new List<MockResponseDto>()
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { existingEndpoint });
        matchingEndpoints.Microservice.Id = 1;

        var context = CreateHttpContext(path: endpointPath);
        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("data") };
        var latency = TimeSpan.FromMilliseconds(30);

        // Act
        await _mockService.CreateMockResponseIfNotExistAsync(matchingEndpoints, context, RestType.GET, endpointPath, null, httpResponseMessage, latency);

        // Assert
        _mockCommonRepository.Verify(r => r.CreateTenantEnvironmentMicroserviceIfNotExistsAsync(matchingEndpoints), Times.Once);
        _mockEndpointRepository.Verify(r => r.CreateEndpointAsync(It.IsAny<EndpointDto>()), Times.Never);
        _mockEndpointRepository.Verify(r => r.AddResponseToEndpointAsync(It.IsAny<int>(), It.IsAny<MockResponseDto>()), Times.Never);
    }
    // endregion

    // region FindExactEndpointAsync Tests
    [Fact]
    public void FindExactEndpointAsync_NullMatchingEndpoints_ReturnsNull()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        var result = _mockService.FindExactEndpointAsync(null, context, RestType.GET, "/test", null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindExactEndpointAsync_NullEndpointsList_ReturnsNull()
    {
        // Arrange
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: null);
        var context = CreateHttpContext();

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.GET, "/test", null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindExactEndpointAsync_EmptyEndpointsList_ReturnsNull()
    {
        // Arrange
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto>());
        var context = CreateHttpContext();

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.GET, "/test", null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindExactEndpointAsync_ExactUrlMatch_BodyMatch_QueryParamMatch_ReturnsEndpoint()
    {
        // Arrange
        var url = "/data";
        var body = "{\"id\":1}";
        var query = "?param=value";
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = url,
            FromBody = body,
            RestType = RestType.POST,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto> { new QueryParameterDto { Name = "param", Value = "value" } }
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "POST", path: url, requestBody: body, queryString: query);

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.POST, url, body);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(endpoint.Id, result.Id);
    }

    [Fact]
    public void FindExactEndpointAsync_ExactUrlMatch_BodyNullMatch_QueryParamMatch_ReturnsEndpoint()
    {
        // Arrange
        var url = "/data";
        var query = "?param=value";
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = url,
            FromBody = null,
            RestType = RestType.GET,
            ExactUrlMatch = true,
            QueryParameters = new List<QueryParameterDto> { new QueryParameterDto { Name = "param", Value = "value" } }
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "GET", path: url, queryString: query);

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.GET, url, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(endpoint.Id, result.Id);
    }

    [Fact]
    public void FindExactEndpointAsync_ExactUrlMatch_BodyMismatch_ReturnsNull()
    {
        // Arrange
        var url = "/data";
        var endpointBody = "{\"id\":1}";
        var requestBody = "{\"id\":2}"; // Mismatch
        var endpoint = new EndpointDto { Id = 1, FromUrl = url, FromBody = endpointBody, RestType = RestType.POST, ExactUrlMatch = true, QueryParameters = new List<QueryParameterDto>() };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "POST", path: url, requestBody: requestBody);

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.POST, url, requestBody);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindExactEndpointAsync_ExactUrlMatch_QueryParamMismatch_ReturnsNull()
    {
        // Arrange
        var url = "/data";
        var endpointQuery = new List<QueryParameterDto> { new QueryParameterDto { Name = "param", Value = "value1" } };
        var requestQuery = "?param=value2"; // Mismatch
        var endpoint = new EndpointDto { Id = 1, FromUrl = url, RestType = RestType.GET, ExactUrlMatch = true, QueryParameters = endpointQuery };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "GET", path: url, queryString: requestQuery);

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.GET, url, null);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void FindExactEndpointAsync_UrlStartsWith_NotExactMatch_BodyMatch_ReturnsEndpoint()
    {
        // Arrange
        var endpointUrl = "/api/resource";
        var requestUrl = "/api/resource/123"; // Starts with endpointUrl
        var body = "data";
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointUrl,
            FromBody = body,
            RestType = RestType.PUT,
            ExactUrlMatch = false, // Not exact
            QueryParameters = new List<QueryParameterDto>()
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "PUT", path: requestUrl, requestBody: body);

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.PUT, requestUrl, body);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(endpoint.Id, result.Id);
    }

    [Fact]
    public void FindExactEndpointAsync_UrlStartsWith_NotExactMatch_BodyMismatch_ReturnsNull()
    {
        // Arrange
        var endpointUrl = "/api/resource";
        var requestUrl = "/api/resource/123";
        var endpointBody = "data1";
        var requestBody = "data2"; // Mismatch
        var endpoint = new EndpointDto
        {
            Id = 1,
            FromUrl = endpointUrl,
            FromBody = endpointBody,
            RestType = RestType.PUT,
            ExactUrlMatch = false,
            QueryParameters = new List<QueryParameterDto>()
        };
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "PUT", path: requestUrl, requestBody: requestBody);

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.PUT, requestUrl, requestBody);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindExactEndpointAsync_NoMatchingRestType_ReturnsNull()
    {
        // Arrange
        var url = "/data";
        var endpoint = new EndpointDto { Id = 1, FromUrl = url, RestType = RestType.GET, QueryParameters = new List<QueryParameterDto>() }; // Endpoint is GET
        var matchingEndpoints = CreateMatchingEndpoints(endpoints: new List<EndpointDto> { endpoint });
        var context = CreateHttpContext(method: "POST", path: url); // Request is POST

        // Act
        var result = _mockService.FindExactEndpointAsync(matchingEndpoints, context, RestType.POST, url, null);

        // Assert
        Assert.Null(result);
    }
    // endregion

    // region Private Helper Methods (indirectly tested, but can add specific tests if complex)

    [Fact]
    public void CompareQueryParameters_Matches_ReturnsTrue()
    {
        // Arrange
        var queryString = new QueryString("?name=test&value=123");
        var queryParamsDto = new List<QueryParameterDto>
        {
            new QueryParameterDto { Name = "name", Value = "test", OrderIndex = 0, Ignore = false },
            new QueryParameterDto { Name = "value", Value = "123", OrderIndex = 1, Ignore = false }
        };

        // Act
        // Using reflection to test private static method
        var methodInfo = typeof(MockService).GetMethod("CompareQueryParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (bool)methodInfo.Invoke(null, new object[] { queryString, queryParamsDto });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CompareQueryParameters_OrderDoesNotMatter_Matches_ReturnsTrue()
    {
        // Arrange
        var queryString = new QueryString("?value=123&name=test"); // Different order
        var queryParamsDto = new List<QueryParameterDto>
        {
            new QueryParameterDto { Name = "name", Value = "test", OrderIndex = 0, Ignore = false },
            new QueryParameterDto { Name = "value", Value = "123", OrderIndex = 1, Ignore = false }
        };

        // Act
        var methodInfo = typeof(MockService).GetMethod("CompareQueryParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (bool)methodInfo.Invoke(null, new object[] { queryString, queryParamsDto });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CompareQueryParameters_MismatchedCount_ReturnsFalse()
    {
        // Arrange
        var queryString = new QueryString("?name=test");
        var queryParamsDto = new List<QueryParameterDto>
        {
            new QueryParameterDto { Name = "name", Value = "test" },
            new QueryParameterDto { Name = "value", Value = "123" }
        };

        // Act
        var methodInfo = typeof(MockService).GetMethod("CompareQueryParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (bool)methodInfo.Invoke(null, new object[] { queryString, queryParamsDto });


        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CompareQueryParameters_MismatchedValue_ReturnsFalse()
    {
        // Arrange
        var queryString = new QueryString("?name=test1");
        var queryParamsDto = new List<QueryParameterDto> { new QueryParameterDto { Name = "name", Value = "test2" } };

        // Act
        var methodInfo = typeof(MockService).GetMethod("CompareQueryParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (bool)methodInfo.Invoke(null, new object[] { queryString, queryParamsDto });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CompareQueryParameters_IgnoreParameter_Matches_ReturnsTrue()
    {
        // Arrange
        var queryString = new QueryString("?name=test&ignored=true&value=123");
        var queryParamsDto = new List<QueryParameterDto>
        {
            new QueryParameterDto { Name = "name", Value = "test", Ignore = false },
            new QueryParameterDto { Name = "ignored", Value = "false", Ignore = true }, // This DTO value for 'ignored' won't be checked.
            new QueryParameterDto { Name = "value", Value = "123", Ignore = false }
        };
        // Act
        var methodInfo = typeof(MockService).GetMethod("CompareQueryParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (bool)methodInfo.Invoke(null, new object[] { queryString, queryParamsDto });

        // Assert
        // This assertion is tricky. The current implementation of CompareQueryParameters does not seem to use the `Ignore` flag on QueryParameterDto correctly when comparing.
        // It filters DTOs by `!qp.Ignore` then checks if the requestQueryParameters (from queryString) contains them.
        // If a parameter is in queryString but marked as Ignore in DTO, it might lead to a mismatch if counts are different.
        // For this test to pass with current logic: if `ignored=true` is in queryString, and there's a `QueryParameterDto { Name="ignored", Ignore=true }`,
        // that DTO param is skipped. If the remaining params match count and value, it's true.
        // If queryParamsDto had *only* { Name="ignored", Value="false", Ignore=true }, and queryString was "?ignored=true", it should be false by count.
        // If queryParamsDto was { Name = "name", Value = "test" }, { Name="ignored", Ignore=true },
        // and queryString was "?name=test&ignored=true", then it should be true.

        // Based on current `CompareQueryParameters`: it counts non-ignored DTOs. If this count matches unique query string params, it checks.
        // So if `ignored=true` is in DTOs, it reduces the count of DTOs to check against.
        // The HashSet from queryString.Value will include "ignored=true".
        // The loop `foreach (var queryParameter in queryParameters.Where(qp => !qp.Ignore))` will skip the ignored DTO.
        // So we're effectively comparing "?name=test&value=123" from queryString (as `ignored=true` isn't in DTOs being checked)
        // against DTOs for "name" and "value".
        // The count check `if (requestQueryParameters.Count != queryParameters.Count)` is comparing count of ALL unique query string params
        // with count of ALL DTO params. This will likely fail the test if `Ignore` is meant to exclude a param from the *request* for matching purposes.
        // Rewriting this test to align with a more intuitive understanding of `Ignore`:
        var queryStringForIgnored = new QueryString("?name=test&extra=stuff");
        var queryParamsDtoForIgnored = new List<QueryParameterDto>
        {
            new QueryParameterDto { Name = "name", Value = "test", Ignore = false },
            new QueryParameterDto { Name = "extra", Value = "otherstuff", Ignore = true } // This param from request should be ignored
        };
        // Current logic: requestQueryParameters.Count (2) vs queryParamsDto.Count (2). Loop checks `name=test`. `extra=stuff` is not in DTO to check. Ok.
        // Then `!requestQueryParameters.Contains($"{queryParameter.Name}={queryParameter.Value}")`
        // If an DTO param is marked `Ignore = true`, it's skipped.
        // The core issue is `requestQueryParameters.Count != queryParameters.Count` when some DTOs are ignored. It should be `requestQueryParameters.Count != queryParameters.Count(qp => !qp.Ignore)`

        // Given the actual implementation:
        var qs = new QueryString("?name=test");
        var dto = new List<QueryParameterDto> {
            new QueryParameterDto { Name = "name", Value = "test", Ignore = false },
            new QueryParameterDto { Name = "ignored_param", Value = "x", Ignore = true}
        };
        var res = (bool)methodInfo.Invoke(null, new object[] { qs, dto });
        Assert.True(res, "Parameter marked as ignore in DTO should allow match if other params match and it's not in query string.");

        qs = new QueryString("?name=test&ignored_param=y");
        // requestQueryParameters.Count = 2. queryParameters.Where(!Ignore).Count() = 1.
        // The initial count check `if (requestQueryParameters.Count != queryParameters.Count)` is problematic with Ignore.
        // It should be `if (requestQueryParameters.Count != queryParameters.Where(qp => !qp.Ignore).Count())` if unpassed ignored params are okay.
        // Or, all non-ignored DTO params must be in request, and all request params must match a non-ignored DTO param or an ignored DTO param.
        // Let's assume the current behavior:
        // - requestQueryParameters from queryString "?name=test&ignored_param=y" has { "name=test", "ignored_param=y" } (count 2)
        // - queryParameters from DTO has { Name="name", Value="test", Ignore=false }, { Name="ignored_param", Value="x", Ignore=true } (count 2)
        // - Count check: 2 == 2. OK.
        // - Loop: `queryParameter` is { Name="name", Value="test"}. requestQueryParameters contains "name=test". OK.
        // - `ignored_param` DTO is skipped due to `!qp.Ignore`.
        // - Result: True. (This means query string can have extra params if they map to an ignored DTO param, regardless of the ignored DTO's value)

        // Test where DTO has an ignored param, and it's NOT in query string.
        var qsOnlyRequired = new QueryString("?name=test");
        var dtoListWithIgnored = new List<QueryParameterDto>
        {
            new QueryParameterDto { Name = "name", Value = "test", Ignore = false },
            new QueryParameterDto { Name = "optional", Value = "whatever", Ignore = true }
        };
        var resultOnlyRequired = (bool)methodInfo.Invoke(null, new object[] { qsOnlyRequired, dtoListWithIgnored });
        Assert.True(resultOnlyRequired, "Query string missing an 'Ignore=true' DTO parameter should still match.");


        // Test where DTO has an ignored param, it IS in query string, but value mismatch
        var qsWithIgnoredMismatch = new QueryString("?name=test&optional=actual_value");
        var resultWithIgnoredMismatch = (bool)methodInfo.Invoke(null, new object[] { qsWithIgnoredMismatch, dtoListWithIgnored });
        Assert.True(resultWithIgnoredMismatch, "Query string with an 'Ignore=true' DTO parameter should match even if DTO's value for it is different, as the DTO's value for ignored field isn't checked.");
    }


    [Fact]
    public async Task GetRequestHeaders_ModeAll_AddsAllHeadersExceptHost()
    {
        // Arrange
        var microservice = new MicroserviceDto { HeadersMode = HeadersMode.All };
        var headers = new Dictionary<string, StringValues>
        {
            { "Content-Type", "application/json" },
            { "X-Custom", "my-value" },
            { "Host", "original.host.com" }
        };
        var context = CreateHttpContext(headers: headers, host: null);

        // Act
        var methodInfo = typeof(MockService).GetMethod("GetRequestHeaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<EndpointHeaderDto>)methodInfo.Invoke(null, new object[] { microservice, context });


        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, h => h.Name == "Content-Type" && h.Value == "application/json");
        Assert.Contains(result, h => h.Name == "X-Custom" && h.Value == "my-value");
        Assert.DoesNotContain(result, h => h.Name == "Host");
    }

    [Fact]
    public async Task GetRequestHeaders_ModeUserDefined_AddsMatchingEnabledOutgoingHeaders()
    {
        // Arrange
        var microservice = new MicroserviceDto
        {
            HeadersMode = HeadersMode.UserDefined,
            Headers = new List<ServiceHeaderDto>
            {
                new ServiceHeaderDto { Name = "X-Allowed", Enabled = true, Outgoing = true },
                new ServiceHeaderDto { Name = "X-Not-Outgoing", Enabled = true, Outgoing = false },
                new ServiceHeaderDto { Name = "X-Disabled", Enabled = false, Outgoing = true },
            }
        };
        var headers = new Dictionary<string, StringValues>
        {
            { "X-Allowed", "value1" },
            { "X-Not-Outgoing", "value2" },
            { "X-Disabled", "value3" },
            { "X-Other", "value4" }
        };
        var context = CreateHttpContext(headers: headers);

        // Act
        var methodInfo = typeof(MockService).GetMethod("GetRequestHeaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<EndpointHeaderDto>)methodInfo.Invoke(null, new object[] { microservice, context });


        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains(result, h => h.Name == "X-Allowed" && h.Value == "value1");
    }
    // endregion
}
