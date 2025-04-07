// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

[Serializable, NetSerializable]
public sealed class IFFShowVesselMessage : BoundUserInterfaceMessage
{
    public bool Show;
}