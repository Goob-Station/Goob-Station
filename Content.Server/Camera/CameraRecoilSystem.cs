// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Camera;

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