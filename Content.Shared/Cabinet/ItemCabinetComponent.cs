// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Cabinet;

/// <summary>
/// Used for entities that can be opened, closed, and can hold one item. E.g., fire extinguisher cabinets.
/// Requires <c>OpenableComponent</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ItemCabinetSystem))]
public sealed partial class ItemCabinetComponent : Component
{
    /// <summary>
    /// Name of the <see cref="ItemSlot"/> that stores the actual item.
    /// </summary>
    [DataField]
    public string Slot = "ItemCabinet";
}

[Serializable, NetSerializable]
public enum ItemCabinetVisuals : byte
{
    ContainsItem,
    Layer
}