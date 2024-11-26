namespace Content.Server.Heretic.Components;

/// <summary>
///     Indicates that an entity can act as a protective blade.
/// </summary>
[RegisterComponent]
public sealed partial class ProtectiveBladeComponent : Component
{
    [DataField] public float Lifetime = 60f;
    [ViewVariables(VVAccess.ReadWrite)] public float Timer = 60f;
}
