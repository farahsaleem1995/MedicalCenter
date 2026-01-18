using Bogus;

namespace MedicalCenter.Infrastructure.Data.Seeders.Runtime.Fakers;

/// <summary>
/// Bogus faker configurations for medical attributes (Allergies, ChronicDiseases, Medications, Surgeries).
/// </summary>
public static class MedicalAttributeFaker
{
    private static readonly string[] CommonAllergies = new[]
    {
        "Peanuts", "Penicillin", "Latex", "Shellfish", "Eggs", "Milk", "Soy",
        "Wheat", "Tree Nuts", "Fish", "Sulfa Drugs", "Aspirin", "Ibuprofen",
        "Codeine", "Dust Mites", "Pollen", "Pet Dander"
    };

    private static readonly string[] AllergySeverities = new[]
    {
        "Mild", "Moderate", "Severe", "Life-threatening"
    };

    private static readonly string[] ChronicDiseases = new[]
    {
        "Type 2 Diabetes", "Hypertension", "Asthma", "Arthritis", "Chronic Obstructive Pulmonary Disease",
        "Heart Disease", "Kidney Disease", "Liver Disease", "Osteoporosis", "Depression",
        "Anxiety Disorder", "Migraine", "Epilepsy", "Hypothyroidism", "Hyperthyroidism"
    };

    private static readonly string[] Medications = new[]
    {
        "Metformin", "Lisinopril", "Atorvastatin", "Amlodipine", "Metoprolol",
        "Omeprazole", "Losartan", "Albuterol", "Gabapentin", "Sertraline",
        "Levothyroxine", "Furosemide", "Hydrochlorothiazide", "Warfarin", "Aspirin"
    };

    private static readonly string[] Dosages = new[]
    {
        "10mg daily", "5mg twice daily", "20mg once daily", "50mg as needed",
        "25mg daily", "100mg twice daily", "500mg three times daily", "2.5mg daily"
    };

    private static readonly string[] Surgeries = new[]
    {
        "Appendectomy", "Knee Replacement", "Cataract Surgery", "Gallbladder Removal",
        "Hernia Repair", "Tonsillectomy", "Hip Replacement", "Coronary Bypass",
        "Cholecystectomy", "Hysterectomy", "Prostatectomy", "Mastectomy"
    };

    public static string GetRandomAllergy(Faker faker) => faker.PickRandom(CommonAllergies);
    
    public static string? GetRandomAllergySeverity(Faker faker) => faker.Random.Bool(0.7f) ? faker.PickRandom(AllergySeverities) : null;
    
    public static string GetRandomChronicDisease(Faker faker) => faker.PickRandom(ChronicDiseases);
    
    public static string GetRandomMedication(Faker faker) => faker.PickRandom(Medications);
    
    public static string? GetRandomDosage(Faker faker) => faker.Random.Bool(0.8f) ? faker.PickRandom(Dosages) : null;
    
    public static string GetRandomSurgery(Faker faker) => faker.PickRandom(Surgeries);
    
    public static string? GetRandomSurgeonName(Faker faker) => faker.Random.Bool(0.6f) ? $"Dr. {faker.Name.FullName()}" : null;
}

