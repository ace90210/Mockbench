namespace Mockbench.Api.Helpers
{
    public static class HelperExtensions
    {
        public static bool TryParseParamCodes(
            string code,
            string rest,
            out string? tenant,
            out string? environment,
            out string? micro,
            out string endpoint,
            out string? error)
        {
            tenant = environment = micro = null;
            endpoint = string.Empty;
            error = null;

            var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < code.Length)
            {
                error = $"Expected {code.Length} path segment(s) " +
                        $"after '{code}', got {parts.Length}.";
                return false;
            }

            for (int i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case 't': tenant = parts[i]; break;
                    case 'e': environment = parts[i]; break;
                    case 'm': micro = parts[i]; break;
                }
            }

            if (parts.Length > code.Length)
                endpoint = string.Join('/', parts.Skip(code.Length));

            return true;
        }
    }
}
