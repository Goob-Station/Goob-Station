// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Abbey Armbruster <abbeyjarmb@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.ComponentTrees;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Serialization;

namespace Content.Shared.Audio;

[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedAmbientSoundSystem))]
public sealed partial class AmbientSoundComponent : Component, IComponentTreeEntry<AmbientSoundComponent>
{
    [DataField("enabled", readOnly: true)]
    [ViewVariables(VVAccess.ReadWrite)] // only for map editing
    public bool Enabled { get; set; } = true;

    [DataField("sound", required: true), ViewVariables(VVAccess.ReadWrite)] // only for map editing
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// How far away this ambient sound can potentially be heard.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] // only for map editing
    [DataField("range")]
    public float Range = 2f;

    public Vector2 RangeVector => new Vector2(Range, Range);

    /// <summary>
    /// Applies this volume to the sound being played.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] // only for map editing
    [DataField("volume")]
    public float Volume = -10f;

    public EntityUid? TreeUid { get; set; }

    public DynamicTree<ComponentTreeEntry<AmbientSoundComponent>>? Tree { get; set; }

    public bool AddToTree => Enabled;

    public bool TreeUpdateQueued { get; set; }
}

[Serializable, NetSerializable]
public sealed class AmbientSoundComponentState : ComponentState
{
    public bool Enabled { get; init; }
    public float Range { get; init; }
    public float Volume { get; init; }
    public SoundSpecifier Sound { get; init; } = default!;
}