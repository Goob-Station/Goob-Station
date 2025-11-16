// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.MartialArts;

public sealed class SaveLastAttacksEvent : EntityEventArgs;

public sealed class ResetLastAttacksEvent(bool dirty = true) : EntityEventArgs
{
    public bool Dirty = dirty;
}

public sealed class LoadLastAttacksEvent(bool dirty = true) : EntityEventArgs
{
    public bool Dirty = dirty;
}

[ByRefEvent]
public record struct GetPerformedAttackTypesEvent(List<ComboAttackType>? AttackTypes = null);
