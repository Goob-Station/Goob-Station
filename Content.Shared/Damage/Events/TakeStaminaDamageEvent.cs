// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Damage.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Damage.Events;

/// <summary>
/// The entity is going to be hit,
/// give opportunities to change the damage or other stuff.
/// </summary>
// goobstation - stun resistance. try not to modify this event allat much
public sealed class TakeStaminaDamageEvent : HandledEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;

    public Entity<StaminaComponent>? Target;

    /// <summary>
    /// The multiplier. Generally, try to use *= or /= instead of overwriting.
    /// </summary>
    public float Multiplier = 1;

    /// <summary>
    /// The flat modifier. Generally, try to use += or -= instead of overwriting.
    /// </summary>
    public float FlatModifier = 0;

    public TakeStaminaDamageEvent(Entity<StaminaComponent> target)
    {
        Target = target;
    }
}

public sealed class StaminaDamageMeleeHitEvent(List<(EntityUid Entity, StaminaComponent Component)> hitEntities, Vector2? direction) : EntityEventArgs
{
    public List<(EntityUid Entity, StaminaComponent Component)> HitEntities = hitEntities;

    public Vector2? Direction = direction;
}