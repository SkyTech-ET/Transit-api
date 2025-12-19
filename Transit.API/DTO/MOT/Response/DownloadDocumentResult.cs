namespace Transit.API.DTO.MOT;
public class DownloadDocumentResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}