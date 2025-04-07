// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Hands.Components;

namespace Content.Client.Toggleable;

/// <summary>
///     Component that handles the toggling the visuals of some light emitting entity.
/// </summary>
/// <remarks>
///     This will toggle the visibility of layers on an entity's sprite, the in-hand visuals, and the clothing/equipment
///     visuals. This will modify the color of any attached point lights.
/// </remarks>
[RegisterComponent]
public sealed partial class ToggleableLightVisualsComponent : Component
{
    /// <summary>
    ///     Sprite layer that will have its visibility toggled when this item is toggled.
    /// </summary>
    [DataField("spriteLayer")]
    public string? SpriteLayer = "light";

    /// <summary>
    ///     Layers to add to the sprite of the player that is holding this entity (while the component is toggled on).
    /// </summary>
    [DataField("inhandVisuals")]
    public Dictionary<HandLocation, List<PrototypeLayerData>> InhandVisuals = new();

    /// <summary>
    ///     Layers to add to the sprite of the player that is wearing this entity (while the component is toggled on).
    /// </summary>
    [DataField("clothingVisuals")]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();
}