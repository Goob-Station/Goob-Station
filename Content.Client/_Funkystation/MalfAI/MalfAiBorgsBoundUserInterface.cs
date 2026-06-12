// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.MalfAI;

public sealed class MalfAiBorgsBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MalfAiBorgsWindow? _window;

    public MalfAiBorgsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new MalfAiBorgsWindow(this);
        _window.OpenCentered();
        _window.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is MalfAiBorgsUiState borgsState)
            _window?.UpdateState(borgsState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Dispose();
    }

    public void SendSetSync(NetEntity borg, bool synced)
    {
        SendMessage(new MalfAiBorgsSetSyncMessage(borg, synced));
    }

    public void SendOpenMasterLawset()
    {
        SendMessage(new MalfAiOpenMasterLawsetMessage());
    }

    public void SendJumpToBorg(NetEntity borg)
    {
        SendMessage(new MalfAiBorgsJumpToBorgMessage(borg));
    }
}
