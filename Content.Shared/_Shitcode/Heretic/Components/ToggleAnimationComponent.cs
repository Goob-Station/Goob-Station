// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ToggleAnimationComponent : Component
{
    [DataField]
    public TimeSpan ToggleOnTime = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan ToggleOffTime = TimeSpan.FromSeconds(1.6);

    public CancellationTokenSource? TokenSource;
}

[Serializable, NetSerializable]
public enum ToggleAnimationVisuals : byte
{
    ToggleState,
}

[Serializable, NetSerializable]
public enum ToggleAnimationState : byte
{
    Off,
    TogglingOn,
    On,
    TogglingOff,
}