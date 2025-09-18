// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.OfficeChair;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleWallPushComponent : Component
{
    /// <summary>
    /// Action prototype granted to the strapped driver to perform a wall kick/push.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? ActionProto;

    [DataField]
    public EntityUid? KickAction;

    /// <summary>
    /// Speed added to the vehicle in the direction opposite the hit surface when kicking.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float KickSpeed = 7f;

    /// <summary>
    /// Minimum valid distance from the vehicle to the target surface for a kick to be considered.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinDistance = 0.2f;

    /// <summary>
    /// Maximum raycast distance to search for a blocking surface to push against.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxDistance = 1.5f;

    public const int KickMask = (int) (
        CollisionGroup.Impassable |
        CollisionGroup.BulletImpassable
    );

    /// <summary>
    /// Sound played when a valid kick connects with a surface.
    /// </summary>
    [DataField]
    public SoundSpecifier KickSound = new SoundPathSpecifier("/Audio/Weapons/punchmiss.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f).WithVariation(0.025f),
    };
}

public sealed partial class VehicleWallPushActionEvent : WorldTargetActionEvent
{
}
