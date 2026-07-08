// SPDX-License-Identifier: MIT

using Content.Shared.Speech;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.IntrinsicVoiceModulator.Components;

[RegisterComponent]
public sealed partial class IntrinsicVoiceModulatorComponent : Component
{
    [DataField]
    public string VoiceName = "";

    [DataField]
    public ProtoId<SpeechVerbPrototype>? SpeechVerbProtoId;

    [DataField]
    public ProtoId<JobIconPrototype>? JobIconProtoId;

    [DataField]
    public string? JobName;
}
