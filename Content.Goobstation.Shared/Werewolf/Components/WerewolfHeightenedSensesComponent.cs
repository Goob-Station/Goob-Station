namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfHeightenedSensesComponent : Component
{
    [ViewVariables]
    public TimeSpan AccumulationTime = TimeSpan.Zero;

    /// <summary>
    /// The duration of the ability in seconds
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(25);

    /// <summary>
    /// Whether the ability has been activated
    /// </summary>
    [ViewVariables]
    public bool Activated;
}
