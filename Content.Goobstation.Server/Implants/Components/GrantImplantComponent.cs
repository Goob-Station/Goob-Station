namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class GrantImplantComponent : Component
{
    [DataField] public HashSet<String> Implants { get; private set; } = new();
}
