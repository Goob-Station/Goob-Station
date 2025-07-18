// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Anger;
using Content.Shared._Lavaland.Hierophant.Systems;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Tile.Shapes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Hierophant.Actions;

public sealed partial class HierophantBlinkAction : MegafaunaAction
{
    [DataField]
    public EntProtoId DamageTile = "LavalandHierophantChaser";

    [DataField]
    public TileShape TeleportShape;

    [DataField]
    public float BaseAfterDelay = 0.9f;

    [DataField]
    public float MinAfterDelay = 0.6f;

    [DataField]
    public float MaxAfterDelay = 1.5f;

    [DataField("delayMultiplier")]
    public float AfterDelayAgressionMultiplier = 1.5f;

    public override float Invoke(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var hieroSystem = entMan.System<HierophantSystem>();

        if (args.AiComponent.CurrentTarget != null)
            hieroSystem.BlinkToTarget(uid, DamageTile, TeleportShape, args.AiComponent.CurrentTarget.Value);
        else
            hieroSystem.BlinkRandom(uid, DamageTile, TeleportShape);

        var anger = entMan.GetComponentOrNull<AngerComponent>(uid)?.CurrentAnger;
        var delay = Math.Min(anger != null ? Math.Max(anger.Value * AfterDelayAgressionMultiplier, MinAfterDelay) : BaseAfterDelay, MaxAfterDelay);
        return delay;
    }
}
