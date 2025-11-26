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

using Content.Goobstation.Shared.Overlays;
using Content.Shared.Heretic;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    protected override void OnAscension(Entity<HereticComponent> ent, ref HereticAscensionCosmosEvent args)
    {
        base.OnAscension(ent, ref args);

        var nightVision = _compFactory.GetComponent<NightVisionComponent>();
        nightVision.Color = Color.White;
        nightVision.ActivateSound = null;
        nightVision.DeactivateSound = null;
        nightVision.DrawOverlay = false;

        AddComp(ent, nightVision, true);
    }
}
