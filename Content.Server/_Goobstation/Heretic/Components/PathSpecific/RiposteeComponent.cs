namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RiposteeComponent : Component
{
    [DataField] public float Cooldown = 20f;
    [ViewVariables(VVAccess.ReadWrite)] public float Timer = 20f;

    [DataField] public bool CanRiposte = true;
}
