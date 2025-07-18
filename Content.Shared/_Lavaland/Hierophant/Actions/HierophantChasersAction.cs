// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Hierophant.Systems;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Megafauna.Actions;
using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Hierophant.Actions;

public sealed partial class HierophantChasersAction : MegafaunaActionSelector
{
    [DataField]
    public MegafaunaNumberSelector SpeedSelector;

    [DataField]
    public MegafaunaNumberSelector StepsSelector;

    [DataField]
    public MegafaunaNumberSelector AmountSelector;

    [DataField]
    public EntProtoId ChaserTile = "LavalandHierophantChaser";

    [DataField]
    public EntProtoId DamageTile = "LavalandHierophantSquare";

    [DataField]
    public int BaseAdditionalSteps = 10;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var hieroSystem = entMan.System<HierophantSystem>();

        var target = args.AiComponent.CurrentTarget ?? uid;

        var speed = SpeedSelector.Get(args);
        var steps = StepsSelector.GetRounded(args);
        var amount = AmountSelector.GetRounded(args);

        hieroSystem.SpawnChasers(uid, DamageTile, speed, steps, target, amount);

        return DelaySelector.Get(args);
    }
}
