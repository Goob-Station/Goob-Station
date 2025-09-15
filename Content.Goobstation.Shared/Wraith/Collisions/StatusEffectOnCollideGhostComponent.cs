using Content.Shared.StatusEffect;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Collisions;

[RegisterComponent, NetworkedComponent]
public sealed partial class StatusEffectOnCollideGhostComponent : Component
{
    [DataField]
    public ProtoId<StatusEffectPrototype> StatusEffect = "Corporeal";

    [DataField]
    public string Component = "Corporeal";

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public TimeSpan Duration;

    [DataField]
    public bool Refresh = true;

    [ViewVariables]
    public string FixtureId = "statusEffectCollision";
}
