namespace Content.Server._Shitmed.DelayedDeath;

[RegisterComponent]
public sealed partial class DelayedDeathComponent : Component
{
    /// <summary>
    /// How long it takes to kill the entity.
    /// </summary>
    [DataField]
    public float DeathTime = 60;

    /// <summary>
    /// How long it has been since the delayed death timer started.
    /// </summary>
    public float DeathTimer;

    /// <summary>
    /// What message is displayed when the time runs out - Goobstation
    /// </summary>
    [DataField]
    public LocId DeathMessageId;
}
