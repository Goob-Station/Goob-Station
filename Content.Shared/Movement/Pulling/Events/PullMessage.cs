// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Movement.Pulling.Events;

public abstract class PullMessage : EntityEventArgs
{
    public readonly EntityUid PullerUid;
    public readonly EntityUid PulledUid;

    protected PullMessage(EntityUid pullerUid, EntityUid pulledUid)
    {
        PullerUid = pullerUid;
        PulledUid = pulledUid;
    }
}