using Microsoft.AspNetCore.Http;
using Mockbench.Http.Middleware;
using Mockbench.Shared.Constants;
using Moq;

namespace Mockbench.Http.Tests;

public class MockHeaderIsolationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNextDelegate;
    private readonly MockHeaderIsolationMiddleware _middleware;

    public MockHeaderIsolationMiddlewareTests()
    {
        _mockNextDelegate = new Mock<RequestDelegate>();
        _middleware = new MockHeaderIsolationMiddleware(_mockNextDelegate.Object);
    }

    private DefaultHttpContext CreateHttpContext(string path = "/", Dictionary<string, string>? initialHeaders = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(path);
        if (initialHeaders != null)
        {
            foreach (var header in initialHeaders)
            {
                context.Request.Headers.Add(header.Key, header.Value);
            }
        }
        return context;
    }

    [Fact]
    public void Constructor_NullRequestDelegate_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("next", () => new MockHeaderIsolationMiddleware(null!));
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate_Always()
    {
        // Arrange
        var context = CreateHttpContext();
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_NotApiMockPath_RemovesMockbenchPrefixFromHeaders()
    {
        // Arrange
        var initialHeaders = new Dictionary<string, string>
        {
            { SharedConstants.MockbenchHeaderPrefix + "Tenant", "TestTenant" },
            { SharedConstants.MockbenchHeaderPrefix + "Environment", "Dev" },
            { "X-Custom-Header", "CustomValue" }
        };
        var context = CreateHttpContext("/some/other/path", initialHeaders);
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Request.Headers.ContainsKey(SharedConstants.MockbenchHeaderPrefix + "Tenant"));
        Assert.False(context.Request.Headers.ContainsKey(SharedConstants.MockbenchHeaderPrefix + "Environment"));
        Assert.True(context.Request.Headers.ContainsKey("Tenant"));
        Assert.Equal("TestTenant", context.Request.Headers["Tenant"]);
        Assert.True(context.Request.Headers.ContainsKey("Environment"));
        Assert.Equal("Dev", context.Request.Headers["Environment"]);
        Assert.True(context.Request.Headers.ContainsKey("X-Custom-Header"));
        Assert.Equal("CustomValue", context.Request.Headers["X-Custom-Header"]);
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_NotApiMockPath_NoMockbenchHeaders_HeadersUnchanged()
    {
        // Arrange
        var initialHeaders = new Dictionary<string, string>
        {
            { "X-Another-Header", "AnotherValue" },
            { "Content-Type", "application/xml" }
        };
        var context = CreateHttpContext("/some/other/path", initialHeaders);
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(2, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("X-Another-Header"));
        Assert.Equal("AnotherValue", context.Request.Headers["X-Another-Header"]);
        Assert.True(context.Request.Headers.ContainsKey("Content-Type"));
        Assert.Equal("application/xml", context.Request.Headers["Content-Type"]);
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ApiMockPath_PrefixesNonMockbenchHeaders_ClearsOriginals()
    {
        // Arrange
        var initialHeaders = new Dictionary<string, string>
        {
            { SharedConstants.MockbenchHeaderPrefix + "Tenant", "IgnoredTenant" }, // This should be filtered out
            { "X-Custom-Header", "CustomValue" },
            { "Accept", "application/json" }
        };
        var context = CreateHttpContext("/api/mock/test-endpoint", initialHeaders);
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        // Original X-Mockbench-Tenant should be gone because it's not included in newHeaders
        Assert.False(context.Request.Headers.ContainsKey(SharedConstants.MockbenchHeaderPrefix + "Tenant"));
        Assert.False(context.Request.Headers.ContainsKey("Tenant")); // And not transformed by the second block

        // Original non-X-Mockbench headers should be gone
        Assert.False(context.Request.Headers.ContainsKey("X-Custom-Header"));
        Assert.False(context.Request.Headers.ContainsKey("Accept"));

        // New prefixed headers should exist
        Assert.True(context.Request.Headers.ContainsKey(SharedConstants.MockHeaderIsolationPrefix + "X-Custom-Header"));
        Assert.Equal("CustomValue", context.Request.Headers[SharedConstants.MockHeaderIsolationPrefix + "X-Custom-Header"]);
        Assert.True(context.Request.Headers.ContainsKey(SharedConstants.MockHeaderIsolationPrefix + "Accept"));
        Assert.Equal("application/json", context.Request.Headers[SharedConstants.MockHeaderIsolationPrefix + "Accept"]);

        // Verify only the prefixed headers are present
        Assert.Equal(2, context.Request.Headers.Count);
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ApiMockPath_NoRelevantHeaders_ClearsHeaders()
    {
        // Arrange
        var initialHeaders = new Dictionary<string, string>
        {
            // Only headers that will be filtered out by the first loop's condition
            { SharedConstants.MockbenchHeaderPrefix + "Tenant", "SomeTenant" },
            { SharedConstants.MockbenchHeaderPrefix + "Environment", "SomeEnv" }
        };
        var context = CreateHttpContext("/api/mock/another", initialHeaders);
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Empty(context.Request.Headers); // All original headers started with X-Mockbench-, so newHeaders was empty
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ApiMockPath_PathIsNull_ActsAsNotApiMockPath()
    {
        // Arrange
        var initialHeaders = new Dictionary<string, string>
        {
            { SharedConstants.MockbenchHeaderPrefix + "Tenant", "TestTenant" },
            { "X-Custom-Header", "CustomValue" }
        };
        var context = CreateHttpContext(null, initialHeaders); // Path is null
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);


        // Act
        await _middleware.InvokeAsync(context);

        // Assert: Should behave as if it's not an /api/mock/ path
        Assert.False(context.Request.Headers.ContainsKey(SharedConstants.MockbenchHeaderPrefix + "Tenant"));
        Assert.True(context.Request.Headers.ContainsKey("Tenant"));
        Assert.Equal("TestTenant", context.Request.Headers["Tenant"]);
        Assert.True(context.Request.Headers.ContainsKey("X-Custom-Header"));
        Assert.Equal("CustomValue", context.Request.Headers["X-Custom-Header"]);
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_NotApiMockPath_KeyBecomesEmptyAfterPrefixRemoval_HeaderNotAdded()
    {
        // Arrange
        // Create a header key that is exactly the MockbenchHeaderPrefix
        var initialHeaders = new Dictionary<string, string>
        {
            { SharedConstants.MockbenchHeaderPrefix, "ThisShouldBeRemovedNotReadded" } // e.g. "X-Mockbench" if prefix is "X-Mockbench-"
        };
        // To make this test meaningful, let's assume for this test, the prefix has a trailing char that would be part of Substring
        // Or, if the prefix itself is the key, newKey would be empty.
        // If SharedConstants.MockbenchHeaderPrefix is "X-Mockbench-", and key is "X-Mockbench-", newKey will be "".
        initialHeaders[SharedConstants.MockbenchHeaderPrefix] = "ValueForFullPrefixKey";

        var context = CreateHttpContext("/not/api/mock", initialHeaders);
        _mockNextDelegate.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Request.Headers.ContainsKey(SharedConstants.MockbenchHeaderPrefix)); // Original removed
        Assert.False(context.Request.Headers.ContainsKey("")); // New key (empty string) should not be added
        Assert.Empty(context.Request.Headers); // Assuming the other test header key was also a prefix or similar
        _mockNextDelegate.Verify(next => next(context), Times.Once);
    }
}
