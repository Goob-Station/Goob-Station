// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.MalfAI;

/// <summary>
/// Tracks state for the Malfunctioning AI voice modulator ability.
/// Modulates speaker name, job icon, and speech accent.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfVoiceModulatorComponent : Component
{
    /// <summary>
    /// Whether voice modulation is active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    /// The disguised speaker name used when speaking or broadcasting over radio.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string VoiceName = string.Empty;

    /// <summary>
    /// Prototype ID of the job icon to spoof over radio chat.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<JobIconPrototype>? JobIconProtoId;

    /// <summary>
    /// Localized title of the spoofed job.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? JobName;

    /// <summary>
    /// Prototype ID of the selected job (e.g. "Captain") from the HoP list.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? JobProtoId;

    /// <summary>
    /// Registration name of the accent component (e.g. "FrenchAccent") or null for none.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Accent;

    /// <summary>
    /// Currently added accent component registration name on the server, tracked for clean removal.
    /// </summary>
    [DataField]
    public string? ActiveAccentComponent;
}
