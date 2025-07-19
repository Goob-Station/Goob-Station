using Content.Pirate.Server.StationEvents.Events;

namespace Content.Pirate.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericFryRule))]
public sealed partial class NoosphericFryRuleComponent : Component
{
    [DataField("fryHeadgearMinorThreshold")]
    public float FryHeadgearMinorThreshold = 750f;

    [DataField("fryHeadgearMajorThreshold")]
    public float FryHeadgearMajorThreshold = 900f;
}
