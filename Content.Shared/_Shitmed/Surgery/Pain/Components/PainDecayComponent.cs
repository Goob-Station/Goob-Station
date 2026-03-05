// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Components;

// Tracks pain decay state for a nerve system
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PainDecayComponent : Component
{
    // The initial pain value when decay started
    [DataField, AutoNetworkedField]
    public FixedPoint2 InitialPain { get; set; }

    // The time when decay started
    [DataField, AutoNetworkedField]
    public TimeSpan StartTime { get; set; }

    // The duration it should take for pain to decay to zero
    [DataField, AutoNetworkedField]
    public TimeSpan DecayDuration { get; set; }

    // The nerve system this decay is associated with
    [DataField, AutoNetworkedField]
    public EntityUid NerveSystemUid { get; set; }
}

// Network-serializable state for PainDecayComponent
[Serializable, NetSerializable]
public sealed class PainDecayComponentState : ComponentState
{
    public FixedPoint2 InitialPain { get; }
    public TimeSpan StartTime { get; }
    public TimeSpan DecayDuration { get; }
    public NetEntity NerveSystemUid { get; }

    public PainDecayComponentState(FixedPoint2 initialPain, TimeSpan startTime, TimeSpan decayDuration, NetEntity nerveSystemUid)
    {
        InitialPain = initialPain;
        StartTime = startTime;
        DecayDuration = decayDuration;
        NerveSystemUid = nerveSystemUid;
    }
}
