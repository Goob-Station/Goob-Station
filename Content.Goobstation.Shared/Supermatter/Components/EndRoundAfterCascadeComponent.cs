namespace Content.Goobstation.Shared.Supermatter.Components;

/// <summary>
/// Ends the round after a defined amount of time
/// </summary>
[RegisterComponent]
public sealed partial class EndRoundAfterCascadeComponent : Component
{
    [DataField] public TimeSpan Delay =  TimeSpan.FromSeconds(720);
}
