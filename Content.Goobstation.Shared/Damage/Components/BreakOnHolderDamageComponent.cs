using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Damage;

[RegisterComponent, NetworkedComponent]
public sealed partial class BreakOnHolderDamageComponent : Component
{
    /// <summary>
    /// Optional sound on brewak.
    /// </summary>
    [DataField]
    public SoundSpecifier? BreakSound;

    /// <summary>
    /// Self explanatory.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnOnBreak;
}
