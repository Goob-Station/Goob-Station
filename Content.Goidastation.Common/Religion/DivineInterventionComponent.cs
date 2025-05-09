// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goidastation.Common.Religion;

[RegisterComponent, NetworkedComponent]
public sealed partial class DivineInterventionComponent : Component
{
    /// <summary>
    /// Which sound to play on spell denial.
    /// </summary>
    [DataField]
    public SoundSpecifier DenialSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

    /// <summary>
    /// Which effect to display.
    /// </summary>
    [DataField]
    public EntProtoId EffectProto = "EffectSpark";
}
