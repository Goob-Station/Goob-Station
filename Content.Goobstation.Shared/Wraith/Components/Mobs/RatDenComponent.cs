using Content.Shared.Damage;
using Content.Shared.Tag;
using Linguini.Shared.Util;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RatDenComponent : Component
{
    [DataField]
    public EntProtoId MadRatProto = "MobMadRat";

    /// <summary>
    /// How long before it spawns.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillSpawn = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Next time at which they will spawn.
    /// </summary>
    public TimeSpan NextTickSpawn = TimeSpan.Zero;

    public DamageSpecifier HealAmount = new()
    {
        DamageDict = new()
        {
            { "Blunt", -1},
            { "Slash", -1 },
            { "Piercing", -1 },
            { "Heat", -1},
            { "Shock", -1},
            { "Cold", -1},
            { "Poison", -1},
            { "Radiation", -1},
            { "Asphyxiation", -1 }
        }
    };

    [DataField]
    public float HealRange = 5f;

    [DataField]
    public TimeSpan HealCooldown = TimeSpan.FromSeconds(5);

    public TimeSpan? NextTickHeal;
}
