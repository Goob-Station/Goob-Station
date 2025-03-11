using Robust.Shared.GameStates;
using Content.Shared.Damage;

namespace Content.Shared._White.Grab;

[RegisterComponent, NetworkedComponent]
public sealed partial class GrabThrownComponent : Component
{
    public DamageSpecifier? DamageOnCollide;

    public List<EntityUid> IgnoreEntity = new();
}
