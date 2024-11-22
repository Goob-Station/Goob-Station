using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Magic;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Spells;

public sealed partial class BloodCultStunEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(16);

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(12);

    [DataField]
    public string? Speech { get; set; }

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class BloodCultTeleportEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public float Range = 5;

    [DataField]
    public string? Speech { get; set; }

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class BloodCultEmpEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public float Range = 4;

    [DataField]
    public float EnergyConsumption = 1000;

    [DataField]
    public float Duration = 20;

    [DataField]
    public string? Speech { get; set; }

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class BloodCultShacklesEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public EntProtoId ShacklesProto = "ShadowShackles";

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(1);

    [DataField]
    public string? Speech { get; set; }

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class BloodCultTwistedConstructionEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; set; }

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class SummonEquipmentEvent : InstantActionEvent, ISpeakSpell
{
    /// <summary>
    /// Slot - EntProtoId
    /// </summary>
    [DataField]
    public Dictionary<string, EntProtoId> Prototypes = new();

    [DataField]
    public string? Speech { get; set; }

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class BloodSpearRecalledEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class TwistedConstructionDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CreateSpeellDoAfterEvent : SimpleDoAfterEvent
{
    public EntProtoId ActionProtoId;
}

[Serializable, NetSerializable]
public sealed partial class TeleportActionDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity Rune;
    public SoundPathSpecifier TeleportInSound = new("/Audio/WhiteDream/BloodCult/veilin.ogg");
    public SoundPathSpecifier TeleportOutSound = new("/Audio/WhiteDream/BloodCult/veilout.ogg");
}

[Serializable, NetSerializable]
public sealed partial class BloodRitesExtractDoAfterEvent : SimpleDoAfterEvent;
