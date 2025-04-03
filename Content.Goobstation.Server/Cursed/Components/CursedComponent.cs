namespace Content.Goobstation.Server.Cursed.Components;

[RegisterComponent]
public sealed partial class CursedComponent : Component
{
    [DataField]
    public float FreezeDuration { get; set; } = 4f;

    [DataField]
    public float Elapsed;

    [DataField]
    public bool HasTriggeredPopup = false;
}
