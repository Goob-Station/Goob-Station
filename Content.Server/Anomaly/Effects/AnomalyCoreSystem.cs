// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Cargo.Systems;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Timing;

namespace Content.Server.Anomaly.Effects;

/// <summary>
/// This component reduces the value of the entity during decay
/// </summary>
public sealed class AnomalyCoreSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AnomalyCoreComponent, PriceCalculationEvent>(OnGetPrice);
    }

    private void OnGetPrice(Entity<AnomalyCoreComponent> core, ref PriceCalculationEvent args)
    {
        var timeLeft = core.Comp.DecayMoment - _gameTiming.CurTime;
        var lerp = timeLeft.TotalSeconds / core.Comp.TimeToDecay;
        lerp = Math.Clamp(lerp, 0, 1);

        args.Price = MathHelper.Lerp(core.Comp.EndPrice, core.Comp.StartPrice, lerp);
    }
}