// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Audio;
using Robust.Shared.Audio;

namespace Content.Server.Gatherable.Components;

/// <summary>
/// Plays the specified sound when this entity is gathered.
/// </summary>
[RegisterComponent, Access(typeof(GatherableSystem))]
public sealed partial class SoundOnGatherComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/break_stone.ogg")
    {
        Params = AudioParams.Default
            .WithVariation(SharedContentAudioSystem.DefaultVariation)
            .WithVolume(-3f),
    };
}