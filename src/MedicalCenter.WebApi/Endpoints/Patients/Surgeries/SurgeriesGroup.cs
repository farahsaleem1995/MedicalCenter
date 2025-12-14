using FastEndpoints;

namespace MedicalCenter.WebApi.Endpoints.Patients.Surgeries;

/// <summary>
/// OpenAPI group for patient surgeries endpoints.
/// </summary>
public class SurgeriesGroup : Group
{
    public SurgeriesGroup()
    {
        Configure("/patients/{patientId}/surgeries", ep =>
        {
            ep.DontAutoTag();
            ep.Description(d => d
                .WithTags("Surgeries")
                .Produces(401)
                .Produces(403)
                .Produces(404));
        });
    }
}

