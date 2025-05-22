using Mockbench.Shared.Models.Response;

namespace Mockbench.Data.Models.Defaults
{
    public static class Defaults
    {
        private static MockResponse _response;

        public static MockResponse Response {
            get
            {
                if (_response == null)
                {
                    _response = new MockResponse()
                    {
                        Body = "{\n\t\"Warning\": \"[No mock setup configured for this end point]\"\n}",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                return _response;
            }
        }


        private static MockResponseDto _responseDto;

        public static MockResponseDto ResponseDto
        {
            get
            {
                if (_responseDto == null)
                {
                    _responseDto = new MockResponseDto()
                    {
                        Body = Response.Body,
                        StatusCode = Response.StatusCode
                    };
                }

                return _responseDto;
            }
        }
    }
}