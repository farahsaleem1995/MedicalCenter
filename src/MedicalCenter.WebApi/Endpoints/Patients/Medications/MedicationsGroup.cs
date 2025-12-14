using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Patients.Medications;

/// <summary>
/// OpenAPI group for patient medications endpoints.
/// </summary>
public class MedicationsGroup : Group
{
    public MedicationsGroup()
    {
        Configure("/patients/{patientId}/medications", ep =>
        {
            ep.DontAutoTag();
            ep.Description(d => d
                .WithTags("Medications")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}

