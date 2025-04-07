// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared.Interaction.Events
{
    /// <summary>
    ///     Event raised directed at a user to see if they can perform a generic interaction.
    /// </summary>
    [ByRefEvent]
    public struct InteractionAttemptEvent(EntityUid uid, EntityUid? target)
    {
        public bool Cancelled;
        public readonly EntityUid Uid = uid;
        public readonly EntityUid? Target = target;
    }

    /// <summary>
    /// Raised to determine whether an entity is conscious to perform an action.
    /// </summary>
    [ByRefEvent]
    public struct ConsciousAttemptEvent(EntityUid uid)
    {
        public bool Cancelled;
        public readonly EntityUid Uid = uid;
    }

    /// <summary>
    ///     Event raised directed at the target entity of an interaction to see if the user is allowed to perform some
    ///     generic interaction.
    /// </summary>
    [ByRefEvent]
    public struct GettingInteractedWithAttemptEvent(EntityUid uid, EntityUid? target)
    {
        public bool Cancelled;
        public readonly EntityUid Uid = uid;
        public readonly EntityUid? Target = target;
    }
}