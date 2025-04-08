// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 RatherUncreative <RatherUncreativeName@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Whatstone <whatston3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

﻿using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used for a <see cref="BorgModuleComponent"/> that provides items to the entity it's installed into.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBorgSystem))]
public sealed partial class ItemBorgModuleComponent : Component
{
    /// <summary>
    /// The items that are provided.
    /// </summary>
    [DataField("items", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>), required: true)]
    public List<string> Items = new();

    /// <summary>
    /// The entities from <see cref="Items"/> that were spawned.
    /// </summary>
    [DataField("providedItems")]
    public SortedDictionary<string, EntityUid> ProvidedItems = new();

    /// <summary>
    /// A counter that ensures a unique
    /// </summary>
    [DataField("handCounter")]
    public int HandCounter;

    /// <summary>
    /// Whether or not the items have been created and stored in <see cref="ProvidedContainer"/>
    /// </summary>
    [DataField("itemsCrated")]
    public bool ItemsCreated;

    /// <summary>
    /// A container where provided items are stored when not being used.
    /// This is helpful as it means that items retain state.
    /// </summary>
    [ViewVariables]
    public Container ProvidedContainer = default!;

    /// <summary>
    /// An ID for the container where provided items are stored when not used.
    /// </summary>
    [DataField("providedContainerId")]
    public string ProvidedContainerId = "provided_container";

    /// <summary>
    /// Frontier: a module ID to check for equivalence // TODO: why not to make it automatically set itself to the prototype of the component's owner?
    /// </summary>
    [DataField(required: true)]
    public string ModuleId = default!;
}