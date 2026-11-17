namespace Content.Goobstation.Shared.MartialArts.Specific.Components;

[RegisterComponent]
[AutoGenerateComponentState]
public sealed partial class SleepingCarpGnasherComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public int ConsecutiveGnashes = 0;

    [DataField]
    public TimeSpan ResetDelay = TimeSpan.FromSeconds(5);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan ResetTime = TimeSpan.Zero;
};
