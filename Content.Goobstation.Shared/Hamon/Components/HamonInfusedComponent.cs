using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Hamon.Components;

/// <summary>
/// Any entity with this component will be infused with hamon which has special effects depending on the entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HamonInfusedComponent : Component
{
    public TimeSpan HamonWearOffTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningshock.ogg");

    [DataField]
    public ResPath RsiPath = new("/Textures/Effects/sparks.rsi");

    [DataField]
    public string RsiState = "sparks";

    [DataField]
    public DamageSpecifier BonusDamage = new()
    {
        DamageDict = new()
        {
            { "Holy", 5},
            { "Blunt", 3},
            { "Shock", 3},
        }
    };
}
