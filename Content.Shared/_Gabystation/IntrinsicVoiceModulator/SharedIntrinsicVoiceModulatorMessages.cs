// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.IntrinsicVoiceModulator.Components;
using Content.Shared.Actions;
using Content.Shared.Speech;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.IntrinsicVoiceModulator;

[Serializable, NetSerializable]
public enum IntrinsicVoiceModulatorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class IntrinsicVoiceModulatorBoundUserInterfaceState : BoundUserInterfaceState
{
    public string CurrentName { get; }
    public ProtoId<SpeechVerbPrototype>? CurrentVerb { get; }
    public ProtoId<JobIconPrototype>? JobIcon { get; }

    public IntrinsicVoiceModulatorBoundUserInterfaceState(string currentName, ProtoId<SpeechVerbPrototype>? currentVerb, ProtoId<JobIconPrototype>? jobIcon)
    {
        CurrentName = currentName;
        CurrentVerb = currentVerb;
        JobIcon = jobIcon;
    }
}

[NetSerializable, Serializable]
public sealed class IntrinsicVoiceModulatorNameChangedMessage : BoundUserInterfaceMessage
{
    public string Name { get; }

    public IntrinsicVoiceModulatorNameChangedMessage(string name)
    {
        Name = name;
    }
}

[NetSerializable, Serializable]
public sealed class IntrinsicVoiceModulatorJobIconChangedMessage : BoundUserInterfaceMessage
{
    public ProtoId<JobIconPrototype> JobIconProtoId { get; }

    public IntrinsicVoiceModulatorJobIconChangedMessage(ProtoId<JobIconPrototype> jobIconProtoId)
    {
        JobIconProtoId = jobIconProtoId;
    }
}

[NetSerializable, Serializable]
public sealed class IntrinsicVoicemodulatorVerbChangedMessage : BoundUserInterfaceMessage
{
    public ProtoId<SpeechVerbPrototype>? SpeechProtoId { get; }

    public IntrinsicVoicemodulatorVerbChangedMessage(ProtoId<SpeechVerbPrototype>? speechProtoId)
    {
        SpeechProtoId = speechProtoId;
    }
}
