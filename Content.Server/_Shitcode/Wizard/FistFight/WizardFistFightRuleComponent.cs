using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.FistFight;

[RegisterComponent, Access(typeof(WizardFistFightSystem))]
public sealed partial class WizardFistFightRuleComponent : Component
{
    [DataField]
    public SoundSpecifier Music = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/wizard_fistfight.ogg", AudioParams.Default.WithVolume(2f));

    /// <summary>
    /// This should be however long the song is.
    /// </summary>
    [DataField]
    public TimeSpan MaxRuleLength = TimeSpan.FromSeconds(277);

    [DataField]
    public SoundSpecifier BellSound = new SoundPathSpecifier("/Audio/Weapons/boxingbell.ogg", AudioParams.Default.WithVolume(-10f));

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/teleport_app.ogg");

    [DataField]
    public EntProtoId TeleportEffect = "AdminInstantEffectSmoke10";

    [DataField]
    public TimeSpan IntroDuration = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan RoundDuration = TimeSpan.FromSeconds(40);

    [DataField]
    public TimeSpan BreakDuration = TimeSpan.FromSeconds(8);

    [DataField]
    public TimeSpan SuddenDeathDuration = TimeSpan.FromSeconds(60);

    [DataField]
    public int Rounds = 5;

    /// <summary>
    /// How long to wait to execute the loser after they lost. Rest is self explanatory.
    /// </summary>
    [DataField]
    public TimeSpan ExecutionAfterVerdictDelay = TimeSpan.FromSeconds(4);

    [DataField]
    public TimeSpan ReturnAfterGibDelay = TimeSpan.FromSeconds(4);

    [DataField]
    public TimeSpan CleanupAfterReturnDelay = TimeSpan.FromSeconds(3);

    [DataField]
    public LocId IntroAnnouncement = "wizard-fistfight-intro";

    [DataField]
    public EntProtoId RedGloves = "ClothingHandsGlovesBoxingRed";

    [DataField]
    public EntProtoId BlueGloves = "ClothingHandsGlovesBoxingBlue";

    [DataField]
    public EntProtoId VictoryItem = "WizardsGrimoire";

    [DataField]
    public LocId AnnouncerName = "wizard-fistfight-announcer";

    [DataField]
    public Color AnnouncerColor = Color.FromHex("#f5b942");

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 WizCoinPrize;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 WizCoinRefund;

    [ViewVariables(VVAccess.ReadOnly)]
    public WizardFistFightStage Stage = WizardFistFightStage.WaitingForChallenger;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan StageEnd;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan FightStart;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurrentRound;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Caster;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Challenger;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Spawner;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? MusicEntity;

    [ViewVariables(VVAccess.ReadOnly)]
    public MapId? ArenaMap;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates? RedCorner;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates? BlueCorner;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityCoordinates> SpectatorSpawns = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates? Home;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Winner;
    [ViewVariables(VVAccess.ReadOnly)]
    public bool PrizePaid;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Losers = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Remains = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public Container? Storage;

}

public enum WizardFistFightStage : byte
{
    WaitingForChallenger,
    Intro,
    Round,
    Break,
    Verdict,
    Execution,
    Return,
}
