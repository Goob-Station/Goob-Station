// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SecondSkin;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Flash;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    protected override void SubscribeRust()
    {
        base.SubscribeRust();

        SubscribeLocalEvent<RustbringerComponent, FlashAttemptEvent>(OnFlashAttempt);

        SubscribeLocalEvent<LeechingWalkComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<LeechingWalkComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out DisgustComponent? disgust))
            return;

        disgust.AccumulationMultiplier = 0f;
    }

    private void OnFlashAttempt(Entity<RustbringerComponent> ent, ref FlashAttemptEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Cancelled = true;
    }
}
