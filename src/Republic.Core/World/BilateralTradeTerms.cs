namespace Republic.Core.World;

using System;
using Republic.Core.Economy.Treasury;

/// <summary>
/// Domain model specifying the explicit economic terms of a bilateral trade transaction between two sovereign nations.
/// Defines the participating buyer and seller, the commodity traded, the unit quantity, and the agreed REPU price.
/// </summary>
public sealed record BilateralTradeTerms
{
    /// <summary>
    /// Gets the unique identifier of the sovereign country supplying physical goods and receiving REPU payment.
    /// </summary>
    public string SellerCountryId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the sovereign country paying REPU and receiving physical goods.
    /// </summary>
    public string BuyerCountryId { get; init; }

    /// <summary>
    /// Gets the commodity or resource type being traded (e.g. Iron, Timber, Petroleum).
    /// </summary>
    public string ResourceType { get; init; }

    /// <summary>
    /// Gets the physical unit quantity of goods being transferred. Must be strictly positive.
    /// </summary>
    public double Quantity { get; init; }

    /// <summary>
    /// Gets the total agreed payment in REPU transferred from buyer to seller. Must be strictly positive.
    /// </summary>
    public double Price { get; init; }

    /// <summary>
    /// Formatted REPU price string (e.g. R500K, R1.5M).
    /// </summary>
    public string FormattedPrice => RepuTreasury.Format(Price);

    /// <summary>
    /// Initializes a new instance of the <see cref="BilateralTradeTerms"/> record.
    /// </summary>
    public BilateralTradeTerms(
        string sellerCountryId,
        string buyerCountryId,
        string resourceType,
        double quantity,
        double price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sellerCountryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(buyerCountryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);

        if (string.Equals(sellerCountryId.Trim(), buyerCountryId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Seller and buyer cannot be the same sovereign country. Internal self-trade is prohibited.", nameof(buyerCountryId));
        }

        if (!double.IsFinite(quantity) || quantity <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Traded quantity must be a finite number strictly greater than zero.");
        }

        if (!double.IsFinite(price) || price <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Agreed trade price must be a finite number strictly greater than zero.");
        }

        SellerCountryId = sellerCountryId.Trim();
        BuyerCountryId = buyerCountryId.Trim();
        ResourceType = resourceType.Trim();
        Quantity = quantity;
        Price = price;
    }

    /// <summary>
    /// Checks whether the specified country is a party to this trade deal (either as seller or buyer).
    /// </summary>
    public bool Involves(string countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return false;
        }

        var trimmed = countryId.Trim();
        return string.Equals(SellerCountryId, trimmed, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(BuyerCountryId, trimmed, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Formats the trade terms for display and logging.
    /// </summary>
    public override string ToString() =>
        $"{Quantity:N0} units of {ResourceType} for {FormattedPrice} (Seller: {SellerCountryId}, Buyer: {BuyerCountryId})";
}
