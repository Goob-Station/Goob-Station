// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Bed.Cryostorage;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Bed.Cryostorage;

[UsedImplicitly]
public sealed class CryostorageBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private CryostorageMenu? _menu;

    public CryostorageBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<CryostorageMenu>();

        _menu.SlotRemoveButtonPressed += (ent, slot) =>
        {
            SendMessage(new CryostorageRemoveItemBuiMessage(ent, slot, CryostorageRemoveItemBuiMessage.RemovalType.Inventory));
        };

        _menu.HandRemoveButtonPressed += (ent, hand) =>
        {
            SendMessage(new CryostorageRemoveItemBuiMessage(ent, hand, CryostorageRemoveItemBuiMessage.RemovalType.Hand));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CryostorageBuiState msg:
                _menu?.UpdateState(msg);
                break;
        }
    }
}