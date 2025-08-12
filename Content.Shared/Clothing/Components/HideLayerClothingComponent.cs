// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 paige404 <59348003+paige404@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// This is used for a clothing item that hides an appearance layer.
/// The entity's HumanoidAppearance component must have the corresponding hideLayerOnEquip value.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HideLayerClothingComponent : Component
{
    /// <summary>
    /// The appearance layer(s) to hide. Use <see cref='Layers'>Layers</see> instead.
    /// </summary>
    [DataField]
    [Obsolete("This attribute is deprecated, please use Layers instead.")]
    public HashSet<HumanoidVisualLayers>? Slots;

    /// <summary>
    /// A map of the appearance layer(s) to hide, and the equipment slot that should hide them.
    /// </summary>
    [DataField]
    public Dictionary<HumanoidVisualLayers, SlotFlags> Layers = new();

    /// <summary>
    /// EE Plasmeme Change: The clothing layers to hide.
    /// </summary>
    [DataField]
    public HashSet<string>? ClothingSlots = new();

    /// <summary>
    /// If true, the layer will only hide when the item is in a toggled state (e.g. masks)
    /// </summary>
    [DataField]
    public bool HideOnToggle = false;
}