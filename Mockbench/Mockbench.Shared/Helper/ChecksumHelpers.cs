using Mockbench.Shared.Models.Enum;
using Mockbench.Shared.Models.Response;
using System;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace Mockbench.Shared.Helper
{
    public static class ChecksumHelpers
    {
        public static string CreateChecksum(SupportedEncodingType encoding, string valueToChecksum)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(encoding, valueToChecksum);
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }

        public static string CreateDefaultChecksum(UpdateMockResponseDto mockResponse)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(mockResponse.Encoding, $"{mockResponse.Body}-{mockResponse.Code}-{mockResponse.ContentType}");
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }

        public static string CreateDefaultChecksum(MockResponseDto mockResponse)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(mockResponse.Encoding, $"{mockResponse.Body}-{mockResponse.Code}-{mockResponse.ContentType}");
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }

        public static string CreateUniqueDefaultChecksum(MockResponseDto mockResponse)
        {
            using var sha = SHA1.Create();

            byte[] valueInBytes = ConvertHelper.ToByteArray(mockResponse.Encoding, $"{mockResponse.Body}-{mockResponse.Code}-{mockResponse.ContentType}-{mockResponse.CreatedUtc.Ticks}");
            byte[] shaChecksumBytes = sha.ComputeHash(valueInBytes);

            return BitConverter.ToString(shaChecksumBytes).Replace("-", string.Empty).ToLower();
        }
    }
}
