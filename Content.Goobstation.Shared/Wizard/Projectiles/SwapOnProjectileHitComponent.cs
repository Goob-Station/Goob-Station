using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

[RegisterComponent, NetworkedComponent]
public sealed partial class SwapOnProjectileHitComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public EntProtoId Effect = "SwapSpellEffect";

    [DataField]
    public EntityWhitelist Whitelist;

    [DataField]
    public bool DeleteProjectileOnSwap;
}
