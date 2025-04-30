// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Flashbang;

public sealed class GetFlashbangedEvent(float range) : EntityEventArgs, IInventoryRelayEvent
{
    public float ProtectionRange = range;

    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
public sealed class AreaFlashEvent(float range, float distance, EntityUid target) : EntityEventArgs
{
    public float Range = range;

    public float Distance = distance;

    public EntityUid Target = target;
}

public sealed class FlashDurationMultiplierEvent : EntityEventArgs, IInventoryRelayEvent
{
    public float Multiplier = 1f;

    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.HEAD | SlotFlags.MASK;
}