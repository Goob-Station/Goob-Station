using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SlaughterDemon.Other;

/// <summary>
/// Deals damage to the slaughter demon, and expels you from its stomach
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DemonsKissComponent : Component
{
    /// <summary>
    ///  Damage to deal to the demon.
    /// Hardcoded cuz its un-yamlabble as a status effect that can only be achieved through drinking.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>()
        {
            { "Blunt", 25},
            { "Slash",  25},
        }
    };

    /// <summary>
    ///  Whether to eject the entity, or not
    /// </summary>
    [DataField]
    public bool Eject = true;
}
