using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._White.Xenomorphs.Ovipositor;

/// <summary>
/// Allows a xenomorph (empress) to attach to an ovipositor to produce plantable eggs.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class XenomorphOvipositorComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Attached;

    [DataField]
    public TimeSpan AttachDelay = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan DetachDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Cooldown between laying eggs while attached (1 egg / 3 seconds).
    /// </summary>
    [DataField]
    public TimeSpan LayEggCooldown = TimeSpan.FromSeconds(3);

    [DataField]
    public EntProtoId EggPrototype = "XenomorphEmpressEgg";

    /// <summary>
    /// Skip laying while this many unplanted eggs of <see cref="EggPrototype"/> are already nearby.
    /// </summary>
    [DataField]
    public int MaxNearbyUnplantedEggs = 6;

    /// <summary>
    /// Range used for <see cref="MaxNearbyUnplantedEggs"/>.
    /// </summary>
    [DataField]
    public float NearbyEggRange = 8f;

    /// <summary>
    /// Visual effect spawned at the empress when forced off the ovipositor by damage.
    /// </summary>
    [DataField]
    public EntProtoId? BurstEffectPrototype = "EffectXenomorphOvipositorBurst";

    [DataField]
    public EntProtoId AttachActionId = "ActionXenomorphOvipositorAttach";

    [DataField]
    public EntProtoId DetachActionId = "ActionXenomorphOvipositorDetach";

    [DataField]
    public EntProtoId LayEggActionId = "ActionXenomorphOvipositorLayEgg";

    [DataField]
    public ResPath AttachedSpriteRsi = new("/Textures/_White/Mobs/Aliens/Xenomorphs/empress_ovipositor.rsi");

    [DataField]
    public ResPath DetachedSpriteRsi = new("/Textures/_White/Mobs/Aliens/Xenomorphs/empress.rsi");

    [DataField]
    public string AttachedSpriteState = "xenomorph";

    [DataField]
    public string DetachedSpriteState = "xenomorph";

    [ViewVariables]
    public EntityUid? AttachAction;

    [ViewVariables]
    public EntityUid? DetachAction;

    [ViewVariables]
    public EntityUid? LayEggAction;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextLayEggAt;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphOvipositorAttachDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphOvipositorDetachDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum XenomorphOvipositorVisuals : byte
{
    Attached
}
