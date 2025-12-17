using Microsoft.AspNetCore.Http;

namespace MedicalCenter.WebApi.Endpoints.Records;

public class UploadAttachmentRequest
{
    public IFormFile? File { get; set; }
}
