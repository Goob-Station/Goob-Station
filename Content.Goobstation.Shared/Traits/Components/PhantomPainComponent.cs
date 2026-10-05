using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Traits.Components;

/// <summary>
/// Makes whoever has this whine in looc that they have phantom pain.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PhantomPainComponent : Component
{
    [DataField]
    public ProtoId<LocalizedDatasetPrototype> LineDataset = "PhantomPainLineDataset";

    [DataField]
    public FixedPoint2 DamageThreshold = FixedPoint2.New(3);

    [DataField]
    public HashSet<ProtoId<DamageGroupPrototype>>? ValidDamageGroups = new()
    {
        "Brute",
        "Burn",
    };

    /// <summary>
    /// Whether the damage has to have come from another entity.
    /// </summary>
    [DataField]
    public bool RequireOrigin = true;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan? NextAllowedTime;
}
