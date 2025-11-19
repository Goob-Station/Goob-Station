// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
///     Component responsible for Fleshmend's passive effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FleshmendComponent : Component
{
    /// <summary>
    ///     Delay between healing ticks.
    /// </summary>
    public TimeSpan UpdateTimer = default!;
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    public EntityUid? SoundSource; // used to stop the passive sound (if it exists)

    [DataField]
    public SoundSpecifier? PassiveSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/fleshmend_sfx.ogg");

    /// <summary>
    ///     Used in the case that someone wants to change the default effect (e.g if they are adding a fleshmend-esque passive to some random mob or ability)
    /// </summary>
    [DataField]
    public string EffectState;
    [DataField]
    public ResPath ResPath;

    [DataField]
    public bool DoVisualEffect = true;

    [DataField]
    public bool IgnoreFire = false; // for whatever reason

    [DataField]
    public float BruteHeal = -10f;

    [DataField]
    public float BurnHeal = -5f;

    [DataField]
    public float AsphyxHeal = -2f;

    [DataField]
    public float BleedingAdjust = -2.5f;

    [DataField]
    public float BloodLevelAdjust = 10f;
}
