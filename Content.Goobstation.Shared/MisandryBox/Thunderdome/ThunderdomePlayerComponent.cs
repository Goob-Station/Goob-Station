using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[RegisterComponent]
public sealed partial class ThunderdomePlayerComponent : Component
{
    [DataField]
    public EntityUid? OriginalBody;

    [DataField]
    public EntityUid? RuleEntity;

    [DataField]
    public int Kills;

    [DataField]
    public int Deaths;

    [DataField]
    public int CurrentStreak;

    /// <summary>
    /// Respawn Penalty to inflict for certain actions, i.e. ghosting or suiciding.
    /// </summary>
    [DataField]
    public float TimePenalty;

    /// <summary>
    /// How long until player can respawn.
    /// </summary>
    [DataField]
    public TimeSpan RespawnTimer;

    /// <summary>
    /// The selected weapon index for respawning with the same gear.
    /// </summary>
    [DataField]
    public int WeaponSelection;
}
