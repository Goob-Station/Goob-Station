// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.IntrinsicVoiceModulator.VoiceMask;

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeJobIconMessage(ProtoId<JobIconPrototype> jobIconProtoId) : BoundUserInterfaceMessage
{
    public ProtoId<JobIconPrototype> JobIconProtoId { get; } = jobIconProtoId;
}
