namespace Republic.Core.Economy.Budget.Models;

/// <summary>
/// Domain model detailing national budget allocations across government ministries,
/// ongoing public payroll obligations, and recurrent governance expenditures.
/// </summary>
public sealed class MinistryBudget
{
    public double DefenseAllocation { get; set; } = 500_000_000.0;
    public double HealthcareAllocation { get; set; } = 400_000_000.0;
    public double EducationAllocation { get; set; } = 350_000_000.0;
    public double InfrastructureAllocation { get; set; } = 600_000_000.0;
    public double ScienceAllocation { get; set; } = 250_000_000.0;

    /// <summary>
    /// Ongoing recurrent public payroll expenses (civil service, law enforcement, education, healthcare workers).
    /// </summary>
    public double CivilServiceSalariesAllocation { get; set; } = 85_000_000.0;

    /// <summary>
    /// Cost of ongoing governance operations (executive administration, judicial branches, parliament sessions, state regulatory agencies).
    /// </summary>
    public double GovernanceOperationsAllocation { get; set; } = 140_000_000.0;

    /// <summary>
    /// Recurrent infrastructure operations, utility grid maintenance, and public facility servicing costs.
    /// </summary>
    public double InfrastructureMaintenanceAllocation { get; set; } = 180_000_000.0;

    /// <summary>
    /// Total recurrent in-country operational, salary, and maintenance overhead per fiscal quarter.
    /// </summary>
    public double TotalOngoingGovernanceExpenses =>
        CivilServiceSalariesAllocation + GovernanceOperationsAllocation + InfrastructureMaintenanceAllocation;

    /// <summary>
    /// Total sovereign expenditures across all sector ministries and ongoing operational commitments per fiscal quarter.
    /// </summary>
    public double TotalQuarterlyExpenditures =>
        DefenseAllocation + HealthcareAllocation + EducationAllocation + InfrastructureAllocation + ScienceAllocation + TotalOngoingGovernanceExpenses;
}
