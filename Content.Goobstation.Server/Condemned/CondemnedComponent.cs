namespace Content.Goobstation.Server.Condemned;

/// <summary>
/// Marks an entity as having sold their soul.
/// When you die, do NOT collect 200, do NOT pass go. Go directly to hell
/// </summary>
/// <remarks>
/// In the future, this will actually send you to hell.
/// But right now it just plays a fancy animation and deletes you :godo:
/// </remarks>
[RegisterComponent]
public sealed partial class CondemnedComponent : Component
{
    [DataField]
    public bool IsCorporateOwned = false;

    [DataField]
    public EntityUid SoulOwner;

    [ViewVariables]
    public CondemnedSystem.CondemnedPhase CurrentPhase = CondemnedSystem.CondemnedPhase.Waiting;

    [ViewVariables]
    public float PhaseTimer;

    [ViewVariables]
    public float HandDuration;

    [ViewVariables]
    public EntityUid? HandEntity;
}
