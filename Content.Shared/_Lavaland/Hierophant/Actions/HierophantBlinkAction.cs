// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Hierophant.Systems;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Megafauna.Actions;
using Content.Shared._Lavaland.Tile.Shapes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Hierophant.Actions;

public sealed partial class HierophantBlinkAction : MegafaunaActionSelector
{
    [DataField]
    public EntProtoId DamageTile = "LavalandHierophantSquare";

    [DataField]
    public TileShape TeleportShape;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var hieroSystem = entMan.System<HierophantSystem>();

        if (args.AiComponent.CurrentTarget != null)
            hieroSystem.BlinkToTarget(uid, DamageTile, TeleportShape, args.AiComponent.CurrentTarget.Value);
        else
            hieroSystem.BlinkRandom(uid, DamageTile, TeleportShape);

        return DelaySelector.Get(args);
    }
}
