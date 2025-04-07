// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Item;

/// <summary>
///     Raised on a *mob* when it tries to pickup something
/// </summary>
public sealed class PickupAttemptEvent : BasePickupAttemptEvent
{
    public PickupAttemptEvent(EntityUid user, EntityUid item) : base(user, item) { }
}

/// <summary>
///     Raised directed at entity being picked up when someone tries to pick it up
/// </summary>
public sealed class GettingPickedUpAttemptEvent : BasePickupAttemptEvent
{
    public GettingPickedUpAttemptEvent(EntityUid user, EntityUid item) : base(user, item) { }
}

[Virtual]
public class BasePickupAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid User;
    public readonly EntityUid Item;

    public BasePickupAttemptEvent(EntityUid user, EntityUid item)
    {
        User = user;
        Item = item;
    }
}