// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicon.AiCameraWarping;

public abstract class SharedStationAiWarpSystem : EntitySystem { }

public sealed partial class ToggleCameraWarpScreenEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed class CameraWarpActionEvent : EntityEventArgs
{
    public NetEntity Target;

    public CameraWarpActionEvent(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public record struct CameraWarpData()
{
    public string DisplayName = string.Empty;
    public NetEntity NetEntityUid = NetEntity.Invalid;
}

[Serializable, NetSerializable]
public sealed class CameraWarpBuiState : BoundUserInterfaceState
{
    public List<CameraWarpData> CameraList;

    public CameraWarpBuiState(List<CameraWarpData> camList)
    {
        CameraList = camList;
    }
}

[Serializable, NetSerializable]
public sealed class CameraWarpActionMessage : BoundUserInterfaceMessage
{
    public readonly CameraWarpActionEvent? WarpAction;
    public CameraWarpActionMessage(CameraWarpActionEvent camWarpAction)
    {
        WarpAction = camWarpAction;
    }
}

[Serializable, NetSerializable]
public sealed class CameraWarpRefreshActionMessage : BoundUserInterfaceMessage { }

[Serializable, NetSerializable]
public enum CamWarpUiKey : byte
{
    Key
}
