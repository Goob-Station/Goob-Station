using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Language;
using Content.Shared.Polymorph;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling;

// <summary>
// Handles the main actions of a Shadowling
// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowlingComponent : Component
{
    // The round-start Shadowling Actions
    public string ActionHatch = "ActionHatch";
    public EntityUid? ActionHatchEntity;

    #region PostHatch Actions
    public string ActionEnthrall       = "ActionEnthrall";
    public string ActionGlare          = "ActionGlare";
    public string ActionVeil           = "ActionVeil";
    public string ActionRapidRehatch   = "ActionRapidRehatch";
    public string ActionShadowWalk     = "ActionShadowWalk";
    public string ActionIcyVeins       = "ActionIcyVeins";
    public string ActionDestroyEngines = "ActionDestroyEngines";
    public string ActionCollectiveMind = "ActionCollectiveMind";
    public string ActionAscendance     = "ActionAscendance"; // remove once debugged

    public EntityUid? ActionEnthrallEntity;
    public EntityUid? ActionGlareEntity;
    public EntityUid? ActionVeilEntity;
    public EntityUid? ActionRapidRehatchEntity;
    public EntityUid? ActionShadowWalkEntity;
    public EntityUid? ActionIcyVeinsEntity;
    public EntityUid? ActionDestroyEnginesEntity;
    public EntityUid? ActionCollectiveMindEntity;
    public EntityUid? ActionAscendanceEntity; // remove once debugged
    #endregion

    #region Ascension Actions
    public string ActionAnnihilate      = "ActionAnnihilate";
    public string ActionHypnosis        = "ActionHypnosis";
    public string ActionPlaneShift      = "ActionPlaneShift";
    public string ActionLightningStorm  = "ActionLightningStorm";
    public string ActionBroadcast       = "ActionAscendantBroadcast";

    public EntityUid? ActionAnnihilateEntity;
    public EntityUid? ActionHypnosisEntity;
    public EntityUid? ActionPlaneShiftEntity;
    public EntityUid? ActionLightningStormEntity;
    public EntityUid? ActionBroadcastEntity;
    #endregion

    // The status icon for Shadowlings
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ShadowlingFaction";

    // Phase Indicator
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public ShadowlingPhases CurrentPhase = ShadowlingPhases.PreHatch;

    [DataField]
    public bool IsHatching;

    [DataField]
    public ProtoId<PolymorphPrototype> ShadowlingPolymorphId = "ShadowlingPolymorph";

    [DataField]
    public bool IsPolymorphed;

    [DataField]
    public string Egg = "SlingEggHatch";

    // Thrall Indicator
    [DataField]
    public List<EntityUid> Thralls = new();

    [DataField]
    public ProtoId<AlertPrototype> AlertProto = "ShadowlingLight";

    [DataField]
    public float LightResistanceModifier = 0.12f;

    [DataField]
    public DamageSpecifier HeatDamage = new()
    {
        DamageDict = new()
        {
            ["Heat"] = 20,
        }
    };

    [DataField]
    public DamageSpecifier HeatDamageProjectileModifier = new()
    {
        DamageDict = new()
        {
            ["Heat"] = 10,
        }
    };

    [DataField]
    public DamageSpecifier GunShootFailDamage = new()
    {
        DamageDict = new()
        {
            ["Blunt"] = 5,
            ["Piercing"] = 4,
        }
    };

    [DataField]
    public TimeSpan GunShootFailStunTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float NormalDamage;

    [DataField]
    public float ModifiedDamage;

    [DataField]
    public ProtoId<LanguagePrototype> SlingLanguageId { get; set; } = "Shadowmind";

    [DataField]
    public bool IsAscending;

    [DataField]
    public string ObjectiveAscend = "ShadowlingAscendObjective";
}

[NetSerializable, Serializable]
public enum ShadowlingPhases : byte
{
    PreHatch,
    PostHatch,
    Ascension,
    FailedAscension,
}
