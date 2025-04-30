// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Tesla.Components;
using Content.Server.Lightning;
using Content.Shared.Power;
using Robust.Shared.Timing;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// The component changes the visual of an object after it is struck by lightning
/// </summary>
public sealed class LightningSparkingSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightningSparkingComponent, HitByLightningEvent>(OnHitByLightning);
    }

    private void OnHitByLightning(Entity<LightningSparkingComponent> uid, ref HitByLightningEvent args)
    {
        _appearance.SetData(uid.Owner, TeslaCoilVisuals.Lightning, true);
        uid.Comp.LightningEndTime = _gameTiming.CurTime + TimeSpan.FromSeconds(uid.Comp.LightningTime);
        uid.Comp.IsSparking = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LightningSparkingComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.IsSparking)
                continue;

            if (component.LightningEndTime < _gameTiming.CurTime)
            {
                _appearance.SetData(uid, TeslaCoilVisuals.Lightning, false);
                component.IsSparking = false;
            }
        }
    }
}