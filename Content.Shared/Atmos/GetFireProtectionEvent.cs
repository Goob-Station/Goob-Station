// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Shared.Atmos;

/// <summary>
/// Raised on a burning entity to check its fire protection.
/// Damage taken is multiplied by the final amount, but not temperature.
/// TemperatureProtection is needed for that.
/// </summary>
[ByRefEvent]
public sealed class GetFireProtectionEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;

    /// <summary>
    /// What to multiply the fire damage by.
    /// If this is 0 then it's ignored
    /// </summary>
    public float Multiplier;

    /// <summary>
    /// Goobstation - The entity the event was originally raised on.
    /// </summary>
    public readonly EntityUid Target;

    public GetFireProtectionEvent(EntityUid target) // Goobstation
    {
        Multiplier = 1f;
        Target = target; // Goobstation
    }

    /// <summary>
    /// Reduce fire damage taken by a percentage.
    /// </summary>
    public void Reduce(float by)
    {
        if (Multiplier < 0f) // Goobstation, negative multiplier ignores fire AP
            return;
        Multiplier -= by;
        Multiplier = MathF.Max(Multiplier, 0f);
    }
}
