using Content.Server.Body.Components;
using Content.Shared.Body.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.Traits.Vampirism.Components;

/// <summary>
///     Enables the mob to suck blood from other mobs to replenish its own saturation.
///     Must be fully initialized before being added to a mob.
/// </summary>
[RegisterComponent]
public sealed partial class VampirismComponent : Component
{
    [DataField]
    public HashSet<ProtoId<MetabolizerTypePrototype>> MetabolizerPrototypes = new() { "Vampiric", "Animal" };

    [DataField]
    public TimeSpan SuccDelay = TimeSpan.FromSeconds(1);
}
