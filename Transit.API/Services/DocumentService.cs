using Transit.Domain.Data;
using Transit.Domain.Models.MOT;
using Transit.Domain.Models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Transit.API.Services;

public interface IDocumentService
{
    Task<ServiceDocument> UploadServiceDocumentAsync(long serviceId, long uploadedByUserId, IFormFile file, DocumentType documentType, long? serviceStageId = null, string? description = null);
    Task<StageDocument> UploadStageDocumentAsync(long serviceStageId, long uploadedByUserId, IFormFile file, DocumentType documentType, string? description = null);
    Task<CustomerDocument> UploadCustomerDocumentAsync(long customerId, long uploadedByUserId, IFormFile file, DocumentType documentType, string? description = null);
    Task<List<ServiceDocument>> GetServiceDocumentsAsync(long serviceId);
    Task<List<StageDocument>> GetStageDocumentsAsync(long serviceStageId);
    Task<List<CustomerDocument>> GetCustomerDocumentsAsync(long customerId);
    Task<ServiceDocument?> GetServiceDocumentAsync(long documentId);
    Task<StageDocument?> GetStageDocumentAsync(long documentId);
    Task<CustomerDocument?> GetCustomerDocumentAsync(long documentId);
    Task<bool> DeleteServiceDocumentAsync(long documentId);
    Task<bool> DeleteStageDocumentAsync(long documentId);
    Task<bool> DeleteCustomerDocumentAsync(long documentId);
    Task<bool> VerifyDocumentAsync(long documentId, long verifiedByUserId, bool isVerified, string? verificationNotes = null);
    Task<string> GetDocumentPathAsync(long documentId, DocumentCategory category);
}

