// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using System.Numerics;
using Content.Shared.Camera;
using Robust.Shared.Player;

namespace Content.Server.Camera;

public sealed class CameraRecoilSystem : SharedCameraRecoilSystem
{
    public override void KickCamera(EntityUid euid, Vector2 kickback, CameraRecoilComponent? component = null)
    {
        if (!Resolve(euid, ref component, false))
            return;

        RaiseNetworkEvent(new CameraKickEvent(GetNetEntity(euid), kickback), euid);
    }
}
