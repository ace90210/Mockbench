using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Mockbench.Shared.Models.Enum;

namespace Mockbench.Shared.Helper
{
    public static class ConvertHelper
    {
        public static Encoding Convert(SupportedEncodingType encodingType)
        {
            switch (encodingType)
            {
                case SupportedEncodingType.ACSCII: return Encoding.ASCII;
                case SupportedEncodingType.UNICODE: return Encoding.Unicode;
                case SupportedEncodingType.UTF8: return Encoding.UTF8;
            }
            return Encoding.UTF8;
        }

        public static byte[] ToByteArray(SupportedEncodingType encodingType, string obj)
        {
            switch (encodingType)
            {
                case SupportedEncodingType.ACSCII: return Encoding.ASCII.GetBytes(obj);
                case SupportedEncodingType.UNICODE: return Encoding.Unicode.GetBytes(obj);
                case SupportedEncodingType.UTF8: return Encoding.UTF8.GetBytes(obj);
            }
            return Encoding.UTF8.GetBytes(obj);
        }

        public static StringContent ToExactStringContent(string content, string contentType)
        {
            var stringContent = new StringContent(content);

            if (contentType != null)
            {
                //workaround to bug where utf-8 added to content type and breaking forwarded requests
                if (contentType.Contains("utf-8"))
                {
                    contentType = contentType.Replace("; charset=utf-8", "");
                }
                else if (contentType.Contains("us-ascii"))
                {
                    contentType = contentType.Replace("; charset=us-ascii", "");
                }
                stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            else
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return stringContent;
        }
    }
}