using Content.Shared._DV.Salvage.Systems;
using Content.Shared.Thief;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Shared._DV.Salvage.Components;

/// </summary>
///     A vendor that sells mining equipment. Also holds a radial menu to redeem vouchers with a preset list of items.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(MiningVoucherSystem))]
public sealed partial class MiningVendorComponent : Component
{
    /// <summary>
    /// The kits that can be selected.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<ThiefBackpackSetPrototype>> Kits = new();
}

