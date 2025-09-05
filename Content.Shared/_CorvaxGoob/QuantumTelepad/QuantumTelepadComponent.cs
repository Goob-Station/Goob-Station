using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.QuantumTelepad;

[RegisterComponent]
public sealed partial class QuantumTelepadComponent : Component
{
    /// <summary>
    /// Recharge time to teleport again
    /// </summary>
    [DataField]
    public float Delay = 30f;

    /// <summary>
    /// How much time need to wait before next teleport in seconds
    /// </summary>
    [DataField]
    public TimeSpan NextTeleport;

    /// <summary>
    /// Time after start teleport
    /// </summary>
    [DataField]
    public TimeSpan TeleportAt;

    /// <summary>
    /// Whitelist of allowed to teleport entities
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Blacklist of restricted to teleport entities
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_CorvaxGoob/Effects/emitter2.ogg");

    [DataField]
    public SoundSpecifier PreTeleportSound = new SoundPathSpecifier("/Audio/Weapons/flash.ogg");

    /// <summary>
    /// Visual effect of teleport that applies to entity
    /// </summary>
    [DataField]
    public EntProtoId? TeleportEffect;

    /// <summary>
    /// Current telepad status
    /// </summary>
    [DataField]
    public QuantumTelepadState State = QuantumTelepadState.Idle;

    [DataField]
    public float MaxEntitiesToTeleportAtOnce = 1;

    /// <summary>
    /// Search range for entities to teleport
    /// </summary>
    [DataField]
    public float WorkingRange = 0.3f;

    /// <summary>
    /// Flag to query entities
    /// </summary>
    [DataField("flag")]
    public LookupFlags LookupFlag = LookupFlags.Dynamic;

    [DataField]
    public bool MustBeAnchored = true;

    /// <summary>
    /// Works only with <see cref="DeviceLinkSourceComponent"/> "Range" field.
    /// </summary>
    [DataField]
    public bool CheckConnectionRange = true;
}

[Serializable, NetSerializable]
public enum QuantumTelepadState : byte
{
    Idle,
    Unlit,
    Teleporting,
    ReceiveTeleporting
};

[Serializable, NetSerializable]
public enum QuantumTelepadVisuals : byte
{
    State,
};
