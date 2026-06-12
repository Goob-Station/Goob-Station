using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// When stepped on, paralyzes the tripper and raises StepTrapTriggeredEvent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StepTrapComponent : Component
{
    /// <summary>
    /// How long the tripper is paralyzed.
    /// </summary>
    [DataField]
    public TimeSpan SnareTime = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier CaughtSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");

    /// <summary>
    /// Entities matching this whitelist are ignored and will not be paralyzed.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
