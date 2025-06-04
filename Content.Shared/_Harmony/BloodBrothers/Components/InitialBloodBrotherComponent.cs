using Content.Shared._Harmony.BloodBrothers.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Harmony.BloodBrothers.Components;

/// <summary>
/// Signifies that an entity is the blood brother chosen by a game-rule.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBloodBrotherSystem))]
[AutoGenerateComponentState]
public sealed partial class InitialBloodBrotherComponent : Component
{
    #region Actions

    [DataField]
    public EntProtoId<EntityTargetActionComponent> ConvertAction = "ActionBloodBrotherConvert";

    [DataField, AutoNetworkedField]
    public EntityUid? ConvertActionEntity;

    [DataField]
    public EntProtoId<EntityTargetActionComponent> CheckConvertAction = "ActionBloodBrotherCheckConvert";

    [DataField, AutoNetworkedField]
    public EntityUid? CheckConvertActionEntity;

    #endregion

    /// <summary>
    /// The antag preference required for someone to be converted into a blood brother.
    /// If null, the check will be skipped.
    /// </summary>
    [DataField]
    public ProtoId<AntagPrototype>? RequiredAntagPreference = "BloodBrother";

    /// <summary>
    /// The popup that will happen when a blood brother is converted.
    /// </summary>
    [DataField]
    public LocId ConvertPopupText = "blood-brother-conversion-popup";

    /// <summary>
    /// The time for which the converted will be stunned after being converted.
    /// If null, the converted will not be stunned
    /// </summary>
    [DataField]
    public TimeSpan? ConvertStunTime = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The objective that will be given to the converted.
    /// The target will be automatically set to be the initial blood brother.
    /// </summary>
    [DataField]
    public EntProtoId<ObjectiveComponent> ConvertedBrotherObjective = "BloodBrotherConvertedObjective";

    #region Briefing

    [DataField]
    public LocId BriefingText = "blood-brother-role-greeting";

    [DataField]
    public Color BriefingColor = Color.MediumVioletRed;

    [DataField]
    public SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/Ambience/Antag/traitor_start.ogg"); // TODO: get a custom briefing sound

    #endregion

    /// <summary>
    /// The faction that the new blood brother will be added to.
    /// </summary>
    [DataField]
    public ProtoId<NpcFactionPrototype> BloodBrotherFaction = "BloodBrother";

    /// <summary>
    /// The mind role that will be given to the new blood brother.
    /// </summary>
    [DataField]
    public EntProtoId<MindRoleComponent> BloodBrotherMindRole = "MindRoleBloodBrother";

    public override bool SendOnlyToOwner => true;
}

public sealed partial class BloodBrotherConvertActionEvent : EntityTargetActionEvent;

public sealed partial class BloodBrotherCheckConvertActionEvent : EntityTargetActionEvent;
