// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;

namespace Content.Goobstation.Client.MartialArts;

public sealed class MartialArtsSystem : SharedMartialArtsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanPerformComboComponent, GetPerformedAttackTypesEvent>(OnGetAttackTypes);
    }

    private void OnGetAttackTypes(Entity<CanPerformComboComponent> ent, ref GetPerformedAttackTypesEvent args)
    {
        args.AttackTypes = ent.Comp.LastAttacks;
    }
}
