using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Components;

/// <summary>
///     Keeps track of entities bought from stores for refunds, especially useful if entities get deleted before they can be refunded.
/// </summary>
[RegisterComponent, Access(typeof(StoreSystem))]
public sealed partial class StoreRefundComponent : Component
{
    [ViewVariables, DataField]
    public EntityUid? StoreEntity;

    // Goobstation start
    [ViewVariables, DataField]
    public ListingData? Data;

    [ViewVariables, DataField]
    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> BalanceSpent = new();
    // Goobstation end
}
