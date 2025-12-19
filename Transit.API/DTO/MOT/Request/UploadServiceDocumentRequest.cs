using System.ComponentModel.DataAnnotations;
using Transit.Domain.Models.Shared;

namespace Transit.API.DTO.MOT;

public class UploadServiceDocumentRequest
{
    [Required]
    public long ServiceId { get; set; }

   public long? serviceStageId { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!; // null-forgiving because it's required
    public string? FilePath { get; set; }

    [Required]
    public DocumentType DocumentType { get; set; }

    public string? Description { get; set; }

}
