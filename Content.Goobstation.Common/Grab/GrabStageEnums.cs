// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace  Content.Goobstation.Common.Grab;

[Serializable, NetSerializable]
public enum GrabStage
{
    No = 0,
    Soft = 1,
    Hard = 2,
    Suffocate = 3,
}

public enum GrabStageDirection
{
    Increase,
    Decrease,
}

public enum GrabResistResult
{
    TooSoon,
    Failed,
    Succeeded,
}

public enum GrabAttemptResult
{
    Succeeded,
    OnCooldown,
    Failed,
}
