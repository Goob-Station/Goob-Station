// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Content.Shared.Inventory; // Goobstation
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Melee.Events
{
    [Serializable, NetSerializable]
    public abstract class AttackEvent : EntityEventArgs
    {
        /// <summary>
        /// Coordinates being attacked.
        /// </summary>
        public readonly NetCoordinates Coordinates;

        protected AttackEvent(NetCoordinates coordinates)
        {
            Coordinates = coordinates;
        }
    }

    /// <summary>
    ///     Event raised on entities that have been attacked.
    /// </summary>
    public sealed class AttackedEvent : EntityEventArgs, IInventoryRelayEvent // Goobstation
    {
        SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET; // Goobstation
        /// <summary>
        ///     Entity used to attack, for broadcast purposes.
        /// </summary>
        public EntityUid Used { get; }

        /// <summary>
        ///     Entity that triggered the attack.
        /// </summary>
        public EntityUid User { get; }

        /// <summary>
        ///     The original location that was clicked by the user.
        /// </summary>
        public EntityCoordinates ClickLocation { get; }

        public DamageSpecifier BonusDamage = new();

        public AttackedEvent(EntityUid used, EntityUid user, EntityCoordinates clickLocation)
        {
            Used = used;
            User = user;
            ClickLocation = clickLocation;
        }
    }

    // Goobstation start
    public sealed class BeforeHarmfulActionEvent(EntityUid user, HarmfulActionType type) : CancellableEntityEventArgs
    {
        public EntityUid User { get; } = user;

        public HarmfulActionType Type { get; } = type;
    }

    public enum HarmfulActionType : byte
    {
        Harm,
        Disarm,
        Grab,
    }
    // Goobstation end
}
