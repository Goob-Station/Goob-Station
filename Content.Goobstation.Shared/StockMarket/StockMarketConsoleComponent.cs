using Content.Shared.Cargo.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StockMarket;

/// <summary>
/// Marker component for a stock market console entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StockMarketConsoleComponent : Component
{
    /// <summary>
    /// Which cargo account this console operates with for buy/sell.
    /// </summary>
    [DataField]
    public ProtoId<CargoAccountPrototype> Account { get; set; } = "Cargo";
}