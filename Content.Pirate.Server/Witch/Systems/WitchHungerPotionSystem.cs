using Content.Pirate.Shared.Witch.Components;
using Content.Goobstation.Common.Devour;
using Content.Goobstation.Maths.FixedPoint;
using System.Linq;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Devour;
using Content.Shared.Devour.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Body.Events;
using Content.Shared.Gibbing.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.Witch.Systems;

public sealed class WitchHungerPotionSystem : EntitySystem
{
    private const string StasiziumReagentId = "Stasizium";

    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly NPCRetaliationSystem _retaliation = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WitchHungerPotionComponent, DevourActionEvent>(OnDevourAction, before: [typeof(DevourSystem)]);
        SubscribeLocalEvent<WitchHungerPotionComponent, DevourDoAfterEvent>(OnDevourDoAfter, after: [typeof(DevourSystem)]);
        SubscribeLocalEvent<WitchHungerPotionComponent, BeingGibbedEvent>(OnBeingGibbed);
        SubscribeLocalEvent<WitchHungerPotionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WitchHungerPotionComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WitchHungerPotionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(comp.Interval);

            if (TryComp<HungerComponent>(uid, out var hunger))
                _hunger.ModifyHunger(uid, comp.HungerDelta, hunger);

            var sawFood = false;
            foreach (var nearby in _lookup.GetEntitiesInRange(uid, comp.Radius, LookupFlags.Dynamic))
            {
                if (nearby != uid &&
                    TryComp<MobStateComponent>(nearby, out var state) &&
                    state.CurrentState == MobState.Alive)
                {
                    sawFood = true;
                }

                if (nearby == uid || !TryComp<NPCRetaliationComponent>(nearby, out var retaliation))
                    continue;

                _retaliation.TryRetaliate((nearby, retaliation), uid, true);
            }

            if (sawFood)
                _popup.PopupEntity(Loc.GetString("witch-hunger-potion-popup-devour"), uid, uid, PopupType.MediumCaution);
        }
    }

    private void OnDevourAction(Entity<WitchHungerPotionComponent> ent, ref DevourActionEvent args)
    {
        if (args.Handled
            || !TryComp<DevourerComponent>(ent.Owner, out var devourer)
            || _whitelist.IsWhitelistFailOrNull(devourer.Whitelist, args.Target)
            || !TryComp<MobStateComponent>(args.Target, out var state)
            || state.CurrentState != MobState.Alive)
            return;

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("witch-hunger-potion-popup-lunge"), ent.Owner, ent.Owner, PopupType.MediumCaution);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.AliveDevourTime, new DevourDoAfterEvent(), ent.Owner, target: args.Target, used: ent.Owner)
        {
            BreakOnMove = true,
        });
    }

    private void OnMobStateChanged(Entity<WitchHungerPotionComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is not (MobState.Critical or MobState.Dead))
            return;

        RegurgitateVictims(ent.Owner, true);
    }

    private void OnBeingGibbed(Entity<WitchHungerPotionComponent> ent, ref BeingGibbedEvent args)
    {
        RegurgitateVictims(ent.Owner, true);
    }

    private void OnShutdown(Entity<WitchHungerPotionComponent> ent, ref ComponentShutdown args)
    {
        RegurgitateVictims(ent.Owner, true);
    }

    private void OnDevourDoAfter(Entity<WitchHungerPotionComponent> ent, ref DevourDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Args.Target is not { } target
            || !TryComp<DevourerComponent>(ent.Owner, out var devourer)
            || !devourer.Stomach.ContainedEntities.Contains(target)
            || !TryComp<BloodstreamComponent>(ent.Owner, out var bloodstream))
            return;

        var boost = new Solution();
        boost.AddReagent(StasiziumReagentId, FixedPoint2.New(ent.Comp.StasiziumPerVictim));
        _bloodstream.TryAddToBloodstream((ent.Owner, bloodstream), boost);
    }

    private void RegurgitateVictims(EntityUid uid, bool showPopup = false)
    {
        if (!TryComp<DevourerComponent>(uid, out var devourer)
            || devourer.Stomach.ContainedEntities.Count == 0)
            return;

        foreach (var victim in devourer.Stomach.ContainedEntities.ToArray())
        {
            RemComp<PreventSelfRevivalComponent>(victim);
        }

        _container.EmptyContainer(devourer.Stomach, force: true, destination: Transform(uid).Coordinates);
        if (showPopup)
            _popup.PopupEntity(Loc.GetString("witch-hunger-potion-popup-regurgitate"), uid, uid, PopupType.LargeCaution);
    }
}
