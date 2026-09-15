namespace Content.Goobstation.Server.Voidwalker.Kidnapping.Voided;

[RegisterComponent]
public sealed partial class VoidedComponent : Component
{
    /// <summary>
    /// The voidwalker that kidnapped this entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<VoidwalkerComponent>? Voidwalker;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextSpacedCheck;

    [DataField]
    public TimeSpan SpacedCheckInterval = TimeSpan.FromSeconds(2);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextVomitTime;

    [DataField]
    public TimeSpan MinVomitInterval = TimeSpan.FromSeconds(60);

    [DataField]
    public TimeSpan MaxVomitInterval = TimeSpan.FromSeconds(90);

    [DataField]
    public float HungerLost = -10f;

    [DataField]
    public float ThirstLost = -10f;

    [DataField]
    public string NebulaVomitProto = "NebulaVomit";
}
