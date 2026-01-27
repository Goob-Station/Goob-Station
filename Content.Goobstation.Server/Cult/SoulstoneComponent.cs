namespace Content.Goobstation.Server.Cult;

/// <summary>
///     Contains a soul. The soul's mind in this case.
/// </summary>
[RegisterComponent]
public sealed partial class SoulstoneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public EntityUid? BoundMind;
}
