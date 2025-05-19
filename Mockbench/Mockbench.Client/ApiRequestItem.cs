namespace Mockbench.Client
{
    public class ApiRequestItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Untitled Request";
        public string Url { get; set; } = "";
        public string Method { get; set; } = "GET"; // e.g., GET, POST
        public string? RequestBody { get; set; }
        public string? RequestContentType { get; set; } = "application/json"; // Default for new requests

        // Store last response
        public string? ResponseBody { get; set; }
        public int? ResponseStatusCode { get; set; }
        public Dictionary<string, List<string>>? ResponseHeaders { get; set; }
        public string? ResponseContentType { get; set; }
        public long? ResponseTimeMs { get; set; }
    }
}
