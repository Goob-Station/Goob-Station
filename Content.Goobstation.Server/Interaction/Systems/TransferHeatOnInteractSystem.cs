// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Interaction.Components;
using Content.Goobstation.Server.Temperature;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Bed.Sleep;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Interaction.Systems;

public sealed class TransferHeatOnInteract : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly GoobTemperatureSystem _goobTemperature = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TransferHeatOnInteractComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<TransferHeatOnInteractComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnActivateInWorld(Entity<TransferHeatOnInteractComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || !ent.Comp.OnActivate)
            return;

        SharedInteract(ent, args.User, args);
    }

    private void OnInteractHand(Entity<TransferHeatOnInteractComponent> ent, ref InteractHandEvent args)
    {
        SharedInteract(ent, args.User, args);
    }

    private void SharedInteract(Entity<TransferHeatOnInteractComponent> ent, EntityUid user, HandledEntityEventArgs args )
    {
        if (args.Handled
            || user == ent.Owner
            || HasComp<SleepingComponent>(ent)
            || !TryComp<TemperatureComponent>(user, out var userTemperature)
            || !TryComp<TemperatureComponent>(ent, out var targetTemperature)
            || !_mobStateSystem.IsAlive(ent))
            return;

        args.Handled = true;

        if (_timing.CurTime < ent.Comp.LastInteractTime + ent.Comp.InteractDelay)
            return;

        ent.Comp.LastInteractTime = _timing.CurTime;

        var targetSpecific = _temperature.GetHeatCapacity(ent);
        var userSpecific = _temperature.GetHeatCapacity(user);

        var temperatureDelta = targetTemperature.CurrentTemperature - userTemperature.CurrentTemperature;
        var energy = temperatureDelta * ent.Comp.TransferRatio * (userSpecific * targetSpecific) / (userSpecific + targetSpecific);

        _temperature.ChangeHeat(ent, -energy);
        _temperature.ChangeHeat(user, energy);

        if (ent.Comp.TransferFireStacks)
            _goobTemperature.TransferFireStacks(user, ent);
    }
}
