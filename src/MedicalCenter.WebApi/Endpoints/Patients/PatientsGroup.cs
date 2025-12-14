using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Patients;

/// <summary>
/// OpenAPI group for patient endpoints.
/// </summary>
public class PatientsGroup : Group
{
    public PatientsGroup()
    {
        Configure("patients", ep =>
        {
            ep.Description(d => d
                .WithTags("Patients")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}

