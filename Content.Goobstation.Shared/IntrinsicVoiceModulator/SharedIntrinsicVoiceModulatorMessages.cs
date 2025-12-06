// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Speech;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.IntrinsicVoiceModulator;

[Serializable, NetSerializable]
public enum IntrinsicVoiceModulatorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class IntrinsicVoiceModulatorBoundUserInterfaceState(
    string currentName,
    ProtoId<SpeechVerbPrototype>? currentVerb,
    ProtoId<JobIconPrototype>? jobIcon)
    : BoundUserInterfaceState
{
    public string CurrentName { get; } = currentName;
    public ProtoId<SpeechVerbPrototype>? CurrentVerb { get; } = currentVerb;
    public ProtoId<JobIconPrototype>? JobIcon { get; } = jobIcon;
}

[NetSerializable, Serializable]
public sealed class IntrinsicVoiceModulatorNameChangedMessage(string name) : BoundUserInterfaceMessage
{
    public string Name { get; } = name;
}

[NetSerializable, Serializable]
public sealed class IntrinsicVoiceModulatorJobIconChangedMessage(ProtoId<JobIconPrototype> jobIconProtoId)
    : BoundUserInterfaceMessage
{
    public ProtoId<JobIconPrototype> JobIconProtoId { get; } = jobIconProtoId;
}

[NetSerializable, Serializable]
public sealed class IntrinsicVoicemodulatorVerbChangedMessage(ProtoId<SpeechVerbPrototype>? speechProtoId)
    : BoundUserInterfaceMessage
{
    public ProtoId<SpeechVerbPrototype>? SpeechProtoId { get; } = speechProtoId;
}
