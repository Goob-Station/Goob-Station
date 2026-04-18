// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Saturates the lungs of nearby respirators.
/// </summary>
public sealed partial class OxygenateNearbySystem : EntityEffectSystem<RespiratorComponent, OxygenateNearby>
{
    protected override void Effect(Entity<RespiratorComponent> entity, ref EntityEffectEvent<OxygenateNearby> args)
    {
        var ev = new OxygenateNearby(args.Effect.Range, args.Effect.Factor);
        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

public sealed partial class OxygenateNearby : EntityEffectBase<OxygenateNearby>
{
    [DataField]
    public float Range = 7;

    [DataField]
    public float Factor = 10f;

    public OxygenateNearby() { }

    public OxygenateNearby(float range, float factor)
    {
        Range = range;
        Factor = factor;
    }

    public override bool ShouldLog => true;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability)); //In due time...
}
