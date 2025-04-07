// SPDX-FileCopyrightText: 2020 Tyler Young <tyler.young@impromptu.ninja>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Collections;
using Robust.Shared.Audio.Midi;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Instruments;

[NetworkedComponent]
[Access(typeof(SharedInstrumentSystem))]
public abstract partial class SharedInstrumentComponent : Component
{
    [ViewVariables]
    public bool Playing { get; set; }

    [DataField("program"), ViewVariables(VVAccess.ReadWrite)]
    public byte InstrumentProgram { get; set; }

    [DataField("bank"), ViewVariables(VVAccess.ReadWrite)]
    public byte InstrumentBank { get; set; }

    [DataField("allowPercussion"), ViewVariables(VVAccess.ReadWrite)]
    public bool AllowPercussion { get; set; }

    [DataField("allowProgramChange"), ViewVariables(VVAccess.ReadWrite)]
    public bool AllowProgramChange { get ; set; }

    [DataField("respectMidiLimits"), ViewVariables(VVAccess.ReadWrite)]
    public bool RespectMidiLimits { get; set; } = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Master { get; set; } = null;

    [ViewVariables]
    public BitArray FilteredChannels { get; set; } = new(RobustMidiEvent.MaxChannels, true);
}

[Serializable, NetSerializable]
public sealed class InstrumentComponentState : ComponentState
{
    public bool Playing;

    public byte InstrumentProgram;

    public byte InstrumentBank;

    public bool AllowPercussion;

    public bool AllowProgramChange;

    public bool RespectMidiLimits;

    public NetEntity? Master;

    public BitArray FilteredChannels = default!;
}


/// <summary>
///     This message is sent to the client to completely stop midi input and midi playback.
/// </summary>
[Serializable, NetSerializable]
public sealed class InstrumentStopMidiEvent : EntityEventArgs
{
    public NetEntity Uid { get; }

    public InstrumentStopMidiEvent(NetEntity uid)
    {
        Uid = uid;
    }
}

/// <summary>
///     Send from the client to the server to set a master instrument.
/// </summary>
[Serializable, NetSerializable]
public sealed class InstrumentSetMasterEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public NetEntity? Master { get; }

    public InstrumentSetMasterEvent(NetEntity uid, NetEntity? master)
    {
        Uid = uid;
        Master = master;
    }
}

/// <summary>
///     Send from the client to the server to set a master instrument channel.
/// </summary>
[Serializable, NetSerializable]
public sealed class InstrumentSetFilteredChannelEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public int Channel { get; }
    public bool Value { get; }

    public InstrumentSetFilteredChannelEvent(NetEntity uid, int channel, bool value)
    {
        Uid = uid;
        Channel = channel;
        Value = value;
    }
}

/// <summary>
///     This message is sent to the client to start the synth.
/// </summary>
[Serializable, NetSerializable]
public sealed class InstrumentStartMidiEvent : EntityEventArgs
{
    public NetEntity Uid { get; }

    public InstrumentStartMidiEvent(NetEntity uid)
    {
        Uid = uid;
    }
}

/// <summary>
///     This message carries a MidiEvent to be played on clients.
/// </summary>
[Serializable, NetSerializable]
public sealed class InstrumentMidiEventEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public RobustMidiEvent[] MidiEvent { get; }

    public InstrumentMidiEventEvent(NetEntity uid, RobustMidiEvent[] midiEvent)
    {
        Uid = uid;
        MidiEvent = midiEvent;
    }
}

[NetSerializable, Serializable]
public enum InstrumentUiKey
{
    Key,
}