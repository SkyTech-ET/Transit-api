
using Transit.Api.Contracts.Common;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Transit.Application;
public interface IAPICallerService : IDisposable
{
    ResponseDto responseModel { get; set; }
    Task<T> SendAsync<T>(ApiRequest apiRequest);
}

public class APICallerService : IAPICallerService
{
    public ResponseDto responseModel { get; set; }
    public IHttpClientFactory httpClient { get; set; }

    public APICallerService(IHttpClientFactory httpClient)
    {
        this.responseModel = new ResponseDto();
        this.httpClient = httpClient;
    }

    public async Task<T> SendAsync<T>(ApiRequest apiRequest)
    {
        try
        {
            var client = httpClient.CreateClient("PaymentServiceServiceAPI");
            HttpRequestMessage message = new HttpRequestMessage();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri(apiRequest.Url);
            client.DefaultRequestHeaders.Clear();
            if (apiRequest.Data != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                    Encoding.UTF8, "application/json");
            }

            if (!string.IsNullOrEmpty(apiRequest.AccessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.AccessToken);

            if (apiRequest.NeedApiKey)
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiRequest.ApiKey);

            if (apiRequest.IsBasic)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(apiRequest.BasicAuth));



            HttpResponseMessage apiResponse = null;
            switch (apiRequest.ApiType)
            {
                case ApiType.POST:
                    message.Method = HttpMethod.Post;
                    break;
                case ApiType.PUT:
                    message.Method = HttpMethod.Put;
                    break;
                case ApiType.DELETE:
                    message.Method = HttpMethod.Delete;
                    break;
                default:
                    message.Method = HttpMethod.Get;
                    break;
            }
            apiResponse = await client.SendAsync(message);
            var apiContent = await apiResponse.Content.ReadAsStringAsync();

            if (apiResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return JsonConvert.DeserializeObject<T>(apiContent);

            if (!apiResponse.IsSuccessStatusCode)
            {

                var responseDto = JsonConvert.DeserializeObject<ErrorResponse>(apiContent);
                var dto = new ResponseDto
                {
                    DisplayMessage = "Error",
                    ErrorResponse = responseDto,
                    IsSuccess = false,
                    IsError = true,
                    Message = responseDto.Errors.Count > 0 ? responseDto.Errors[0] : responseDto.Message,

                };
                if (responseDto.Errors.Count > 0)
                    dto.Errors.Add(new Error() { Message = responseDto.Errors[0] });

                var res = JsonConvert.SerializeObject(dto);
                var errorResponseDto = JsonConvert.DeserializeObject<T>(res);
                return errorResponseDto;

            }

            var apiResponseDto = JsonConvert.DeserializeObject<T>(apiContent);
            return apiResponseDto;
        }
        catch (Exception e)
        {
            var dto = new ResponseDto
            {
                DisplayMessage = "Error",
                ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                IsSuccess = false,

            };
            var res = JsonConvert.SerializeObject(dto);
            var apiResponseDto = JsonConvert.DeserializeObject<T>(res);
            return apiResponseDto;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(true);
    }
}


public class ApiRequest
{
    public ApiType ApiType { get; set; } = ApiType.GET;
    public string Url { get; set; }
    public object Data { get; set; }
    public string AccessToken { get; set; }
    public string ApiKey { get; set; }
    public bool NeedApiKey { get; set; }
    public bool IsBasic { get; set; }
    public byte[] BasicAuth { get; set; }
}

public class ResponseDto
{
    public bool IsSuccess { get; set; } = true;
    public object Result { get; set; }
    public string DisplayMessage { get; set; } = "";
    public List<string> ErrorMessages { get; set; }
    public ErrorResponse ErrorResponse { get; set; }
    public bool IsError { get; set; }
    public string Message { get; set; }

    public List<Error> Errors { get; set; } = new List<Error>();


}

public enum ApiType
{
    GET,
    POST,
    PUT,
    DELETE
}