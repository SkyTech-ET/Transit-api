namespace Transit.Api.Contracts.Common
{
    public class ErrorResponse
    {
        public bool Error { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public string Message { get; set; }
        public string Response { get; set; }

    }

    public class ApiResponse<T>
    {

        public int StatusCode { get; set; }
        public bool Error { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public string Message { get; set; }
        public Response<T> Response { get; set; }

    }

    public class Response<T>
    {
        public T Data { get; set; }
    }

}
