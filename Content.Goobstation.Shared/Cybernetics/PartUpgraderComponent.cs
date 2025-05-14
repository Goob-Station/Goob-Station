using Content.Shared.Body.Part;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cybernetics;

/// <summary>
/// Upgrades an existing organ of the user
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PartUpgraderComponent : Component
{
    [DataField]
    public BodyPartType TargetBodyPart = BodyPartType.Other;

    [DataField]
    public BodyPartSymmetry TargetBodyPartSymmetry = BodyPartSymmetry.None;

    /// <summary>
    /// If it's null, will upgrade the body part. If not, will search for this organ.
    /// </summary>
    [DataField]
    public string? TargetOrgan;

    /// <summary>
    /// These components will be added to organ as BodyPart.OnAdd and applied to the user, for example, LeftMantisBladeUserComponent
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsToUser;

    /// <summary>
    /// These components will be added to the organ itself, for example, CyberneticsComponent
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsToPart;

    [DataField]
    public bool OneTimeUse = true;

    [DataField, AutoNetworkedField]
    public bool Used;

    [DataField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(15);

    [DataField] // If you're changing this, do not forget the loop
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/autosurgeon.ogg", AudioParams.Default.WithLoop(true).WithVolume(-6));

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActiveSound;
}

[Serializable, NetSerializable]
public sealed partial class PartUpgraderDoAfterEvent : SimpleDoAfterEvent;
