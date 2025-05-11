namespace Mockbench.Api.Helpers
{
    public static class HelperExtensions
    {
        public static bool TryParseParamCodes(
             string code,      // Should not be null, pass string.Empty if no code
             string rest,      // Should not be null, pass string.Empty if no rest
             out string? tenant,
             out string? environment,
             out string? micro,
             out string? endpoint, // Changed to nullable string
             out string? error)
        {
            tenant = null;
            environment = null;
            micro = null;
            endpoint = null; // Initialize as null
            error = null;

            var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Handle the case where 'code' is empty
            if (string.IsNullOrEmpty(code))
            {
                // tenant, environment, and microservice remain null
                if (parts.Length == 0)
                {
                    endpoint = null; // Root call, endpoint is null
                }
                else
                {
                    endpoint = string.Join('/', parts); // Endpoint is the entire 'rest'
                }
                return true; // Successfully parsed
            }

            // Original logic for when 'code' is not empty
            if (parts.Length < code.Length)
            {
                error = $"Expected {code.Length} path segment(s) for codes '{code}', but got {parts.Length} from '{rest}'.";
                return false;
            }

            for (int i = 0; i < code.Length; i++)
            {
                // Basic validation for path segments if needed (e.g., not empty)
                if (string.IsNullOrEmpty(parts[i]))
                {
                    error = $"Path segment for code '{code[i]}' at position {i + 1} cannot be empty in '{rest}'.";
                    return false;
                }
                switch (code[i])
                {
                    case 't': tenant = parts[i]; break;
                    case 'e': environment = parts[i]; break;
                    case 'm': micro = parts[i]; break;
                    default:
                        // This case should ideally not be hit due to the route regex,
                        // but good for robustness.
                        error = $"Invalid character '{code[i]}' in code parameter.";
                        return false;
                }
            }

            if (parts.Length > code.Length)
            {
                endpoint = string.Join('/', parts.Skip(code.Length));
            }
            else if (parts.Length == code.Length)
            {
                // If parts.Length equals code.Length, there's no "remaining" part for the endpoint path.
                // This could mean the endpoint is effectively the root of the specified t/e/m combination.
                // Depending on requirements, endpoint could be string.Empty or null.
                // Setting to string.Empty to indicate "no further path segments".
                endpoint = string.Empty;
            }
            // If parts.Length < code.Length, it's already handled by the check above.

            return true;
        }
    }
}
