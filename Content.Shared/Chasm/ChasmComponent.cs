// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Chasm;

/// <summary>
///     Marks a component that will cause entities to fall into them on a step trigger activation
/// </summary>
[NetworkedComponent, RegisterComponent, Access(typeof(ChasmSystem))]
public sealed partial class ChasmComponent : Component
{
    /// <summary>
    ///     Sound that should be played when an entity falls into the chasm
    /// </summary>
    [DataField("fallingSound")]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");
}