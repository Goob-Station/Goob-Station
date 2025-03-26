namespace Content.Server.Heretic.Components;

[RegisterComponent]
public sealed partial class ChangeUseDelayOnAscensionComponent : Component
{
    [DataField(required: true)]
    public TimeSpan NewUseDelay;

    [DataField]
    public string? RequiredPath;
}
