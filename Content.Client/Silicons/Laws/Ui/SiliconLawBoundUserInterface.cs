// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <shadowjjt@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Silicons.Laws.Ui;

[UsedImplicitly]
public sealed class SiliconLawBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SiliconLawMenu? _menu;
    private EntityUid _owner;
    private List<SiliconLaw>? _laws;

    public SiliconLawBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _owner = owner;
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SiliconLawMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SiliconLawBuiState msg)
            return;

        if (_laws != null && _laws.Count == msg.Laws.Count)
        {
            var isSame = true;
            foreach (var law in msg.Laws)
            {
                if (_laws.Contains(law))
                    continue;
                isSame = false;
                break;
            }

            if (isSame)
                return;
        }

        _laws = msg.Laws.ToList();

        _menu?.Update(_owner, msg);
    }
}