using Content.Shared._DV.Salvage.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Salvage.Components;

/// <summary>
/// Stores mining points for a holder, such as an ID card or ore processor.
/// Mining points are gained by smelting ore and redeeming them to your ID card.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(MiningPointsSystem))]
[AutoGenerateComponentState]
public sealed partial class MiningPointsComponent : Component
{
    /// <summary>
    /// The number of points stored.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint Points;
}
