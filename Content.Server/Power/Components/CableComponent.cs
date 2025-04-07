// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2020 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Dawid Bla <46636558+DawBla@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Power.EntitySystems;
using Content.Shared.Power;
using Content.Shared.Tools;
using Robust.Shared.Prototypes;
using Content.Shared.Tools.Systems;

namespace Content.Server.Power.Components;

/// <summary>
///     Allows the attached entity to be destroyed by a cutting tool, dropping a piece of cable.
/// </summary>
[RegisterComponent]
[Access(typeof(CableSystem))]
public sealed partial class CableComponent : Component
{
    [DataField]
    public EntProtoId CableDroppedOnCutPrototype = "CableHVStack1";

    /// <summary>
    /// The tool quality needed to cut the cable. Setting to null prevents cutting.
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype>? CuttingQuality = SharedToolSystem.CutQuality;

    /// <summary>
    ///     Checked by <see cref="CablePlacerComponent"/> to determine if there is
    ///     already a cable of a type on a tile.
    /// </summary>
    [DataField("cableType")]
    public CableType CableType = CableType.HighVoltage;

    [DataField("cuttingDelay")]
    public float CuttingDelay = 1f;
}

/// <summary>
///     Event to be raised when a cable is anchored / unanchored
/// </summary>
[ByRefEvent]
public readonly struct CableAnchorStateChangedEvent
{
    public readonly TransformComponent Transform;
    public EntityUid Entity => Transform.Owner;
    public bool Anchored => Transform.Anchored;

    /// <summary>
    ///     If true, the entity is being detached to null-space
    /// </summary>
    public readonly bool Detaching;

    public CableAnchorStateChangedEvent(TransformComponent transform, bool detaching = false)
    {
        Detaching = detaching;
        Transform = transform;
    }
}