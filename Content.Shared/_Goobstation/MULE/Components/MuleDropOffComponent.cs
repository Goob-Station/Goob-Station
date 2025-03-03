namespace Content.Shared._Goobstation.MULE.Components;

/// <summary>
/// This handles...
/// </summary>
[RegisterComponent]
[AutoGenerateComponentState]
public sealed partial class MuleDropOffComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public string Tag = string.Empty;
}

