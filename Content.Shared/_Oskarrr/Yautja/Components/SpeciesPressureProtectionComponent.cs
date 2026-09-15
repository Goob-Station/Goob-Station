using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.Yautja.Components;

/// <summary>
/// PressureProtection на цьому одязі діє лише для вказаних рас.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeciesPressureProtectionComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<SpeciesPrototype>> Species = new();
}
