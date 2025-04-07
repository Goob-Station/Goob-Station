// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public sealed class InventoryDisplay : LayoutContainer
{
    private int Columns = 0;
    private int Rows = 0;
    private const int MarginThickness = 10;
    private const int ButtonSpacing = 5;
    private const int ButtonSize = 75;
    private readonly Control resizer;

    private readonly Dictionary<string, (SlotControl, Vector2i)> _buttons = new();

    public InventoryDisplay()
    {
        resizer = new Control();
        AddChild(resizer);
    }

    public SlotControl AddButton(SlotControl newButton, Vector2i buttonOffset)
    {
        AddChild(newButton);
        HorizontalExpand = true;
        VerticalExpand = true;
        InheritChildMeasure = true;
        if (!_buttons.TryAdd(newButton.SlotName, (newButton, buttonOffset)))
            Logger.Warning("Tried to add button without a slot!");
        SetPosition(newButton, buttonOffset * ButtonSize + new Vector2(ButtonSpacing, ButtonSpacing));
        UpdateSizeData(buttonOffset);
        return newButton;
    }

    public SlotControl? GetButton(string slotName)
    {
        return !_buttons.TryGetValue(slotName, out var foundButton) ? null : foundButton.Item1;
    }

    private void UpdateSizeData(Vector2i buttonOffset)
    {
        var (x, _) = buttonOffset;
        if (x > Columns)
            Columns = x;
        var (_, y) = buttonOffset;
        if (y > Rows)
            Rows = y;
        resizer.SetHeight = (Rows + 1) * (ButtonSize + ButtonSpacing);
        resizer.SetWidth = (Columns + 1) * (ButtonSize + ButtonSpacing);
    }

    public bool TryGetButton(string slotName, out SlotControl? button)
    {
        var success = _buttons.TryGetValue(slotName, out var buttonData);
        button = buttonData.Item1;
        return success;
    }

    public void RemoveButton(string slotName)
    {
        if (!_buttons.Remove(slotName))
            return;
        //recalculate the size of the control when a slot is removed
        Columns = 0;
        Rows = 0;
        foreach (var (_, (_, buttonOffset)) in _buttons)
        {
            UpdateSizeData(buttonOffset);
        }
    }

    public void ClearButtons()
    {
        Children.Clear();
    }
}