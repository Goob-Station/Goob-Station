// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskFailedEvent : EntityEventArgs
{
    public EntityUid User;
    public int Penalty;

    public TaskFailedEvent(EntityUid user, int penalty = 4)
    {
        User = user;
        Penalty = penalty;
    }
}
