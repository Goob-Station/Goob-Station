// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public sealed class ItemSlotButtonContainer : ItemSlotUIContainer<SlotControl>
{
    private readonly InventoryUIController _inventoryController;
    private string _slotGroup = "";

    public string SlotGroup
    {
        get => _slotGroup;
        set
        {
            _inventoryController.RemoveSlotGroup(SlotGroup);
            _slotGroup = value;
            _inventoryController.RegisterSlotGroupContainer(this);
        }
    }

    public ItemSlotButtonContainer()
    {
        _inventoryController = UserInterfaceManager.GetUIController<InventoryUIController>();
    }
}