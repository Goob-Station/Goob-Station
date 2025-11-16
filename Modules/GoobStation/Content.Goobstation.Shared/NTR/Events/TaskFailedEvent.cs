// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskFailedEvent(EntityUid user, int penalty = 4) : EntityEventArgs
{
    public EntityUid User = user;
    public int Penalty = penalty;
}
