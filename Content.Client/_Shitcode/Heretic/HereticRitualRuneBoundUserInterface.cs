// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._Shitcode.Heretic.UI;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic;

public sealed class HereticRitualRuneBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private HereticRitualRuneRadialMenu? _hereticRitualMenu;

    public HereticRitualRuneBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _hereticRitualMenu = this.CreateWindow<HereticRitualRuneRadialMenu>();
        _hereticRitualMenu.SetEntity(Owner);
        _hereticRitualMenu.SendHereticRitualRuneMessageAction += SendHereticRitualMessage;

        var vpSize = _displayManager.ScreenSize;
        _hereticRitualMenu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    private void SendHereticRitualMessage(ProtoId<HereticRitualPrototype> protoId)
    {
        SendMessage(new HereticRitualMessage(protoId));
    }
}