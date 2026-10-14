namespace Republic.Core.World;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Domain model representing a sovereign nation's physical stockpile of economic resources and goods.
/// Distinguishes accumulated physical goods (stock) from periodic yield flows.
/// Prevents negative inventory balances and unauthorized deficits.
/// </summary>
public sealed class ResourceInventory
{
    private readonly Dictionary<string, double> _stockpiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    /// <summary>
    /// Gets a read-only snapshot of all currently held resource stockpiles by type and unit quantity.
    /// </summary>
    public IReadOnlyDictionary<string, double> Stockpiles
    {
        get
        {
            lock (_lock)
            {
                return _stockpiles.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>
    /// Initializes a new, empty <see cref="ResourceInventory"/> instance.
    /// </summary>
    public ResourceInventory()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ResourceInventory"/> instance seeded with existing resource stockpiles.
    /// </summary>
    /// <param name="initialStockpiles">Initial resource types and quantities.</param>
    public ResourceInventory(IEnumerable<KeyValuePair<string, double>> initialStockpiles)
    {
        ArgumentNullException.ThrowIfNull(initialStockpiles);

        foreach (var (resourceType, quantity) in initialStockpiles)
        {
            if (!string.IsNullOrWhiteSpace(resourceType) && double.IsFinite(quantity) && quantity > 0.0)
            {
                _stockpiles[resourceType.Trim()] = quantity;
            }
        }
    }

    /// <summary>
    /// Retrieves the current available quantity for the specified resource type.
    /// Returns 0.0 if the nation holds no stockpile of this resource.
    /// </summary>
    public double GetQuantity(string resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
        {
            return 0.0;
        }

        lock (_lock)
        {
            return _stockpiles.TryGetValue(resourceType.Trim(), out var quantity) ? quantity : 0.0;
        }
    }

    /// <summary>
    /// Determines whether the inventory has at least the specified quantity of the resource.
    /// </summary>
    public bool HasSufficient(string resourceType, double quantity)
    {
        if (string.IsNullOrWhiteSpace(resourceType) || !double.IsFinite(quantity) || quantity < 0.0)
        {
            return false;
        }

        if (quantity == 0.0)
        {
            return true;
        }

        return GetQuantity(resourceType) >= quantity;
    }

    /// <summary>
    /// Adds the specified quantity of physical goods to the nation's stockpile.
    /// </summary>
    public void Add(string resourceType, double quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        if (!double.IsFinite(quantity) || quantity < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to add must be a finite non-negative number.");
        }

        if (quantity == 0.0)
        {
            return;
        }

        var key = resourceType.Trim();
        lock (_lock)
        {
            var current = _stockpiles.TryGetValue(key, out var q) ? q : 0.0;
            _stockpiles[key] = current + quantity;
        }
    }

    /// <summary>
    /// Removes the specified quantity of physical goods from the nation's stockpile.
    /// Throws <see cref="InvalidOperationException"/> if available quantity is insufficient.
    /// </summary>
    public void Remove(string resourceType, double quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        if (!double.IsFinite(quantity) || quantity < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to remove must be a finite non-negative number.");
        }

        if (quantity == 0.0)
        {
            return;
        }

        var key = resourceType.Trim();
        lock (_lock)
        {
            var current = _stockpiles.TryGetValue(key, out var q) ? q : 0.0;
            if (current < quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient resource '{key}'. Requested: {quantity:N0}, Available: {current:N0}. Physical goods deficits are prohibited.");
            }

            _stockpiles[key] = current - quantity;
        }
    }

    /// <summary>
    /// Attempts to remove the specified quantity of physical goods from the nation's stockpile.
    /// Returns false without modifying the stockpile if available quantity is insufficient.
    /// </summary>
    public bool TryRemove(string resourceType, double quantity)
    {
        if (string.IsNullOrWhiteSpace(resourceType) || !double.IsFinite(quantity) || quantity < 0.0)
        {
            return false;
        }

        if (quantity == 0.0)
        {
            return true;
        }

        var key = resourceType.Trim();
        lock (_lock)
        {
            var current = _stockpiles.TryGetValue(key, out var q) ? q : 0.0;
            if (current < quantity)
            {
                return false;
            }

            _stockpiles[key] = current - quantity;
            return true;
        }
    }

    /// <summary>
    /// Creates an independent deep clone of this resource inventory.
    /// </summary>
    public ResourceInventory Clone()
    {
        lock (_lock)
        {
            return new ResourceInventory(_stockpiles);
        }
    }

    /// <summary>
    /// Formats the stockpiles as a human-readable list.
    /// </summary>
    public string FormattedStockpiles
    {
        get
        {
            lock (_lock)
            {
                var nonZero = _stockpiles.Where(kv => kv.Value > 0.0).ToList();
                if (nonZero.Count == 0)
                {
                    return "None";
                }

                return string.Join(", ", nonZero.Select(kv => $"{kv.Key}: {kv.Value:N0} units"));
            }
        }
    }

    public override string ToString() => FormattedStockpiles;
}
