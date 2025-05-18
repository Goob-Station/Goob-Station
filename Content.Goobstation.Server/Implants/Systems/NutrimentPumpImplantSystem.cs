// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class NutrimentPumpImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(1f);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ImplantedComponent>();
        while (query.MoveNext(out var uid, out var implantedComponent))
        {
            foreach (var containedEntity in implantedComponent.ImplantContainer.ContainedEntities)
            {
                if (!TryComp<NutrimentPumpImplantComponent>(containedEntity, out var pumpImplant))
                    continue;

                if (pumpImplant.NextExecutionTime > _gameTiming.CurTime)
                    continue;

                if (!TryComp<HungerComponent>(uid, out var hungerComponent))
                    continue;
                _hunger.ModifyHunger(uid, pumpImplant.FoodRate, hungerComponent);

                if (!TryComp<ThirstComponent>(uid, out var thirstComponent))
                    continue;
                _thirst.ModifyThirst(uid, thirstComponent, pumpImplant.DrinkRate); // why the fuck is the order of arguments different for ModifyThirst????

                pumpImplant.NextExecutionTime = _gameTiming.CurTime + ExecutionInterval;
            }
        }
    }
}
