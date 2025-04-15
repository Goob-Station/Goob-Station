// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Injazz <injazza@gmail.com>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2020 nuke <47336974+nuke-makes-games@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;

namespace Content.Server.Chemistry.Components;

/// <summary>
/// A container that transforms its appearance depending on the reagent it contains.
/// It returns to its initial state once the reagent is removed.
/// e.g. An empty glass changes to a beer glass when beer is added to it.
///
/// Should probably be joined with SolutionContainerVisualsComponent when solutions are networked.
/// </summary>
[RegisterComponent, Access(typeof(TransformableContainerSystem))]
public sealed partial class TransformableContainerComponent : Component
{
    /// <summary>
    /// This is the initial metadata description for the container.
    /// It will revert to this when emptied.
    ///     /// It defaults to the description of the parent entity unless overwritten.
    /// </summary>
    [DataField("initialDescription")]
    public string? InitialDescription;
    /// <summary>
    /// This stores whatever primary reagent is currently in the container.
    /// It is used to help determine if a transformation is needed on solution update.
    /// </summary>
    [DataField("currentReagent")]
    public ReagentPrototype? CurrentReagent;

    /// <summary>
    /// This returns whether this container in a transformed or initial state.
    /// </summary>
    ///
    [DataField("transformed")]
    public bool Transformed;
}