namespace Content.Shared._Goobstation.MULE.Components;

/// <summary>
/// This handles...
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class MuleDropOff : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public DropLocation Location = DropLocation.None;
}
