using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.EntityEffects.Effects;
using Content.Shared._Shitmed.Targeting;

namespace Content.Shared.EntityEffects.Effects.Damage;

public sealed partial class HealthChange
{
    [DataField]
    public SplitDamageBehavior SplitDamage = SplitDamageBehavior.SplitEnsureAllOrganic;

    [DataField]
    public bool UseTargeting = true;

    [DataField]
    public TargetBodyPart TargetPart = TargetBodyPart.All;

    [DataField]
    public bool IgnoreBlockers = false;

    [DataField]
    public TemperatureScaling? ScaleByTemperature;
}
