// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Джексон Миссиссиппи <tripwiregamer@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Strip;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.Inventory;

[Prototype("inventoryTemplate")]
public sealed partial class InventoryTemplatePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = string.Empty;

    [DataField("slots")] public SlotDefinition[] Slots { get; private set; } = Array.Empty<SlotDefinition>();
}

[DataDefinition]
public sealed partial class SlotDefinition
{
    [DataField("name", required: true)] public string Name { get; private set; } = string.Empty;
    [DataField("slotTexture")] public string TextureName { get; private set; } = "pocket";
    /// <summary>
    /// The texture displayed in a slot when it has an item inside of it.
    /// </summary>
    [DataField] public string FullTextureName { get; private set; } = "SlotBackground";
    [DataField("slotFlags")] public SlotFlags SlotFlags { get; private set; } = SlotFlags.PREVENTEQUIP;
    [DataField("showInWindow")] public bool ShowInWindow { get; private set; } = true;
    [DataField("slotGroup")] public string SlotGroup { get; private set; } = "Default";
    [DataField("stripTime")] public TimeSpan StripTime { get; private set; } = TimeSpan.FromSeconds(4f);

    [DataField("uiWindowPos", required: true)]
    public Vector2i UIWindowPosition { get; private set; }

    [DataField("strippingWindowPos", required: true)]
    public Vector2i StrippingWindowPos { get; private set; }

    [DataField("dependsOn")] public string? DependsOn { get; private set; }

    [DataField("dependsOnComponents")] public ComponentRegistry? DependsOnComponents { get; private set; }

    [DataField("displayName", required: true)]
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    ///     Whether or not this slot will have its item hidden in the strip menu, and block interactions.
    ///     <seealso cref="SharedStrippableSystem.IsStripHidden"/>
    /// </summary>
    [DataField("stripHidden")] public bool StripHidden { get; private set; }

    /// <summary>
    ///     Offset for the clothing sprites.
    /// </summary>
    [DataField("offset")] public Vector2 Offset { get; private set; } = Vector2.Zero;

    /// <summary>
    ///     Entity whitelist for CanEquip checks.
    /// </summary>
    [DataField("whitelist")] public EntityWhitelist? Whitelist = null;

    /// <summary>
    ///     Entity blacklist for CanEquip checks.
    /// </summary>
    [DataField("blacklist")] public EntityWhitelist? Blacklist = null;
}