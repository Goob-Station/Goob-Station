using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// Cancels pull attempt and instead swallows them.
/// Swallowed stuff can be spat out or chewed (though those are technically different systems).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SwallowerComponent : Component
{
    [DataField]
    public string ContainerId = "swallower_stomach";

    [ViewVariables]
    public Container Stomach = default!;

    /// <summary>
    /// How many things the swallower can hold in their stomach at once.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Capacity = 3;

    /// <summary>
    /// Self explanatory.
    /// </summary>
    [DataField]
    public TimeSpan SwallowTime = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How many things to spit out when using the spit action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int SpitCount = 3;

    [DataField]
    public float SpitSpeed = 15f;

    [DataField]
    public float SpitDistance = 10f;

    /// <summary>
    /// Leave empty for harmless spit.
    /// </summary>
    [DataField]
    public DamageSpecifier SpitDamage = new();

    /// <summary>
    /// If it passes thw whitelist, will fully heal the swallower upon chewing.
    /// </summary>
    [DataField]
    public EntityWhitelist? ChewHealWhitelist;

    /// <summary>
    /// Cannot chew stuff that fits the blacklist so it just ignores it and chews the rest.
    /// </summary>
    [DataField]
    public EntityWhitelist? ChewBlacklist;

    [DataField]
    public SoundSpecifier? SwallowSound = new SoundPathSpecifier("/Audio/_Goobstation/Voice/gulpexotic.ogg");

    [DataField]
    public SoundSpecifier? SpitSound = new SoundPathSpecifier("/Audio/_Goobstation/Voice/spit.ogg");

    [DataField]
    public SoundCollectionSpecifier? ChewSound = new SoundCollectionSpecifier("eating");

    [DataField]
    public SoundSpecifier? RegurgitateSound;

    /// <summary>
    /// Like a shotgun.
    /// </summary>
    [DataField]
    public float SpitSpread = 30f;

    /// <summary>
    /// Only dead or crit can be swallowed when true.
    /// </summary>
    [DataField]
    public bool IncapacitatedOnly = true;
}

public sealed partial class SwallowerRegurgitateEvent : InstantActionEvent;

public sealed partial class SwallowerSpitEvent : WorldTargetActionEvent;

public sealed partial class SwallowerChewEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class SwallowDoAfterEvent : SimpleDoAfterEvent;
