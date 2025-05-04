using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpyUplinkComponent : Component
{
    /// <summary>
    /// The currently claimed bounty.
    /// </summary>
    public SpyBountyData? ClaimedBounty = null;

    /// <summary>
    /// The sound played when beginning to steal an object.
    /// </summary>
    public SoundSpecifier StealSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/wewewew.ogg");

    /// <summary>
    /// The sound played when successfully stealing an object.
    /// </summary>
    public SoundSpecifier StealSuccessSound = new SoundPathSpecifier("/Audio/Effects/kaching.ogg");

    /// <summary>
    /// The duration of the stealing do-after.
    /// </summary>
    public TimeSpan StealTime = TimeSpan.FromSeconds(5);
}
