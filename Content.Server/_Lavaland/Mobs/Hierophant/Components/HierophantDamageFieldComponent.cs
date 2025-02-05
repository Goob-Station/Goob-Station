using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantDamageFieldComponent : Component
{
    /// <summary>
    /// Setting this to false will ignore damaging and sound logic.
    /// Used on LavalandHierophantSquare prototype to pass it's owner to the damage square.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// Entity that caused this damaging square to spawn.
    /// It will be ignored by this square.
    /// </summary>
    [DataField]
    public EntityUid OwnerEntity;

    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public SoundPathSpecifier? Sound;
}
