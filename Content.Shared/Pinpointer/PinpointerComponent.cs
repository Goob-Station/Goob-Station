// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Pinpointer;

/// <summary>
/// Displays a sprite on the item that points towards the target component.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedPinpointerSystem))]
public sealed partial class PinpointerComponent : Component
{
    /// <summary>
    /// Goob edit: pinpointer now works on EntityWhitelist (actually only on components but nvm)
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField("mediumDistance"), ViewVariables(VVAccess.ReadWrite)]
    public float MediumDistance = 16f;

    [DataField("closeDistance"), ViewVariables(VVAccess.ReadWrite)]
    public float CloseDistance = 8f;

    [DataField("reachedDistance"), ViewVariables(VVAccess.ReadWrite)]
    public float ReachedDistance = 1f;

    /// <summary>
    ///     Pinpointer arrow precision in radians.
    /// </summary>
    [DataField("precision"), ViewVariables(VVAccess.ReadWrite)]
    public double Precision = 0.09;

    /// <summary>
    ///     Name to display of the target being tracked.
    /// </summary>
    [DataField("targetName"), ViewVariables(VVAccess.ReadWrite)]
    public string? TargetName;

    /// <summary>
    ///     Whether or not the target name should be updated when the target is updated.
    /// </summary>
    [DataField("updateTargetName"), ViewVariables(VVAccess.ReadWrite)]
    public bool UpdateTargetName;

    /// <summary>
    ///     Goob edit: pinpointer can retarget only whitelisted entities if specified.
    /// </summary>
    [DataField]
    public EntityWhitelist? RetargetingWhitelist;

    [DataField]
    public EntityWhitelist? RetargetingBlacklist;

    /// <summary>
    ///     Whether or not the target can be reassigned.
    /// </summary>
    [DataField("canRetarget"), ViewVariables(VVAccess.ReadWrite)]
    public bool CanRetarget;

    /// <summary>
    ///     Goob edit: if true, this pinpointer will automatically track ANY nearest entity of a specified type.
    ///     Doesn't work with retargeting, it will always left only one entity in target list.
    /// </summary>
    [DataField]
    public bool CanTargetMultiple = true;

    /// <summary>
    /// Goob edit: many targets instead of just one
    /// </summary>
    [ViewVariables]
    public List<EntityUid> Targets = new();

    // WD EDIT START
    [DataField]
    public ProtoId<AlertPrototype>? Alert;

    [DataField]
    public bool CanToggle = true;

    [DataField]
    public bool CanEmag = true;

    [DataField]
    public bool CanExamine = true;
    // WD EDIT END

    [DataField, AutoNetworkedField] // WD EDIT: ViewVariables -> DataField
    public bool IsActive = false;

    [ViewVariables, AutoNetworkedField]
    public Angle ArrowAngle;

    [ViewVariables, AutoNetworkedField]
    public Distance DistanceToTarget = Distance.Unknown;

    [ViewVariables]
    public bool HasTarget => DistanceToTarget != Distance.Unknown;
}

[Serializable, NetSerializable]
public enum Distance : byte
{
    Unknown,
    Reached,
    Close,
    Medium,
    Far
}
