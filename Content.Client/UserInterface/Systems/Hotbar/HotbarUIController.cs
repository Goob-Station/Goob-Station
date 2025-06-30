// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Hands;
using Content.Client.UserInterface.Systems.Hands.Controls;
using Content.Client.UserInterface.Systems.Hotbar.Widgets;
using Content.Client.UserInterface.Systems.Inventory;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Content.Client.UserInterface.Systems.Inventory.Widgets;
using Content.Client.UserInterface.Systems.Storage;
using Content.Client.UserInterface.Systems.Storage.Controls;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.Hotbar;

public sealed class HotbarUIController : UIController
{
    private InventoryUIController? _inventory;
    private HandsUIController? _hands;
    private StorageUIController? _storage;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        ReloadHotbar();
    }

    public void Setup(HandsContainer handsContainer)
    {
        _inventory = UIManager.GetUIController<InventoryUIController>();
        _hands = UIManager.GetUIController<HandsUIController>();
        _storage = UIManager.GetUIController<StorageUIController>();
        _hands.RegisterHandContainer(handsContainer);
    }

    public void ReloadHotbar()
    {
        if (UIManager.ActiveScreen == null)
        {
            return;
        }

        if (UIManager.ActiveScreen.GetWidget<HotbarGui>() is { } hotbar)
        {
            foreach (var container in GetAllItemSlotContainers(hotbar))
            {
                // Yes, this is dirty.
                container.SlotGroup = container.SlotGroup;
            }
        }

        _hands?.ReloadHands();
        _inventory?.ReloadSlots();

        //todo move this over to its own hellhole
        var inventory = UIManager.ActiveScreen.GetWidget<InventoryGui>();
        if (inventory == null)
        {
            return;
        }

        foreach (var container in GetAllItemSlotContainers(inventory))
        {
            // Yes, this is dirty.
            container.SlotGroup = container.SlotGroup;
        }

        _inventory?.RegisterInventoryBarContainer(inventory.InventoryHotbar);
    }

    private static IEnumerable<ItemSlotButtonContainer> GetAllItemSlotContainers(Control gui)
    {
        var result = new List<ItemSlotButtonContainer>();

        foreach (var child in gui.Children)
        {
            if (child is ItemSlotButtonContainer container)
            {
                result.Add(container);
            }

            result.AddRange(GetAllItemSlotContainers(child));
        }

        return result;
    }
}