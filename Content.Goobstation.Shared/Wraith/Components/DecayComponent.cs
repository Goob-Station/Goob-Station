using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent]
public sealed partial class DecayComponent : Component
{
    [DataField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The debuff applied while the component is present.
    /// </summary>
    [DataField]
    public DamageSpecifier WraithDecay = new()
    {
        DamageDict = new()
        {
            { "Asphyxiation", 5.0},
            { "Ion", 10.0},
        }
    };
}
