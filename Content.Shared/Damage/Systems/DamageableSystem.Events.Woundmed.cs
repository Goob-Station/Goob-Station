using Content.Goobstation.Common.CCVar;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    private void WoundmedInitialize()
    {
        Subs.CVar(_config, GoobCVars.ExplosionVitalMultiplier, value => ExplosionVitalMultiplier = value, true); // Woundmed
        Subs.CVar(_config, GoobCVars.ExplosionNonVitalMultiplier, value => ExplosionNonVitalMultiplier = value, true); // Woundmed
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
