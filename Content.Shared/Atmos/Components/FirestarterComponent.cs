// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmos.Components;

/// <summary>
/// Lets its owner entity ignite flammables around it and also heal some damage.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedFirestarterSystem))]
public sealed partial class FirestarterComponent : Component
{
    /// <summary>
    /// Radius of objects that will be ignited if flammable.
    /// </summary>
    [DataField]
    public float IgnitionRadius = 4f;

    /// <summary>
    /// The action entity.
    /// </summary>
    [DataField]
    public EntProtoId? FireStarterAction = "ActionFireStarter";

    [DataField] public EntityUid? FireStarterActionEntity;


    /// <summary>
    /// Radius of objects that will be ignited if flammable.
    /// </summary>
    [DataField]
    public SoundSpecifier IgniteSound = new SoundPathSpecifier("/Audio/Magic/rumble.ogg");
}