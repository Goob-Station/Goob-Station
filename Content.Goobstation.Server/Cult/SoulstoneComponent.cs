namespace Content.Goobstation.Server.Cult;

[RegisterComponent]
public sealed partial class SoulstoneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public EntityUid? BoundMind;
}
