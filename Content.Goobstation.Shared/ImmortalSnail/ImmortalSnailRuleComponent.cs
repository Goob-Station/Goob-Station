using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.ImmortalSnail;

/// <summary>
/// Component for the immortal snail game rule.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ImmortalSnailRuleComponent : Component
{
    /// <summary>
    /// The snails target.
    /// </summary>
    [DataField]
    public EntityUid? TargetEntity;

    /// <summary>
    /// The snail entity.
    /// </summary>
    [DataField]
    public EntityUid? SnailEntity;

    /// <summary>
    /// Announcement sound.
    /// </summary>
    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/_Goobstation/Music/immortal_snail_beat.ogg");

    /// <summary>
    /// Sound to play when the snail dies or the target dies.
    /// </summary>
    [DataField]
    public SoundSpecifier HonkSound = new SoundPathSpecifier("/Audio/Items/bikehorn.ogg");

    /// <summary>
    /// Whether to play announcements for this game rule.
    /// </summary>
    [DataField]
    public bool PlayAnnouncements = true;
}
