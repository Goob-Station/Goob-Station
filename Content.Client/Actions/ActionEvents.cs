// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions.Components;
using static Robust.Shared.Input.Binding.PointerInputCmdHandler;

namespace Content.Client.Actions;

/// <summary>
///     This event is raised when a user clicks on an empty action slot. Enables other systems to fill this slot.
/// </summary>
public sealed class FillActionSlotEvent : EntityEventArgs
{
    public EntityUid? Action;
}

/// <summary>
/// Client-side event used to attempt to trigger a targeted action.
/// This only gets raised if the has <see cref="TargetActionComponent">.
/// Handlers must set <c>Handled</c> to true, then if the action has been performed,
/// i.e. a target is found, then FoundTarget must be set to true.
/// </summary>
[ByRefEvent]
public record struct ActionTargetAttemptEvent(
    PointerInputCmdArgs Input,
    Entity<ActionsComponent> User,
    ActionComponent Action,
    bool Handled = false,
    bool FoundTarget = false);
