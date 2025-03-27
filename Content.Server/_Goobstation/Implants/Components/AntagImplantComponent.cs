namespace Content.Server._Goobstation.Implants.Components;

[RegisterComponent]
public sealed partial class AntagImplantComponent : Component
{
    [DataField("antag")]
    public string? SelectedAntagPrototype { get; set; }
}
