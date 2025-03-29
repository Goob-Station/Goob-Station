namespace Content.Goobstation.Server.Teleportation.Components;

[RegisterComponent]
public sealed partial class WarpParentOnTriggerComponent : Component
{
    [DataField] public string WarpLocation { get; set; } = "CentComm";
}
