using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.Cyberpsychosis;

/// <summary>
/// Component added to body parts
/// </summary>
[RegisterComponent]
public sealed partial class ModifyCyberSanityPartComponent : Component
{
    [DataField(required: true)]
    public int ToAdd = 0;
}
