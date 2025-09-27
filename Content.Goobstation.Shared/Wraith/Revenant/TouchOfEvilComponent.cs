using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Revenant;


[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class TouchOfEvilComponent : Component
{
    /// <summary>
    ///  The damage buff the entity gets
    /// </summary>
    [DataField(required: true)]
    public float DamageBuff;

    /// <summary>
    ///  The original damage the entity had
    /// </summary>
    [ViewVariables]
    public DamageSpecifier? OriginalDamage;

    /// <summary>
    /// Knock them up across the room
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;

    [DataField]
    public bool Active;

    [DataField]
    public TimeSpan BuffDuration = TimeSpan.FromSeconds(15);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
