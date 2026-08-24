// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.NTR.Events;
public sealed class TaskCompletedEvent : EntityEventArgs
{
    public NtrTaskData Task;

    public TaskCompletedEvent(NtrTaskData task)
    {
        Task = task;
    }
}
