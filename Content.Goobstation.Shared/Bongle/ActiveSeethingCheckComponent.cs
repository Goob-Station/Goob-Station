namespace Content.Goobstation.Shared.Bingle;

/// <summary>
/// Marks entity as currently looking for targets to seethe over.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class ActiveSeethingCheckComponent : Component
{
    [DataField, AutoPausedField]
    public TimeSpan NextCheck;
}
