using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Voidwalker.CosmicSkull;

[RegisterComponent]
public sealed partial class CosmicSkullComponent : Component
{
    [DataField]
    public int Uses = 1;

    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public ProtoId<DamageModifierSetPrototype> GlassModifierSet = "Glass";
}
