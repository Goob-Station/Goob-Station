using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Content.Shared.Flash;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Hypnoflash;

public sealed class HypnoflashSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HypnoflashComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<HypnoflashComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || ent.Comp.EndTime != null)
            return;
        if (TryComp<LimitedChargesComponent>(ent, out var charges)
            && _sharedCharges.IsEmpty((ent, charges)))
            return;

        _sharedCharges.TryUseCharge((ent, charges));

        args.Handled = true;
        ent.Comp.Activator = args.User;

        var curTime = _timing.CurTime;
        ent.Comp.EndTime = curTime + ent.Comp.FullDuration;

        if (ent.Comp.ProtoOnFlash != null)
            ent.Comp.SpawnEndTime = curTime + ent.Comp.Duration;

        if (ent.Comp.Unremoveable)
        {
            EnsureComp<UnremoveableComponent>(ent);
            _popup.PopupClient(Loc.GetString(ent.Comp.PopupMessage), ent, args.User);
        }

        _audio.PlayPredicted(ent.Comp.ActivationSound, ent, args.User);
        _appearance.SetData(ent, FlashVisuals.Flashing, true); // goida
        Dirty(ent, ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HypnoflashComponent, TransformComponent>();
        var currentTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (comp.SpawnEndTime != null && currentTime >= comp.SpawnEndTime)
            {
                comp.SpawnEndTime = null;
                var activator = comp.Activator;
                comp.Activator = null;
                Dirty(uid, comp);

                if (comp.ProtoOnFlash != null)
                    PredictedSpawnAtPosition(comp.ProtoOnFlash, xform.Coordinates);

                var gamers = new HashSet<EntityUid>();
                _lookup.GetEntitiesInRange(xform.Coordinates, comp.Radius, gamers);

                var validTargets = new List<EntityUid>();

                foreach (var gamer in gamers)
                {
                    if (comp.Blacklist != null && _whitelist.IsValid(comp.Blacklist, gamer))
                        continue;

                    if (comp.Whitelist != null && !_whitelist.IsValid(comp.Whitelist, gamer))
                        continue;

                    if (comp.CheckEyeProt && _inventory.TryGetSlotEntity(gamer, "eyes", out var eyes))
                        continue;

                    validTargets.Add(gamer);
                }

                foreach (var target in validTargets)
                    if (comp.Event != null)
                        RaiseLocalEvent(target, comp.Event);

                if (activator != null && activator.Value.Valid)
                    if (comp.EventOnUser != null)
                        RaiseLocalEvent(activator.Value, comp.EventOnUser);
            }

            if (comp.EndTime != null && currentTime >= comp.EndTime)
            {
                comp.EndTime = null;

                if (comp.Unremoveable)
                    RemCompDeferred<UnremoveableComponent>(uid);

                _appearance.SetData(uid, FlashVisuals.Flashing, false);
                Dirty(uid, comp);
            }
        }
    }
}
