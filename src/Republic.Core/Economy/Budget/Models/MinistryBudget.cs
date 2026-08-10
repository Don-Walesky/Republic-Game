namespace Republic.Core.Economy.Budget.Models;

/// <summary>
/// Domain model detailing national budget allocations across government ministries.
/// </summary>
public sealed class MinistryBudget
{
    public double DefenseAllocation { get; set; } = 500_000_000.0;
    public double HealthcareAllocation { get; set; } = 400_000_000.0;
    public double EducationAllocation { get; set; } = 350_000_000.0;
    public double InfrastructureAllocation { get; set; } = 600_000_000.0;
    public double ScienceAllocation { get; set; } = 250_000_000.0;

    public double TotalQuarterlyExpenditures => DefenseAllocation + HealthcareAllocation + EducationAllocation + InfrastructureAllocation + ScienceAllocation;
}
