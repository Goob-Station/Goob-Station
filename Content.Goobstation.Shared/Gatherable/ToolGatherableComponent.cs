// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gatherable;

/// <summary>
/// Requires <c>GatherableComponent</c> but cannot be used with <c>GatherableSoundComponent</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedToolGatherableSystem))]
public sealed partial class ToolGatherableComponent : Component
{
    /// <summary>
    /// The tool quality to use for the gather doafter.
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype> ToolQuality = "Mining";

    /// <summary>
    /// How long it takes to gather using a tool.
    /// </summary>
    [DataField]
    public TimeSpan GatherTime = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Sound to play when gathering.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Effects/break_stone.ogg")
    {
        Params = AudioParams.Default
            .WithVolume(-3f)
    };
}
