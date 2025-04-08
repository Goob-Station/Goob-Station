// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Actions.Events;

public sealed class DisarmAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid TargetUid;
    public readonly EntityUid DisarmerUid;
    public readonly EntityUid? TargetItemInHandUid;

    public DisarmAttemptEvent(EntityUid targetUid, EntityUid disarmerUid, EntityUid? targetItemInHandUid = null)
    {
        TargetUid = targetUid;
        DisarmerUid = disarmerUid;
        TargetItemInHandUid = targetItemInHandUid;
    }
}