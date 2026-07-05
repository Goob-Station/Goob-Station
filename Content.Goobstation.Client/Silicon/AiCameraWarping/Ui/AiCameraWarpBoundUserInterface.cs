// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.AiCameraWarping;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Silicon.AiCameraWarping.Ui;

public sealed class AiCameraWarpBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private AiCameraWarpMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<AiCameraWarpMenu>();
        _menu.OnCamWarpAction += SendAction;
        _menu.OnRefresh += SendRefresh;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CameraWarpBuiState msg)
            return;

        _menu?.Update(msg);
    }

    public void SendAction(CameraWarpActionEvent action)
    {
        SendMessage(new CameraWarpActionMessage(action));
    }

    public void SendRefresh()
    {
        SendMessage(new CameraWarpRefreshActionMessage());
    }
}
