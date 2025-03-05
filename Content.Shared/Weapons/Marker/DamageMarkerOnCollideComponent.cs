using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Marker;

/// <summary>
/// Applies <see cref="DamageMarkerComponent"/> when colliding with an entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedDamageMarkerSystem))]
public sealed partial class DamageMarkerOnCollideComponent : Component
{
    [DataField("whitelist"), AutoNetworkedField]
    public EntityWhitelist? Whitelist = new();

    [ViewVariables(VVAccess.ReadWrite), DataField("duration"), AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Additional damage to be applied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("damage")]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// How many more times we can apply it.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("amount"), AutoNetworkedField]
    public int Amount = 1;

    /// <summary>
    /// Sprite to apply to the entity while damagemarker is applied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("effect")]
    public SpriteSpecifier.Rsi Effect = default!; //new(new ResPath("/Textures/Objects/Weapons/Effects.rsi"), "shield2");

    /// <summary>
    /// Sound to play when the damage marker is procced.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("sound")]
    public SoundSpecifier? Sound; //= new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/kinetic_accel.ogg");

    /// <summary>
    ///     Lavaland Change: Whether the marker can only be applied to fauna.
    /// </summary>
    [DataField]
    public bool OnlyWorkOnFauna = false;
}