public enum DocumentCategory
{
    Service,
    Stage,
    Customer
}

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly string _documentsPath;

    public DocumentService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
        _documentsPath = Path.Combine(_environment.WebRootPath, "Documents");
        
        // Ensure documents directory exists
        if (!Directory.Exists(_documentsPath))
        {
            Directory.CreateDirectory(_documentsPath);
        }
    }

    public async Task<ServiceDocument> UploadServiceDocumentAsync(long serviceId, long uploadedByUserId, IFormFile file, DocumentType documentType, long? serviceStageId = null, string? description = null)
    {
        var fileName = await SaveFileAsync(file, "Service", serviceId.ToString());
        var filePath = Path.Combine(_documentsPath, "Service", serviceId.ToString(), fileName);
        
        var document = ServiceDocument.Create(
            fileName,
            filePath,
            file.FileName,
            Path.GetExtension(file.FileName),
            file.Length,
            file.ContentType,
            documentType,
            serviceId,
            uploadedByUserId,
            serviceStageId,
            description
        );

        _context.ServiceDocuments.Add(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<StageDocument> UploadStageDocumentAsync(long serviceStageId, long uploadedByUserId, IFormFile file, DocumentType documentType, string? description = null)
    {
        var fileName = await SaveFileAsync(file, "Stage", serviceStageId.ToString());
        var filePath = Path.Combine(_documentsPath, "Stage", serviceStageId.ToString(), fileName);
        
        var document = StageDocument.Create(
            fileName,
            filePath,
            file.FileName,
            Path.GetExtension(file.FileName),
            file.Length,
            file.ContentType,
            documentType,
            serviceStageId,
            uploadedByUserId,
            description
        );

        _context.StageDocuments.Add(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<CustomerDocument> UploadCustomerDocumentAsync(long customerId, long uploadedByUserId, IFormFile file, DocumentType documentType, string? description = null)
    {
        var fileName = await SaveFileAsync(file, "Customer", customerId.ToString());
        var filePath = Path.Combine(_documentsPath, "Customer", customerId.ToString(), fileName);
        
        var document = CustomerDocument.Create(
            fileName,
            filePath,
            file.FileName,
            Path.GetExtension(file.FileName),
            file.Length,
            file.ContentType,
            documentType,
            customerId,
            uploadedByUserId,
            description
        );

        _context.CustomerDocuments.Add(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<List<ServiceDocument>> GetServiceDocumentsAsync(long serviceId)
    {
        return await _context.ServiceDocuments
            .Where(d => d.ServiceId == serviceId)
            .Include(d => d.UploadedByUser)
            .Include(d => d.ServiceStage)
            .OrderByDescending(d => d.RegisteredDate)
            .ToListAsync();
    }

    public async Task<List<StageDocument>> GetStageDocumentsAsync(long serviceStageId)
    {
        return await _context.StageDocuments
            .Where(d => d.ServiceStageId == serviceStageId)
            .Include(d => d.UploadedByUser)
            .OrderByDescending(d => d.RegisteredDate)
            .ToListAsync();
    }

    public async Task<List<CustomerDocument>> GetCustomerDocumentsAsync(long customerId)
    {
        return await _context.CustomerDocuments
            .Where(d => d.CustomerId == customerId)
            .Include(d => d.UploadedByUser)
            .OrderByDescending(d => d.RegisteredDate)
            .ToListAsync();
    }

    public async Task<ServiceDocument?> GetServiceDocumentAsync(long documentId)
    {
        return await _context.ServiceDocuments
            .Include(d => d.UploadedByUser)
            .Include(d => d.Service)
            .Include(d => d.ServiceStage)
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<StageDocument?> GetStageDocumentAsync(long documentId)
    {
        return await _context.StageDocuments
            .Include(d => d.UploadedByUser)
            .Include(d => d.ServiceStage)
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<CustomerDocument?> GetCustomerDocumentAsync(long documentId)
    {
        return await _context.CustomerDocuments
            .Include(d => d.UploadedByUser)
            .Include(d => d.Customer)
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<bool> DeleteServiceDocumentAsync(long documentId)
    {
        var document = await _context.ServiceDocuments.FindAsync(documentId);
        if (document == null) return false;

        // Delete physical file
        var filePath = Path.Combine(_documentsPath, "Service", document.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.ServiceDocuments.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteStageDocumentAsync(long documentId)
    {
        var document = await _context.StageDocuments.FindAsync(documentId);
        if (document == null) return false;

        // Delete physical file
        var filePath = Path.Combine(_documentsPath, "Stage", document.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.StageDocuments.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerDocumentAsync(long documentId)
    {
        var document = await _context.CustomerDocuments.FindAsync(documentId);
        if (document == null) return false;

        // Delete physical file
        var filePath = Path.Combine(_documentsPath, "Customer", document.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.CustomerDocuments.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyDocumentAsync(long documentId, long verifiedByUserId, bool isVerified, string? verificationNotes = null)
    {
        // This method can be used for any document type
        var serviceDocument = await _context.ServiceDocuments.FindAsync(documentId);
        if (serviceDocument != null)
        {
            serviceDocument.Verify(verifiedByUserId, verificationNotes);
            await _context.SaveChangesAsync();
            return true;
        }

        var stageDocument = await _context.StageDocuments.FindAsync(documentId);
        if (stageDocument != null)
        {
            stageDocument.Verify(verifiedByUserId, verificationNotes);
            await _context.SaveChangesAsync();
            return true;
        }

        var customerDocument = await _context.CustomerDocuments.FindAsync(documentId);
        if (customerDocument != null)
        {
            customerDocument.VerifyDocument(isVerified, verificationNotes);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<string> GetDocumentPathAsync(long documentId, DocumentCategory category)
    {
        string fileName = string.Empty;
        
        switch (category)
        {
            case DocumentCategory.Service:
                var serviceDoc = await _context.ServiceDocuments.FindAsync(documentId);
                fileName = serviceDoc?.FileName ?? string.Empty;
                break;
            case DocumentCategory.Stage:
                var stageDoc = await _context.StageDocuments.FindAsync(documentId);
                fileName = stageDoc?.FileName ?? string.Empty;
                break;
            case DocumentCategory.Customer:
                var customerDoc = await _context.CustomerDocuments.FindAsync(documentId);
                fileName = customerDoc?.FileName ?? string.Empty;
                break;
        }

        if (string.IsNullOrEmpty(fileName))
            return string.Empty;

        return Path.Combine(_documentsPath, category.ToString(), fileName);
    }

    private async Task<string> SaveFileAsync(IFormFile file, string category, string entityId)
    {
        var categoryPath = Path.Combine(_documentsPath, category);
        if (!Directory.Exists(categoryPath))
        {
            Directory.CreateDirectory(categoryPath);
        }

        var entityPath = Path.Combine(categoryPath, entityId);
        if (!Directory.Exists(entityPath))
        {
            Directory.CreateDirectory(entityPath);
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(entityPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }
}
