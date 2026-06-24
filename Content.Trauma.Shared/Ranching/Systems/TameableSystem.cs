// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction.Events;
using Content.Shared.NPC.Systems;
using Content.Trauma.Shared.Ranching.Components;
using Robust.Shared.Random;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed partial class TameableSystem : EntitySystem
{
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TameableComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<TameableComponent, InteractionSuccessEvent>(OnSuccess);
        SubscribeLocalEvent<TameableComponent, InteractionFailureEvent>(OnFail);
    }

    private void OnMapInit(Entity<TameableComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.PetsRequired = _random.Next(ent.Comp.MinPetsRequired, ent.Comp.MaxPetsRequired);
        Dirty(ent);
    }

    private void OnSuccess(Entity<TameableComponent> ent, ref InteractionSuccessEvent args)
    {
        ent.Comp.Pets++;
        Dirty(ent);

        if (ent.Comp.Pets < ent.Comp.PetsRequired)
            return;

        if (ent.Comp.ClearFactions)
            _faction.ClearFactions(ent.Owner);

        _faction.AddFaction(ent.Owner, ent.Comp.Faction);

        RemComp<TameableComponent>(ent.Owner);
    }

    private void OnFail(Entity<TameableComponent> ent, ref InteractionFailureEvent args)
    {
        ent.Comp.Pets--;
        Dirty(ent);
    }
}
