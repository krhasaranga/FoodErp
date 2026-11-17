using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// Defines how to convert between two units.
/// e.g. 1 Box = 12 Pieces → FromUnitId=Box, ToUnitId=Piece, Factor=12
/// </summary>
public class UnitConversion : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid FromUnitId { get; private set; }
    public Unit? FromUnit { get; private set; }
    public Guid ToUnitId { get; private set; }
    public Unit? ToUnit { get; private set; }
    public decimal Factor { get; private set; }   // 1 FromUnit = Factor ToUnits

    private UnitConversion() { }

    public static UnitConversion Create(Guid fromUnitId, Guid toUnitId, decimal factor, Guid tenantId)
    {
        if (factor <= 0)
            throw new ArgumentException("Conversion factor must be greater than zero.", nameof(factor));
        if (fromUnitId == toUnitId)
            throw new ArgumentException("From and To units cannot be the same.");

        return new UnitConversion
        {
            FromUnitId = fromUnitId,
            ToUnitId = toUnitId,
            Factor = factor,
            TenantId = tenantId
        };
    }
}