namespace MedicalCenter.Core.Queries;

/// <summary>
/// Available sort fields for listing users.
/// </summary>
public enum ListUsersSortBy
{
    FullName = 0,
    Email = 1,
    Role = 2,
    CreatedAt = 3,
    NationalId = 4
}
