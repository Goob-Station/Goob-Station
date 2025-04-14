// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Shared.Interaction
{
    public sealed class InteractHandEventArgs : EventArgs, ITargetedInteractEventArgs
    {
        public InteractHandEventArgs(EntityUid user, EntityUid target)
        {
            User = user;
            Target = target;
        }

        public EntityUid User { get; }
        public EntityUid Target { get; }
    }

    /// <summary>
    ///     Raised directed on a target entity when it is interacted with by a user with an empty hand.
    /// </summary>
    [PublicAPI]
    public sealed class InteractHandEvent : HandledEntityEventArgs, ITargetedInteractEventArgs
    {
        /// <summary>
        ///     Entity that triggered the interaction.
        /// </summary>
        public EntityUid User { get; }

        /// <summary>
        ///     Entity that was interacted on.
        /// </summary>
        public EntityUid Target { get; }

        public InteractHandEvent(EntityUid user, EntityUid target)
        {
            User = user;
            Target = target;
        }
    }

    /// <summary>
    /// Raised on the user before interacting on an entity with bare hand.
    /// Interaction is cancelled if this event is handled, so set it to true if you do custom interaction logic.
    /// </summary>
    public sealed class BeforeInteractHandEvent : HandledEntityEventArgs
    {
        public EntityUid Target { get; }

        public BeforeInteractHandEvent(EntityUid target)
        {
            Target = target;
        }
    }
}