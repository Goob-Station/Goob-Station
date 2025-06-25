// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Aggression;
using Content.Server._Lavaland.Megafauna;
using Content.Server._Lavaland.Megafauna.Components;

namespace Content.Server._Lavaland.Hierophant.Actions;

public sealed partial class HierophantSquareAction : BaseHierophantAction
{
    public override float Invoke(MegafaunaAttackBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var aggroSystem = entMan.System<AggressorsSystem>();
        var hieroSystem = entMan.System<HierophantSystem>();

        var anger = entMan.GetComponentOrNull<AggressiveMegafaunaAiComponent>(uid)?.CurrentAnger;
        aggroSystem.TryPickTarget(uid, out var target);
        target ??= uid;

        var radius = anger != null ? Math.Max((int) MathF.Round(anger.Value * 0.6f), 1) : 5;
        hieroSystem.SetupSquarePattern(target.Value, DamageTile, TileDamageDelay, radius);

        return radius * TileDamageDelay;
    }
}
