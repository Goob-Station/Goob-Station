using Robust.Shared.GameStates;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCombatMarkComponent : Component
{
    [DataField] public string Path = string.Empty;
    // attached entity is used only for good looks and does/should do absolutely nothing else
    [DataField] public EntityUid? AttachedEntity = null;
}
