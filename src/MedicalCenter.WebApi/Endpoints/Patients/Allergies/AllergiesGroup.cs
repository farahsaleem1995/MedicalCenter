using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Patients.Allergies;

/// <summary>
/// OpenAPI group for patient allergies endpoints.
/// </summary>
public class AllergiesGroup : Group
{
    public AllergiesGroup()
    {
        Configure("/patients/{patientId}/allergies", ep =>
        {
            ep.DontAutoTag();
            ep.Description(d => d
                .WithTags("Allergies")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}

