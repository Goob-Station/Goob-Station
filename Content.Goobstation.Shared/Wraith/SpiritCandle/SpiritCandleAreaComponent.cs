using Content.Shared.Eye;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.SpiritCandle;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpiritCandleAreaComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist;

    [ViewVariables]
    public HashSet<EntityUid?> EntitiesInside;
}

[ByRefEvent]
public record struct AttemptCollideSpiritCandleEvent(bool Cancelled = false);
