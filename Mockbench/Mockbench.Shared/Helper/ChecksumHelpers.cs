using Mockbench.Shared.Models.Response;
using System.Security.Cryptography;

namespace Mockbench.Shared.Helper
{
    public static class ChecksumHelpers
    {
        public static string CreateDefaultChecksum(IMockResponse mockResponse)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(mockResponse.Encoding, $"{mockResponse.Body}-{mockResponse.StatusCode}-{mockResponse.ContentType}");
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }

        public static string CreateUniqueDefaultChecksum(IMockResponse mockResponse)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(mockResponse.Encoding, $"{mockResponse.Body}-{mockResponse.StatusCode}-{mockResponse.ContentType}-{mockResponse.CreatedUtc?.Ticks}");
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }
    }
}
