// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Containers.ItemSlots;

namespace Content.Server.Labels.Components
{
    /// <summary>
    ///     This component allows you to attach and remove a piece of paper to an entity.
    /// </summary>
    [RegisterComponent]
    public sealed partial class PaperLabelComponent : Component
    {
        [DataField("labelSlot")]
        public ItemSlot LabelSlot = new();
    }
}