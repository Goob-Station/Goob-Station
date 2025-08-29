using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class PlasmaAmmoProviderComponent : AmmoProviderComponent
{
    [DataField(required: true)]
    public EntProtoId Proto;

    [DataField]
    public FixedPoint2 FireCost = 55f;
}
