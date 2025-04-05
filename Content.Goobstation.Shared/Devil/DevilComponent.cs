using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil;

[RegisterComponent]
public sealed partial class DevilComponent : Component
{
    public readonly List<EntProtoId> BaseDevilActions = new()
    {
        "ActionSoulStoreMenu",
        "ActionCreateContract",
        "ActionShadowJaunt",
        "ActionCreateRevivalContract",
    };

    /// <summary>
    /// The amount of souls or successful contracts the entity has.
    /// </summary>
    [DataField]
    public float Souls = 0f;

    /// <summary>
    /// The true name of the devil.
    /// This is auto-generated from a list in the system.
    /// </summary>
    [DataField]
    public string TrueName;
}
