using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Hierophant;

public sealed partial class HierophantBeatActionEvent : EntityTargetActionEvent;

public sealed partial class HierophantChasersActionEvent : EntityTargetActionEvent
{
    [DataField]
    public EntProtoId ChaserTile = "LavalandHierophantChaser";

    [DataField]
    public EntProtoId DamageTile = "LavalandHierophantSquare";

    [DataField("speed", required: true)]
    public MegafaunaNumberSelector SpeedSelector;

    [DataField("steps", required: true)]
    public MegafaunaNumberSelector StepsSelector;

    [DataField("amount", required: true)]
    public MegafaunaNumberSelector AmountSelector;
}

public sealed partial class HierophantBlinkActionEvent : WorldTargetActionEvent
{
    [DataField(required: true)]
    public EntProtoId Spawn;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(0.9f);

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class HierophantAttackActionEvent : WorldTargetActionEvent
{
    [DataField(required: true)]
    public EntProtoId Spawn;
}
