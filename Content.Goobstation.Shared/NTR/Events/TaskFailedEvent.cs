// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class TaskFailedEvent(EntityUid user, int penalty = 4) : EntityEventArgs
{
    public EntityUid User = user;
    public int Penalty = penalty;
}
