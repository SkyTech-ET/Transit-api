using System.Net;

namespace Transit.Application.Helper;

public class FileUploadResponse : OperationStatusResponse
{
    public string? FilePath { get; set; }
}

public class FileStreamProcessResponse : OperationStatusResponse
{
    public byte[]? File { get; set; }
}

[Serializable]
public class OperationStatusResponse
{
    public string? Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
