// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Implants.Components;

/// <summary>
/// Subdermal implants get stored in a container on an entity and grant the entity special actions
/// The actions can be activated via an action, a passive ability (ie tracking), or a reactive ability (ie on death) or some sort of combination
/// They're added and removed with implanters
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SubdermalImplantComponent : Component
{
    /// <summary>
    /// Used where you want the implant to grant the owner an instant action.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("implantAction")]
    public EntProtoId? ImplantAction;

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    /// <summary>
    /// The entity this implant is inside
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ImplantedEntity;

    /// <summary>
    /// Should this implant be removeable?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("permanent"), AutoNetworkedField]
    public bool Permanent = false;

    /// <summary>
    /// Should you be able to implant this into yourself?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool CanImplantSelf = true; // Goobstation - allow traitors to buy suicide implants (fields for self-/other-implantability)

    /// <summary>
    /// Should you be able to implant this into others?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool CanImplantOther = true; // Goobstation - allow traitors to buy suicide implants (fields for self-/other-implantability)

    /// <summary>
    /// Multiplier to time taken to implant this implant
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ImplantationTimeMultiplier = 1; // Goobstation - allow traitors to buy suicide implants (add time multiplier)

    /// <summary>
    /// Target whitelist for this implant specifically.
    /// Only checked if the implanter allows implanting on the target to begin with.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Target blacklist for this implant specifically.
    /// Only checked if the implanter allows implanting on the target to begin with.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
    
    /// <summary>
    /// If set, this ProtoId is used when attempting to draw the implant instead.
    /// Useful if the implant is a child to another implant and you don't want to differentiate between them when drawing.
    /// </summary>
    [DataField]
    public EntProtoId? DrawableProtoIdOverride;
}

/// <summary>
/// Used for opening the storage implant via action.
/// </summary>
public sealed partial class OpenStorageImplantEvent : InstantActionEvent
{

}

public sealed partial class UseFreedomImplantEvent : InstantActionEvent
{

}

/// <summary>
/// Used for triggering trigger events on the implant via action
/// </summary>
public sealed partial class ActivateImplantEvent : InstantActionEvent
{

}

/// <summary>
/// Used for opening the uplink implant via action.
/// </summary>
public sealed partial class OpenUplinkImplantEvent : InstantActionEvent
{

}

public sealed partial class UseScramImplantEvent : InstantActionEvent
{

}

public sealed partial class UseDnaScramblerImplantEvent : InstantActionEvent
{

}