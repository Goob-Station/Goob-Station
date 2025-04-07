// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Shared.Electrocution
{
    public sealed class ElectrocutionAttemptEvent : CancellableEntityEventArgs, IInventoryRelayEvent
    {
        public SlotFlags TargetSlots { get; }

        public readonly EntityUid TargetUid;
        public readonly EntityUid? SourceUid;
        public float SiemensCoefficient = 1f;

        public ElectrocutionAttemptEvent(EntityUid targetUid, EntityUid? sourceUid, float siemensCoefficient, SlotFlags targetSlots)
        {
            TargetUid = targetUid;
            TargetSlots = targetSlots;
            SourceUid = sourceUid;
            SiemensCoefficient = siemensCoefficient;
        }
    }

    public sealed class ElectrocutedEvent : EntityEventArgs
    {
        public readonly EntityUid TargetUid;
        public readonly EntityUid? SourceUid;
        public readonly float SiemensCoefficient;
        public readonly float? ShockDamage = null; // Goobstation

        public ElectrocutedEvent(EntityUid targetUid, EntityUid? sourceUid, float siemensCoefficient, float shockDamage) // Goobstation
        {
            TargetUid = targetUid;
            SourceUid = sourceUid;
            SiemensCoefficient = siemensCoefficient;
            ShockDamage = shockDamage; // Goobstation
        }
    }
}