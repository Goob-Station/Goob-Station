// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Clon clover <76759079+noudoit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Alert;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cuffs.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedCuffableSystem))]
public sealed partial class CuffableComponent : Component
{
    /// <summary>
    /// The current RSI for the handcuff layer
    /// </summary>
    [DataField("currentRSI"), ViewVariables(VVAccess.ReadWrite)]
    public string? CurrentRSI;

    /// <summary>
    /// How many of this entity's hands are currently cuffed.
    /// </summary>
    [ViewVariables]
    public int CuffedHandCount => Container.ContainedEntities.Count * 2;

    /// <summary>
    /// The last pair of cuffs that was added to this entity.
    /// </summary>
    [ViewVariables]
    public EntityUid LastAddedCuffs => Container.ContainedEntities[^1];

    /// <summary>
    ///     Container of various handcuffs currently applied to the entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Container Container = default!;

    /// <summary>
    /// Whether or not the entity can still interact (is not cuffed)
    /// </summary>
    [DataField("canStillInteract"), ViewVariables(VVAccess.ReadWrite)]
    public bool CanStillInteract = true;

    [DataField]
    public ProtoId<AlertPrototype> CuffedAlert = "Handcuffed";
}

public sealed partial class RemoveCuffsAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public sealed class CuffableComponentState : ComponentState
{
    public readonly bool CanStillInteract;
    public readonly int NumHandsCuffed;
    public readonly string? RSI;
    public readonly string? IconState;
    public readonly Color? Color;

    public CuffableComponentState(int numHandsCuffed, bool canStillInteract, string? rsiPath, string? iconState, Color? color)
    {
        NumHandsCuffed = numHandsCuffed;
        CanStillInteract = canStillInteract;
        RSI = rsiPath;
        IconState = iconState;
        Color = color;
    }
}

[ByRefEvent]
public readonly record struct CuffedStateChangeEvent;
