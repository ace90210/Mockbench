using System.Text;
using Microsoft.AspNetCore.Http;

namespace Mockbench.Services.Helpers
{
    public static class GeneralHelpers
    {
        public static async Task<string> RequestBodyToStringAsync(HttpRequest request)
        {
            try
            {
                var body = "";
                if (request.ContentLength == null || !(request.ContentLength > 0) ||
                    !request.Body.CanSeek) return body;
                request.Body.Seek(0, SeekOrigin.Begin);
                
                using (var reader = new StreamReader(request.Body, Encoding.Default, true, 1024, true))
                {
                    body = await reader.ReadToEndAsync();
                }

                request.Body.Position = 0;
                return body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
