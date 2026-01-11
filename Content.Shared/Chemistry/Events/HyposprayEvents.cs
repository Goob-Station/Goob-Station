// SPDX-FileCopyrightText: 2024 Nikolai Korolev <korolevns98@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Chemistry.Hypospray.Events;

public abstract partial class BeforeHyposprayInjectsTargetEvent : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;
    public EntityUid EntityUsingHypospray;
    public readonly EntityUid Hypospray;
    public EntityUid TargetGettingInjected;
    public string? InjectMessageOverride;

    public BeforeHyposprayInjectsTargetEvent(EntityUid user, EntityUid hypospray, EntityUid target)
    {
        EntityUsingHypospray = user;
        Hypospray = hypospray;
        TargetGettingInjected = target;
        InjectMessageOverride = null;
    }
}

/// <summary>
///     This event is raised on the user using the hypospray before the hypospray is injected.
///     The event is triggered on the user and all their clothing.
/// </summary>
public sealed class SelfBeforeHyposprayInjectsEvent : BeforeHyposprayInjectsTargetEvent
{
    public SelfBeforeHyposprayInjectsEvent(EntityUid user, EntityUid hypospray, EntityUid target) : base(user, hypospray, target) { }
}

/// <summary>
///     This event is raised on the target before the hypospray is injected.
///     The event is triggered on the target itself and all its clothing.
/// </summary>
public sealed class TargetBeforeHyposprayInjectsEvent : BeforeHyposprayInjectsTargetEvent
{
    public TargetBeforeHyposprayInjectsEvent(EntityUid user, EntityUid hypospray, EntityUid target) : base(user, hypospray, target) { }
}

/// <summary>
///     This even is raised on the hypospray itself before it injects.
///     Goobstation
/// </summary>
public sealed class BeforeHyposprayInjectsEvent : BeforeHyposprayInjectsTargetEvent
{
    public BeforeHyposprayInjectsEvent(EntityUid user, EntityUid hypospray, EntityUid target) : base(user, hypospray, target) { }
}


/// <summary>
///     This event is raised on the hypospray before it draws.
///     Goobstation
/// </summary>
public sealed partial class BeforeHyposprayDrawEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// Entity that used the hypospray.
    /// </summary>
    public EntityUid User;

    /// <summary>
    /// Entity that is being drawn from.
    /// </summary>
    public EntityUid Target;

    /// <summary>
    /// Solution that is being drawn from.
    /// </summary>
    public Entity<SolutionComponent> Soln;

    public BeforeHyposprayDrawEvent(EntityUid user, EntityUid target, Entity<SolutionComponent> soln)
    {
        User = user;
        Target = target;
        Soln = soln;
    }
}
