using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Personal;

/// <summary>
/// Saves information about spawned entities in player's mind. This prevents duplication of items if user choose ghost role.
/// </summary>
[RegisterComponent]
public sealed partial class PersonalEntityMarkerComponent : Component
{
    /// <summary>
    /// Saves name of personal prototype
    /// </summary>
    [DataField]
    public ProtoId<PersonalEntityPrototype>? PersonalPrototype;

    /// <summary>
    /// Marks if items are already spawned
    /// </summary>
    [DataField]
    public bool IsSpawned = false;
}
