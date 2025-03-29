using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Goobstation.Server.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(PuddleMetricSystem))]
public sealed partial class PuddleMetricComponent : Component
{
    // Impact Constants
    private const float MinimalImpact = 0.02f;
    private const float MinorImpact = 0.1f;
    private const float ModerateImpact = 0.2f;
    private const float MajorImpact = 0.3f;

    /// <summary>
    ///   The cost of each puddle, per mL. Note about 200 mL is one puddle.
    ///   Example: A water puddle of 200mL would contribute (200 * 0.02) = 4 chaos points.
    /// </summary>
    [DataField("puddles", customTypeSerializer: typeof(DictionarySerializer<string, FixedPoint2>))]
    public Dictionary<string, FixedPoint2> Puddles =
        new()
        {
            { "Water", MinimalImpact },
            { "SpaceCleaner", MinimalImpact },

            { "Nutriment", MinorImpact },
            { "Sugar", MinorImpact },
            { "Ephedrine", MinorImpact },
            { "Ale", MinorImpact },
            { "Beer", ModerateImpact },

            { "Slime", ModerateImpact },
            { "Blood", ModerateImpact },
            { "CopperBlood", ModerateImpact },
            { "ZombieBlood", ModerateImpact },
            { "AmmoniaBlood", ModerateImpact },
            { "ChangelingBlood", ModerateImpact },
            { "SpaceDrugs", MajorImpact },
            { "SpaceLube", MajorImpact },
            { "SpaceGlue", MajorImpact },
        };

    [DataField("puddleDefault")]
    public FixedPoint2 PuddleDefault = 0.1f;

}
