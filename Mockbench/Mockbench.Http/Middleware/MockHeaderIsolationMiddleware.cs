namespace Mockbench.Server.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Mockbench.Shared.Constants;
    using System.Threading.Tasks;

    public class MockHeaderIsolationMiddleware
    {
        private readonly RequestDelegate _next;

        public MockHeaderIsolationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.Request.Path.Value.StartsWith("/api/mock/"))
            {
                var newHeaders = new HeaderDictionary();
                foreach (var header in context.Request.Headers.Where(h => !h.Key.StartsWith("Mockbench-")))
                {
                    // Prefix the header name and add it to the new headers collection
                    newHeaders[$"{SharedConstants.MockHeaderIsolationPrefix}{header.Key}"] = header.Value;
                }

                // Clear the original headers and add the modified headers
                context.Request.Headers.Clear();
                foreach (var header in newHeaders)
                {
                    context.Request.Headers.Add(header);
                }
            }

            // allow Mockbench headers to behave normally by removing Mockbench prefix
            foreach (var header in context.Request.Headers.Where(h => h.Key.StartsWith(SharedConstants.MockbenchHeaderPrefix)))
            {
                var newKey = header.Key.Substring("Mockbench-".Length);
                context.Request.Headers.Remove(header.Key);
                context.Request.Headers[newKey] = header.Value; 
            }

            await _next(context);
        }
    }

}
