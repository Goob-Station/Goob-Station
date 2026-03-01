// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenobiology.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SlimeExtractComponent : Component
{
    /// <summary>
    ///     Was this extract already used?
    ///     Ideally this should come as a replacement to ReactiveComponent checks in the not so far future.
    /// </summary>
    [DataField] public bool Used = false;

    /// <summary>
    ///     Minor effect that should happen when a luminescent slimeperson presses the funny action button.
    /// </summary>
    [DataField] public List<EntityEffect>? LuminescentMinorEffect = new();

    /// <summary>
    ///     Major effect that should happen when a luminescent slimeperson presses the funny action button.
    /// </summary>
    [DataField] public List<EntityEffect>? LiminescentMajorEffect = new();

    // todo add crossbreeding here (feeding extracts to slimes) (not gonna happen xoon:tm:)
}
