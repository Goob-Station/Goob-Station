// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ScyronX <166930367+ScyronX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Components;

namespace Content.Shared.Damage.Events;

/// <summary>
/// The components in the list are going to be hit,
/// give opportunities to change the damage or other stuff.
/// </summary>
public sealed class StaminaMeleeHitEvent : HandledEntityEventArgs
{
    /// <summary>
    /// List of hit stamina components.
    /// </summary>
    public List<(EntityUid Entity, StaminaComponent Component)> HitList;

    /// <summary>
    /// The multiplier. Generally, try to use *= or /= instead of overwriting.
    /// </summary>
    public float Multiplier = 1;

    /// <summary>
    /// The flat modifier. Generally, try to use += or -= instead of overwriting.
    /// </summary>
    public float FlatModifier = 0;

    public StaminaMeleeHitEvent(List<(EntityUid Entity, StaminaComponent Component)> hitList)
    {
        HitList = hitList;
    }
}