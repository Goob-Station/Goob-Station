using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    // Todo: we can remove this once entity queries can be declared with "default"!
    public void InitializeWoundmed()
    {
        _bodyQuery = GetEntityQuery<BodyComponent>();
        _consciousnessQuery = GetEntityQuery<ConsciousnessComponent>();
        _woundableQuery = GetEntityQuery<WoundableComponent>();
        _bodyPartQuery = GetEntityQuery<BodyPartComponent>();
    }
}

public sealed partial class DamageChangedEvent
{
    /// <summary>
    ///     [Woundmed]
    ///     Whether or not the damage change should be blocked due to traumas or wounds
    /// </summary>
    public readonly bool IgnoreBlockers;

    /// <summary>
    ///     [Woundmed]
    ///     Damage before clamp of excessive heal and damage cap was applied
    /// </summary>
    public readonly DamageSpecifier? UncappedDamage;
}

public sealed partial class DamageModifyEvent
{
    /// <summary>
    /// [Woundmed]
    /// </summary>
    public readonly EntityUid Target = target;
    /// <summary>
    /// [Woundmed]
    /// </summary>
    public readonly TargetBodyPart? TargetPart = targetPart;
}