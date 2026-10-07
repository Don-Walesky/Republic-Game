namespace Republic.Core.Economy.Treasury;

/// <summary>
/// Domain model representing a sovereign nation's accumulated liquid REPU treasury balance.
/// Distinguishes accumulated monetary balance (stock) from periodic yield revenue (flow).
/// Prevents negative balances and unauthorized overdrafts.
/// </summary>
public sealed class RepuTreasury
{
    /// <summary>
    /// Currency symbol for REPU.
    /// </summary>
    public const string CurrencySymbol = "R";

    /// <summary>
    /// Currency code for REPU.
    /// </summary>
    public const string CurrencyCode = "REPU";

    private double _balance;

    /// <summary>
    /// Gets the unique identifier of the country that owns this treasury.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current liquid accumulated REPU balance.
    /// Default is 0.0 (the smallest sensible starting balance).
    /// </summary>
    public double Balance
    {
        get => _balance;
        init
        {
            if (value < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Treasury balance cannot be initialized to a negative amount.");
            }
            _balance = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepuTreasury"/> class with a default zero balance.
    /// </summary>
    public RepuTreasury()
    {
        _balance = 0.0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepuTreasury"/> class with an initial balance and optional country ID.
    /// </summary>
    /// <param name="initialBalance">The starting REPU balance. Must be non-negative.</param>
    /// <param name="countryId">The owning country identifier.</param>
    public RepuTreasury(double initialBalance, string countryId = "")
    {
        if (initialBalance < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial treasury balance cannot be negative.");
        }
        _balance = initialBalance;
        CountryId = countryId;
    }

    /// <summary>
    /// Adds REPU revenue to the treasury.
    /// </summary>
    /// <param name="amount">The non-negative revenue amount to deposit.</param>
    public void Deposit(double amount)
    {
        if (amount < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount cannot be negative.");
        }

        if (amount == 0.0)
        {
            return;
        }

        _balance += amount;
    }

    /// <summary>
    /// Withdraws or spends REPU from the treasury.
    /// Throws <see cref="InvalidOperationException"/> if the amount exceeds available funds.
    /// </summary>
    /// <param name="amount">The non-negative amount to spend.</param>
    public void Withdraw(double amount)
    {
        if (amount < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Withdrawal amount cannot be negative.");
        }

        if (amount > _balance)
        {
            throw new InvalidOperationException(
                $"Insufficient treasury funds. Requested: {Format(amount)}, Available: {Format(_balance)}. Deficit or debt mechanics are not permitted.");
        }

        _balance -= amount;
    }

    /// <summary>
    /// Attempts to spend or withdraw REPU from the treasury.
    /// Returns false if funds are insufficient or amount is negative, without modifying the balance.
    /// </summary>
    /// <param name="amount">The amount to spend.</param>
    /// <returns>True if the transaction succeeded; otherwise, false.</returns>
    public bool TryWithdraw(double amount)
    {
        if (amount < 0.0 || amount > _balance)
        {
            return false;
        }

        _balance -= amount;
        return true;
    }

    /// <summary>
    /// Formats the current balance using standard REPU notation (e.g., R0, R45M, R450M, R1.2B).
    /// </summary>
    public string FormattedBalance => Format(_balance);

    /// <summary>
    /// Formats an arbitrary amount into standard REPU currency string.
    /// Examples: R45M, R450M, R1.2B.
    /// </summary>
    public static string Format(double amount)
    {
        var abs = Math.Abs(amount);
        var sign = amount < 0 ? "-" : "";

        if (abs >= 1_000_000_000.0)
        {
            return $"{sign}{CurrencySymbol}{(abs / 1_000_000_000.0):0.#}B";
        }
        if (abs >= 1_000_000.0)
        {
            return $"{sign}{CurrencySymbol}{(abs / 1_000_000.0):0.#}M";
        }
        if (abs >= 1_000.0)
        {
            return $"{sign}{CurrencySymbol}{(abs / 1_000.0):0.#}K";
        }
        return $"{sign}{CurrencySymbol}{abs:0.#}";
    }

    /// <summary>
    /// Creates an independent deep clone of the treasury instance.
    /// </summary>
    public RepuTreasury Clone() => new()
    {
        CountryId = CountryId,
        Balance = _balance
    };

    public override string ToString() => FormattedBalance;
}
