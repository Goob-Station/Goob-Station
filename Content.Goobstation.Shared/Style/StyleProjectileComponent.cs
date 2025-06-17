using Content.Goobstation.Common.Style;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Style;

/// <summary>
/// Used to mark the projectile that was shot from an entity that needs to track style
/// </summary>
[RegisterComponent, Serializable]
public sealed partial class StyleProjectileComponent : Component
{
    [DataField]
    public StyleCounterComponent? Component;

    [DataField]
    public EntityUid? User;
}
