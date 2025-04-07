// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.Stunnable;

public sealed class GetClothingStunModifierEvent : EntityEventArgs
{
    public GetClothingStunModifierEvent(EntityUid target)
    {
        Target = target;
    }

    public EntityUid Target;
    public float Modifier;
}