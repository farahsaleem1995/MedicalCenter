using FastEndpoints;
using MedicalCenter.Core.Aggregates.MedicalRecords;
using MedicalCenter.Core.Aggregates.MedicalRecords.Specifications;
using MedicalCenter.Core.Aggregates.MedicalRecords.ValueObjects;
using MedicalCenter.Core.Aggregates.Patients;
using MedicalCenter.Core.Aggregates.Patients.Specifications;
using MedicalCenter.Core.Primitives;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.Authorization;

namespace MedicalCenter.WebApi.Endpoints.Records;

/// <summary>
/// Create medical record endpoint.
/// </summary>
public class CreateRecordEndpoint(
    IRepository<MedicalRecord> recordRepository,
    IRepository<Patient> patientRepository,
    IFileStorageService fileStorageService,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : Endpoint<CreateRecordRequest, CreateRecordResponse>
{
    public override void Configure()
    {
        Post("/records");
        Group<RecordsGroup>();
        Policies(AuthorizationPolicies.CanModifyRecords);
        Summary(s =>
        {
            s.Summary = "Create medical record";
            s.Description = "Creates a new medical record for a patient. Can optionally include attachment references from previously uploaded files.";
            s.Responses[200] = "Record created successfully";
            s.Responses[400] = "Validation error";
            s.Responses[401] = "Unauthorized";
            s.Responses[403] = "Forbidden";
            s.Responses[404] = "Patient not found or attachment not found";
        });
    }

    public override async Task HandleAsync(CreateRecordRequest req, CancellationToken ct)
    {
        var practitionerId = userContext.UserId;

        // Verify patient exists
        var patientSpec = new PatientByIdSpecification(req.PatientId);
        var patient = await patientRepository.FirstOrDefaultAsync(patientSpec, ct);
        if (patient == null)
        {
            ThrowError("Patient not found", 404);
            return;
        }

        // Get practitioner user to populate Practitioner value object
        var practitionerUser = await identityService.GetUserByIdAsync(practitionerId, ct);
        if (practitionerUser == null)
        {
            ThrowError("Practitioner not found", 404);
            return;
        }

        // Create Practitioner value object from user
        var practitioner = Practitioner.Create(
            practitionerUser.FullName,
            practitionerUser.Email,
            practitionerUser.Role);

        // Create medical record
        var record = MedicalRecord.Create(
            req.PatientId,
            practitionerId,
            practitioner,
            req.RecordType,
            req.Title,
            req.Content);

        // Add attachments if provided
        if (req.AttachmentIds != null && req.AttachmentIds.Count > 0)
        {
            foreach (var attachmentId in req.AttachmentIds)
            {
                // Verify attachment exists by trying to download it
                var downloadResult = await fileStorageService.DownloadFileAsync(attachmentId, ct);
                if (downloadResult.IsFailure)
                {
                    ThrowError($"Attachment with ID {attachmentId} not found", 404);
                    return;
                }

                // Create attachment value object from file metadata
                var fileMetadata = downloadResult.Value!;
                var attachment = Attachment.Create(
                    attachmentId,
                    fileMetadata.FileName,
                    fileMetadata.FileSize,
                    fileMetadata.ContentType,
                    dateTimeProvider.Now);

                record.AddAttachment(practitionerId, attachment);
            }
        }

        await recordRepository.AddAsync(record, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateRecordResponse
        {
            Id = record.Id,
            PatientId = record.PatientId,
            PractitionerId = record.PractitionerId,
            Practitioner = new CreateRecordResponse.PractitionerDto
            {
                FullName = record.Practitioner.FullName,
                Email = record.Practitioner.Email,
                Role = record.Practitioner.Role
            },
            RecordType = record.RecordType,
            Title = record.Title,
            Content = record.Content,
            Attachments = record.Attachments.Select(a => new AttachmentDto
            {
                FileId = a.FileId,
                FileName = a.FileName,
                FileSize = a.FileSize,
                ContentType = a.ContentType,
                UploadedAt = a.UploadedAt
            }).ToList(),
            CreatedAt = record.CreatedAt
        }, ct);
    }
}
