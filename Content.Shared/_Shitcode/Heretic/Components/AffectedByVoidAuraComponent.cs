using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AffectedByVoidAuraComponent : Component
{
    [DataField]
    public EntityUid Aura;

    [DataField]
    public float? OldVelocity;
}
