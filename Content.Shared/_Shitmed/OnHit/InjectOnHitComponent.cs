using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;

namespace Content.Shared._Shitmed.OnHit;

[RegisterComponent]
public sealed partial class InjectOnHitComponent : Component
{
    [DataField("reagents")]
    public List<ReagentQuantity> Reagents;

    [DataField("sound")]
    public SoundSpecifier? Sound;
}
[ByRefEvent]
public record struct InjectOnHitAttemptEvent(bool Cancelled);
