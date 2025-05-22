using Mockbench.Data.Models;
using Mockbench.Shared.Helper;
using System.Security.Cryptography;

namespace Mockbench.Data.Helpers
{
    public static class ChecksumEntityHelpers
    {
        public static string CreateDefaultChecksum(MockResponse mockResponse)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(mockResponse.Encoding, $"{mockResponse.Body}-{mockResponse.StatusCode}-{mockResponse.ContentType}");
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }
    }
}
