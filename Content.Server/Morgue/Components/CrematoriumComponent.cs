// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Morgue.Components;

[RegisterComponent]
public sealed partial class CrematoriumComponent : Component
{
    /// <summary>
    ///     The time it takes to cook in second
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int CookTime = 5;

    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Heat", 10000.0 },
        },
    };

    /// <summary>
    /// Do we need to strip the mob we are tryng to cremate
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool DemandStrip = true;

    [DataField("cremateStartSound")]
    public SoundSpecifier CremateStartSound = new SoundPathSpecifier("/Audio/Items/Lighters/lighter1.ogg");

    [DataField("crematingSound")]
    public SoundSpecifier CrematingSound = new SoundPathSpecifier("/Audio/Effects/burning.ogg");

    [DataField("cremateFinishSound")]
    public SoundSpecifier CremateFinishSound = new SoundPathSpecifier("/Audio/Machines/ding.ogg");

    [DataField]
    public SoundSpecifier CremateDeniedSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");
}
