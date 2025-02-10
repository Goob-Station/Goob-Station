namespace Content.Shared._Goobstation.MULE.Components;

/// <summary>
/// This is used for the MULE
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class MuleComponent : Component
{
    /// <summary>
    /// The current order that the MULE has been assigned.
    /// </summary>
    [DataField("currentOrders"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public MuleOrderType CurrentOrder = MuleOrderType.Idle;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntityUid CurrentTarget = EntityUid.Invalid;
}

public enum MuleOrderType
{
    Return,
    Transport,
    Idle,
    Blocked,
}
