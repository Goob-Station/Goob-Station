using Robust.Shared.GameStates;

namespace Content.Shared.WhiteDream.BloodCult.Items;

[RegisterComponent, NetworkedComponent]
public sealed partial class CultItemComponent : Component
{
    /// <summary>
    ///     Allow non-cultists to use this item?
    /// </summary>
    [DataField]
    public bool AllowUseToEveryone;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);
}
