using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Server.AlertLevel;

/// <summary>
/// Goobstation.
/// Tracks whether the amber alert level is unlocked for a station.
/// </summary>
[RegisterComponent]
public sealed partial class AmberAlertComponent : Component
{
    [DataField]
    public bool Unlocked;

    /// <summary>
    /// The first ID card (with Captain or Head of Security access) swiped.
    /// </summary>
    [ViewVariables]
    public EntityUid? PendingCard;

    /// <summary>
    /// When the pending first authorization expires if a second swipe isn't made.
    /// </summary>
    [ViewVariables]
    public TimeSpan? PendingExpiry;

    /// <summary>
    /// The alert level this component gates.
    /// </summary>
    [DataField]
    public string AmberLevel = "amber";

    /// <summary>
    /// How long a first authorization is held while waiting for a second command member.
    /// </summary>
    [DataField]
    public TimeSpan PendingTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Any one of these access levels is required to begin the authorization (the first swipe).
    /// </summary>
    [DataField]
    public List<ProtoId<AccessLevelPrototype>> InitiatorAccess = new()
    {
        "Captain",
        "HeadOfSecurity",
        "CentralCommand",
    };

    /// <summary>
    /// Access required to confirm the authorization (the second swipe).
    /// </summary>
    [DataField]
    public ProtoId<AccessLevelPrototype> CommandAccess = "Command";
}
