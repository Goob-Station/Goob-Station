// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Weapons.AmmoSelector;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.AmmoSelector;

[UsedImplicitly]
public sealed class AmmoSelectorMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private AmmoSelectorMenu? _menu;

    public AmmoSelectorMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<AmmoSelectorMenu>();
        _menu.SetEntity(Owner);
        _menu.SendAmmoSelectorSystemMessageAction += SendAmmoSelectorSystemMessage;

        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    public void SendAmmoSelectorSystemMessage(ProtoId<SelectableAmmoPrototype> protoId)
    {
        SendPredictedMessage(new AmmoSelectedMessage(protoId));
    }
}