// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Stores the Malf AI voice modulator overrides (radio job icon disguise).
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiVoiceComponent : Component
{
    /// <summary>
    /// The job icon displayed next to the name on radio, or null to keep the AI icon.
    /// </summary>
    [DataField]
    public ProtoId<JobIconPrototype>? JobIcon;
}
