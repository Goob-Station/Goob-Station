// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Religion.Nullrod;

[ByRefEvent]
public sealed class DamageUnholyEvent(EntityUid target, EntityUid? origin = null) : EntityEventArgs
{
    public readonly EntityUid Target = target;

    public bool ShouldTakeHoly = false;

    public EntityUid? Origin = origin;
}
