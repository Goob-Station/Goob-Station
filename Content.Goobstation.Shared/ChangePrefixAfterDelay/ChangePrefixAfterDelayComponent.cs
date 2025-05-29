namespace Content.Goobstation.Shared.ChangePrefixAfterDelay;

/// <summary>
/// Changes held and equipped prefix of an item after the delay, then removes itself.
/// </summary>
[RegisterComponent]
public sealed partial class ChangePrefixAfterDelayComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? ChangeAt;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.3);

    [DataField]
    public string? NewHeldPrefix;

    [DataField]
    public string? NewEquippedPrefix;
}
