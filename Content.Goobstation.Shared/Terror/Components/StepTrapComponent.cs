using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Generic step trap. Paralyzes any entity that steps on this (unless they have
/// <see cref="IgnoreSpiderWebComponent"/> or similar exemptions) and raises
/// <see cref="StepTrapTriggeredEvent"/> for other systems to layer effects onto.
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
}
