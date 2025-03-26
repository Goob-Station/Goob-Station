using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BlobMobComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), DataField("healthOfPulse")]
    public DamageSpecifier HealthOfPulse = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", -4 },
            { "Slash", -4 },
            { "Piercing", -4 },
            { "Heat", -4 },
            { "Cold", -4 },
            { "Shock", -4 },
        }
    };
}
