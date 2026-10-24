using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Security;

[RegisterComponent, NetworkedComponent]
public sealed partial class StructureBreakerComponent : Component
{
    /// <summary>
    /// What it can target.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    /// <summary>
    /// Do after time before it deals damage.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1.5);

    /// <summary>
    /// Damage dealt. (Or healing if you revert the numbers, you know how it is.)
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public SoundSpecifier? HitSound;
}

[Serializable, NetSerializable]
public sealed partial class StructureBreakerDoAfterEvent : SimpleDoAfterEvent;
