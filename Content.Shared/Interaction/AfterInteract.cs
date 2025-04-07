// SPDX-FileCopyrightText: 2020 BlueberryShortcake <rubetskoy234@mail.ru>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 DmitriyRubetskoy <75271456+DmitriyRubetskoy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;
using Robust.Shared.Map;


namespace Content.Shared.Interaction
{
    [PublicAPI]
    public abstract class InteractEvent : HandledEntityEventArgs
    {
        /// <summary>
        ///     Entity that triggered the interaction.
        /// </summary>
        public EntityUid User { get; }

        /// <summary>
        ///     Entity that the user used to interact.
        /// </summary>
        public EntityUid Used { get; }

        /// <summary>
        ///     Entity that was interacted on. This can be null if there was no target (e.g., clicking on tiles).
        /// </summary>
        public EntityUid? Target { get; }

        /// <summary>
        ///     Location that the user clicked outside of their interaction range.
        /// </summary>
        public EntityCoordinates ClickLocation { get; }

        /// <summary>
        /// Is the click location in range without obstructions?
        /// </summary>
        public bool CanReach { get; }

        public InteractEvent(EntityUid user, EntityUid used, EntityUid? target,
            EntityCoordinates clickLocation, bool canReach)
        {
            User = user;
            Used = used;
            Target = target;
            ClickLocation = clickLocation;
            CanReach = canReach;
        }
    }

    /// <summary>
    ///     Raised directed on the used object when clicking on another object and no standard interaction occurred.
    ///     Used for low-priority interactions facilitated by the used entity.
    /// </summary>
    public sealed class AfterInteractEvent : InteractEvent
    {
        public AfterInteractEvent(EntityUid user, EntityUid used, EntityUid? target,
            EntityCoordinates clickLocation, bool canReach) : base(user, used, target, clickLocation, canReach)
        { }
    }

    /// <summary>
    ///     Raised directed on the target when clicking on another object and no standard interaction occurred. Used for
    ///     low-priority interactions facilitated by the target entity.
    /// </summary>
    public sealed class AfterInteractUsingEvent : InteractEvent
    {
        public AfterInteractUsingEvent(EntityUid user, EntityUid used, EntityUid? target,
            EntityCoordinates clickLocation, bool canReach) : base(user, used, target, clickLocation, canReach)
        { }
    }
}