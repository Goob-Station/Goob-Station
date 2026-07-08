// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Damage.Components;

namespace Content.Shared._Shitcode.Weapons.Misc;

public sealed class StaminaDamageMeleeHitEvent(List<(EntityUid Entity, StaminaComponent Component)> hitEntities, Vector2? direction) : EntityEventArgs
{
    public List<(EntityUid Entity, StaminaComponent Component)> HitEntities = hitEntities;

    public Vector2? Direction = direction;
}
