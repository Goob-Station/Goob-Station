// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.RCD.Systems;
using Content.Shared.Atmos.Components; // Pirate: chem plumbing
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization; // Pirate: chem plumbing

namespace Content.Shared.RCD.Components;

/// <summary>
/// Main component for the RCD
/// Optionally uses LimitedChargesComponent.
/// Charges can be refilled with RCD ammo
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RCDSystem))]
public sealed partial class RCDComponent : Component
{
    /// <summary>
    /// List of RCD prototypes that the device comes loaded with
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<RCDPrototype>> AvailablePrototypes { get; set; } = new();

    /// <summary>
    /// Sound that plays when a RCD operation successfully completes
    /// </summary>
    [DataField]
    public SoundSpecifier SuccessSound { get; set; } = new SoundPathSpecifier("/Audio/Items/deconstruct.ogg");

    /// <summary>
    /// The ProtoId of the currently selected RCD prototype
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<RCDPrototype> ProtoId { get; set; } = "Invalid";

    /// <summary>
    /// Indicates if a mirrored version of the construction prototype should be used (if available)
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public bool UseMirrorPrototype = false;

    /// <summary>
    /// Indicates whether this is an RCD or an RPD
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsRpd { get; set; } = false;

    #region Pirate: chem plumbing
    /// <summary>
    /// Indicates whether this is an RPLD.
    /// </summary>
    [DataField("isRPLD"), AutoNetworkedField]
    public bool IsRPLD { get; set; } = false;

    /// <summary>
    /// Last free-mode pipe layer selected by the client.
    /// </summary>
    [DataField]
    public AtmosPipeLayer? LastSelectedLayer { get; set; } = null;

    /// <summary>
    /// Current pipe layer build mode for RPD/RPLD placement.
    /// </summary>
    [DataField, AutoNetworkedField]
    public RpdMode CurrentMode { get; set; } = RpdMode.Free;

    [DataField]
    public SoundSpecifier SoundSwitchMode { get; set; } = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");
    #endregion

    /// <summary>
    /// The direction constructed entities will face upon spawning
    /// </summary>
    [DataField, AutoNetworkedField]
    public Direction ConstructionDirection
    {
        get => _constructionDirection;
        set
        {
            _constructionDirection = value;
            ConstructionTransform = new Transform(new(), _constructionDirection.ToAngle());
        }
    }

    private Direction _constructionDirection = Direction.South;

    /// <summary>
    /// Returns a rotated transform based on the specified ConstructionDirection
    /// </summary>
    /// <remarks>
    /// Contains no position data
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public Transform ConstructionTransform { get; private set; }
}

#region Pirate: chem plumbing
[Serializable, NetSerializable]
public enum RpdMode : byte
{
    Primary = 0,
    Secondary = 1,
    Tertiary = 2,
    Quaternary = 3,
    Quinary = 4,
    Free = 5,
}
#endregion
