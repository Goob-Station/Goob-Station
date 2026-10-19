// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Content.Shared.MalfAI;

namespace Content.Client.MalfAI;

[UsedImplicitly]
public sealed class MalfAiBorgsBoundUserInterface : BoundUserInterface
{
    private MalfAiBorgsWindow? _window;

    public MalfAiBorgsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<MalfAiBorgsWindow>();
        if (_window != null)
        {
            _window.OnUpdateLawsRequested += OnUpdateLawsRequested;
            _window.OnJumpToBorgRequested += OnJumpToBorgRequested;
            _window.OnMasterLawsetRequested += OnMasterLawsetRequested;
            _window.OnSetSyncRequested += OnSetSyncRequested;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _window != null)
        {
            _window.OnUpdateLawsRequested -= OnUpdateLawsRequested;
            _window.OnJumpToBorgRequested -= OnJumpToBorgRequested;
            _window.OnMasterLawsetRequested -= OnMasterLawsetRequested;
            _window.OnSetSyncRequested -= OnSetSyncRequested;
        }
        base.Dispose(disposing);
    }

    private void OnUpdateLawsRequested(string uniqueId)
    {
        SendMessage(new MalfAiBorgsUpdateLawsMessage(uniqueId));
    }

    private void OnJumpToBorgRequested(string uniqueId)
    {
        SendMessage(new MalfAiBorgsJumpToBorgMessage(uniqueId));
    }

    private void OnSetSyncRequested(string uniqueId, bool enabled)
    {
        SendMessage(new MalfAiBorgsSetSyncMessage(uniqueId, enabled));
    }

    private void OnMasterLawsetRequested()
    {
        SendMessage(new MalfAiOpenMasterLawsetMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is MalfAiBorgsUiState s)
        {
            _window?.UpdateState(s);
        }
    }
}
