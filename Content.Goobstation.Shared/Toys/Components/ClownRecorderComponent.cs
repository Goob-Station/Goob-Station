using Content.Goobstation.Shared.Toys.Systems;

namespace Content.Goobstation.Shared.Toys.Components;

/// <summary>
/// See <see cref="ClownRecorderSystem"/>
/// </summary>
[RegisterComponent]
public sealed partial class ClownRecorderComponent : Component
{
    /// <summary>
    /// The normal timer
    /// </summary>
    [DataField]
    public TimeSpan NormalDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long the emagged timer will be
    /// </summary>
    [DataField]
    public TimeSpan EmaggedDelay = TimeSpan.FromSeconds(5);
}
