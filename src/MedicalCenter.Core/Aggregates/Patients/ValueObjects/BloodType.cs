using Ardalis.GuardClauses;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.Aggregates.Patients.Enums;

namespace MedicalCenter.Core.Aggregates.Patients.ValueObjects;

/// <summary>
/// Blood type value object representing ABO and Rh factor.
/// Belongs to the Patient aggregate.
/// </summary>
public class BloodType : ValueObject
{
    public BloodABO ABO { get; }
    public BloodRh Rh { get; }

    private BloodType() { } // EF Core

    public BloodType(BloodABO abo, BloodRh rh)
    {
        ABO = abo;
        Rh = rh;
    }

    public static BloodType Create(BloodABO abo, BloodRh rh)
    {
        Guard.Against.OutOfRange((int)abo, nameof(abo), 1, Enum.GetValues<BloodABO>().Length);
        Guard.Against.OutOfRange((int)rh, nameof(rh), 1, Enum.GetValues<BloodRh>().Length);
        
        return new BloodType(abo, rh);
    }

    public override string ToString()
    {
        return $"{ABO}{(Rh == BloodRh.Positive ? "+" : "-")}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ABO;
        yield return Rh;
    }
}

