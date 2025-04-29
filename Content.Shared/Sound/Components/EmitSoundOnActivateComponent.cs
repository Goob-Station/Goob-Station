// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Sound.Components;

/// <summary>
/// Simple sound emitter that emits sound on ActivateInWorld
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EmitSoundOnActivateComponent : BaseEmitSoundComponent
{
    /// <summary>
    ///     Whether or not to mark an interaction as handled after playing the sound. Useful if this component is
    ///     used to play sound for some other component with activation functionality.
    /// </summary>
    /// <remarks>
    ///     If false, you should be confident that the interaction will also be handled by some other system, as
    ///     otherwise this might enable sound spamming, as use-delays are only initiated if the interaction was
    ///     handled.
    /// </remarks>
    [DataField("handle")]
    public bool Handle = true;
}