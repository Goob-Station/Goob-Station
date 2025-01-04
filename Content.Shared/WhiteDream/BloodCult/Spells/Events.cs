using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Magic;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Spells;

public sealed partial class BloodCultTeleportEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public float Range = 5;

    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(2);

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

    [DataField]
    public bool Force { get; set; } = true;

    public InGameICChatType ChatType => InGameICChatType.Whisper;
}

public sealed partial class BloodSpearRecalledEvent : InstantActionEvent;

public sealed partial class PlaceTileEntityEvent : WorldTargetActionEvent
{
    [DataField]
    public EntProtoId? Entity;

    [DataField]
    public string? TileId;

    [DataField]
    public SoundSpecifier? Audio;

}

public sealed partial class PhaseShiftEvent : InstantActionEvent
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    [DataField]
    public ProtoId<StatusEffectPrototype> StatusEffectId = "PhaseShifted";
}

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
