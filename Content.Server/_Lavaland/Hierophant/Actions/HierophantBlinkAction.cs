// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Aggression;
using Content.Server._Lavaland.Megafauna;
using Content.Server._Lavaland.Megafauna.Components;

namespace Content.Server._Lavaland.Hierophant.Actions;

public sealed partial class HierophantBlinkAction : BaseHierophantAction
{
    [DataField]
    public float BaseAfterDelay = 0.9f;

    [DataField]
    public float MinAfterDelay = 0.6f;

    [DataField]
    public float MaxAfterDelay = 1.5f;

    [DataField("delayMultiplier")]
    public float AfterDelayAgressionMultiplier = 1.5f;

    public override float Invoke(MegafaunaAttackBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var aggroSystem = entMan.System<AggressorsSystem>();
        var hieroSystem = entMan.System<HierophantSystem>();

        if (aggroSystem.TryPickTarget(uid, out var target))
            hieroSystem.BlinkToTarget(uid, DamageTile, target.Value);
        else
            hieroSystem.BlinkRandom(uid, DamageTile);

        var anger = entMan.GetComponentOrNull<AggressiveMegafaunaAiComponent>(uid)?.CurrentAnger;
        var delay = Math.Min(anger != null ? Math.Max(anger.Value * AfterDelayAgressionMultiplier, MinAfterDelay) : BaseAfterDelay, MaxAfterDelay);
        return delay;
    }
}
