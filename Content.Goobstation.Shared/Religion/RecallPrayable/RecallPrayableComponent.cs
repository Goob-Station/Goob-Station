using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Religion.RecallPrayable;

[RegisterComponent]
public sealed partial class RecallPrayableComponent : Component
{
    /// <summary>
    /// How long does the recall do-after take to complete.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);
}
