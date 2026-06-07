using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;


[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class TouchOfEvilComponent : Component
{
    /// <summary>
    ///  The damage buff the entity gets
    /// </summary>
    [DataField]
    public float DamageBuff = 2.5f;

    /// <summary>
    ///  The original damage the entity had
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public DamageSpecifier? OriginalDamage;

    /// <summary>
    /// Knock them up across the room
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;

    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [DataField]
    public TimeSpan BuffDuration = TimeSpan.FromSeconds(15);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [ViewVariables]
    public EntProtoId TouchOfEvilEffect = "StatusEffectTouchOfEvil";
}
