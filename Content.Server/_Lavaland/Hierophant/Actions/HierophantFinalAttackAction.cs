// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Megafauna;
using Content.Server._Lavaland.Megafauna.Components;

namespace Content.Server._Lavaland.Hierophant.Actions;

public sealed partial class HierophantFinalAttackAction : BaseHierophantAction
{
    [DataField]
    public int Range = 11;

    [DataField]
    public bool TargetAgressor;

    [DataField]
    public float DefaultTileProb = 0.2f;

    [DataField]
    public float MinTileProb = 0.1f;

    [DataField]
    public float MaxTileProb = 0.4f;

    [DataField("probMultiplier")]
    public float TileProbAngerMultiplier = 0.1f;

    public override float Invoke(MegafaunaThinkBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var hieroSystem = entMan.System<HierophantSystem>();

        var target = uid;
        if (TargetAgressor && args.AiComponent.CurrentTarget != null)
            target = args.AiComponent.CurrentTarget.Value;

        var anger = entMan.GetComponentOrNull<AggressiveMegafaunaAiComponent>(uid)?.CurrentAnger;
        var tileProb =  anger != null ? Math.Clamp(anger.Value * TileProbAngerMultiplier, MinTileProb, MaxTileProb) : 0.2f;

        hieroSystem.SpawnBoxHell(target, DamageTile, tileProb, Range);

        return TileDamageDelay * 2;
    }
}
