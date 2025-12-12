using Transit.Domain.Models.Shared;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Transit.Api.Contracts.User.Request;

public class UserRequest
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }
    [Required]
    [MinLength(3)]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }
    [Required]
    [MinLength(3)]
    public string LastName { get; set; }
    /// <summary>
    /// Gets or sets the logo file to be uploaded for the landlord.
    /// </summary>
    [Required]
    public IFormFile ProfileFile { get; set; } // For file upload, changed to non-nullable

    /// <summary>
    /// Gets or sets the path where the logo will be saved, not to be set by the client.
    /// </summary>
    [JsonIgnore]
    public string? ProfilePhoto { get; set; } // For the path after saving, server-side only
    public string Phone { get; set; }
    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    public bool? IsSuperAdmin { get; set; } = false;
    public List<long>? Roles { get; set; }
}

public class UpdateUserRequest
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public IFormFile? ProfileFile { get; set; } // For file upload, changed to non-nullable

    /// <summary>
    /// Gets or sets the path where the logo will be saved, not to be set by the client.
    /// </summary>
    [JsonIgnore]
    public string? ProfilePhoto { get; set; } // For the path after saving, server-side only
    public string Phone { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public bool? IsSuperAdmin { get; set; }
    public List<long>? Roles { get; set; }
}
