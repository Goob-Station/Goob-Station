using Content.Shared.Dataset;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Server._Sunrise.AssaultOps;

[RegisterComponent, Access(typeof(AssaultOpsRuleSystem))]
public sealed partial class AssaultOpsRuleComponent : Component
{
    [DataField("icarusKeyImplant", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string IcarusKeyImplant = "IcarusKey";

    [DataField("requiredKeys")] public int RequiredKeys = 3;

    [DataField("keysCarrierJobs", customTypeSerializer: typeof(PrototypeIdArraySerializer<JobPrototype>))]
    public string[] KeysCarrierJobs =
    {
        "Captain",
        "HeadOfSecurity",
        "ChiefEngineer",
        "ChiefMedicalOfficer",
        "ResearchDirector",
        "Quartermaster"
    };

    [DataField("faction", customTypeSerializer: typeof(PrototypeIdSerializer<NpcFactionPrototype>), required: true)]
    public string Faction = default!;

    [DataField]
    public int TCAmountPerOperative = 50;

    [DataField]
    public int RoundstartOperatives;

    [DataField("greetingSound", customTypeSerializer: typeof(SoundSpecifierTypeSerializer))]
    public SoundSpecifier? GreetSoundNotification = new SoundPathSpecifier("/Audio/_Sunrise/AssaultOperatives/assault_operatives_greet.ogg",
        AudioParams.Default.WithVolume(-6f));

    [DataField("winType")] public WinType WinType = WinType.Stalemate;

    [DataField("winConditions")] public List<WinCondition> WinConditions = new ();

    public EntityUid? ShuttleGrid;

    public EntityUid? TargetStation;
}

public enum WinType : byte
{
    /// <summary>
    ///     Operative major win. Goldeneye activated and all ops alive.
    /// </summary>
    OpsMajor,
    /// <summary>
    ///     Minor win. Goldeneye was activated and some ops alive.
    /// </summary>
    OpsMinor,
    /// <summary>
    ///     Hearty. Goldeneye activated but no ops alive.
    /// </summary>
    Hearty,
    /// <summary>
    ///     Stalemate. Goldeneye not activated and ops still alive.
    /// </summary>
    Stalemate,
    /// <summary>
    ///     Crew major win. Goldeneye not activated and no ops alive.
    /// </summary>
    CrewMajor
}

public enum WinCondition
{
    IcarusActivated,
    AllOpsDead,
    SomeOpsAlive,
    AllOpsAlive
}
