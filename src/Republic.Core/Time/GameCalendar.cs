namespace Republic.Core.Time;

/// <summary>
/// Mutable calendar helper used by the time system.
/// </summary>
public sealed class GameCalendar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameCalendar"/> class.
    /// </summary>
    public GameCalendar(int daysPerMonth, int monthsPerYear)
    {
        DaysPerMonth = daysPerMonth;
        MonthsPerYear = monthsPerYear;
        CurrentDate = new GameDate(1, 1, 1);
    }

    /// <summary>
    /// Gets the current date.
    /// </summary>
    public GameDate CurrentDate { get; private set; }

    /// <summary>
    /// Gets the configured days per month.
    /// </summary>
    public int DaysPerMonth { get; }

    /// <summary>
    /// Gets the configured months per year.
    /// </summary>
    public int MonthsPerYear { get; }

    /// <summary>
    /// Advances the calendar by one day.
    /// </summary>
    public CalendarAdvanceResult AdvanceDay()
    {
        var day = CurrentDate.Day + 1;
        var month = CurrentDate.Month;
        var year = CurrentDate.Year;
        var monthChanged = false;
        var yearChanged = false;

        if (day > DaysPerMonth)
        {
            day = 1;
            month++;
            monthChanged = true;
        }

        if (month > MonthsPerYear)
        {
            month = 1;
            year++;
            yearChanged = true;
        }

        CurrentDate = new GameDate(day, month, year);
        return new CalendarAdvanceResult(CurrentDate, true, monthChanged, yearChanged);
    }

    /// <summary>
    /// Restores the current date.
    /// </summary>
    public void Restore(GameDate date) => CurrentDate = date ?? throw new ArgumentNullException(nameof(date));
}

/// <summary>
/// Result of a day advancement operation.
/// </summary>
public sealed record CalendarAdvanceResult(GameDate Date, bool DayChanged, bool MonthChanged, bool YearChanged);
