// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Teleport;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Shitcode.Wizard.Teleport;

[UsedImplicitly]
public sealed class WizardTeleportBoundUserInterface : BoundUserInterface
{
    private WizardTeleportTargetWindow? _menu;
    private NetEntity? _action;

    public WizardTeleportBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<WizardTeleportTargetWindow>();
        _menu.OpenCentered();
        _menu.WarpClicked += SendWizardTeleportSystemMessage;
        _menu.Populate();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not WizardTeleportState teleportState)
            return;

        _action = teleportState.Action;
        _menu?.UpdateWarps(teleportState.Warps);
        _menu?.Populate();
    }

    public void SendWizardTeleportSystemMessage(NetEntity warpUid, string name)
    {
        SendMessage(new WizardTeleportLocationSelectedMessage(warpUid, name, _action));
    }
}