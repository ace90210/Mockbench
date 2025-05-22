using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Mockbench.Shared.Constants;

namespace Mockbench.Http.Middleware;

public class MockHeaderIsolationMiddleware
{
    private readonly RequestDelegate _next;

    public MockHeaderIsolationMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value != null && context.Request.Path.Value.StartsWith("/api/mock/"))
        {
            var newHeaders = new HeaderDictionary();
            // Collect non-Mockbench headers and prefix them
            foreach (var header in context.Request.Headers.Where(h => !h.Key.StartsWith(SharedConstants.MockbenchHeaderPrefix)))
            {
                newHeaders[$"{SharedConstants.MockHeaderIsolationPrefix}{header.Key}"] = header.Value;
            }

            // Replace original headers with the new prefixed ones
            context.Request.Headers.Clear();
            foreach (var header in newHeaders)
            {
                context.Request.Headers.Add(header);
            }
        }
        else // Only apply the Mockbench prefix removal if not an /api/mock/ path
        {
            // Identify headers that need their "X-Mockbench-" prefix removed.
            // Materialize the list to avoid modification issues during iteration.
            var headersToModify = context.Request.Headers
                .Where(h => h.Key.StartsWith(SharedConstants.MockbenchHeaderPrefix))
                .Select(h => new KeyValuePair<string, StringValues>(h.Key, h.Value)) // Capture key and value
                .ToList();

            if (headersToModify.Any())
            {
                foreach (var headerKvp in headersToModify)
                {
                    var originalKey = headerKvp.Key;
                    var value = headerKvp.Value;
                    var newKey = originalKey.Substring(SharedConstants.MockbenchHeaderPrefix.Length);

                    context.Request.Headers.Remove(originalKey);

                    // Only add if newKey is not empty (prefix itself was not the entire key)
                    // and to avoid issues if newKey accidentally collides with an existing, differently cased key after removal.
                    // Though HeaderDictionary is case-insensitive for keys.
                    if (!string.IsNullOrEmpty(newKey))
                    {
                        // If the newKey already exists (e.g. from a previous transformation or original header),
                        // this will replace its value. If it doesn't exist, it adds it.
                        context.Request.Headers[newKey] = value;
                    }
                }
            }
        }

        await _next(context);
    }
}

