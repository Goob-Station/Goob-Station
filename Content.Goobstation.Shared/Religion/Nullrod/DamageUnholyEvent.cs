// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Religion.Nullrod;

public sealed class DamageUnholyEvent : EntityEventArgs
{
    public readonly DamageSpecifier OriginalDamage;
    public readonly EntityUid Target;
    public DamageSpecifier Damage;
    public bool Handled = false;
    public EntityUid? Origin;

    public DamageUnholyEvent(EntityUid target,
        DamageSpecifier damage,
        EntityUid? origin = null)
    {
        Target = target;
        OriginalDamage = damage;
        Damage = damage;
        Origin = origin;
    }
}
