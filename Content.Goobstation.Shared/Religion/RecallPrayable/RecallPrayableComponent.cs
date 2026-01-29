using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Religion.RecallPrayable;

[RegisterComponent]
public sealed partial class RecallPrayableComponent : Component
{
    /// <summary>
    /// Used for verb text.
    /// </summary>
    [DataField]
    public string Verb = "chaplain-recall-verb";

    /// <summary>
    /// How long does the recall do-after take to complete.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);
}
