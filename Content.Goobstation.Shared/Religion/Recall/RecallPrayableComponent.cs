namespace Content.Goobstation.Shared.Religion.Recall;

[RegisterComponent]
public sealed partial class RecallPrayableComponent : Component
{
    /// <summary>
    /// How long does the recall do-after take to complete.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);
}
