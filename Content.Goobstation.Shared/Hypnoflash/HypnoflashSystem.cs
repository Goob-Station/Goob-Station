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

    private void OnUseInHand(EntityUid uid, HypnoflashComponent comp, UseInHandEvent args)
    {
        if (args.Handled || comp.EndTime != null)
            return;
        if (TryComp<LimitedChargesComponent>(uid, out var charges)
            && _sharedCharges.IsEmpty((uid, charges)))
            return;

        _sharedCharges.TryUseCharge((uid, charges));

        args.Handled = true;
        comp.Activator = args.User;

        var curTime = _timing.CurTime;
        comp.EndTime = curTime + comp.FullDuration;

        if (comp.ProtoOnFlash != null)
            comp.SpawnEndTime = curTime + comp.Duration;

        if (comp.Unremoveable)
        {
            EnsureComp<UnremoveableComponent>(uid);
            _popup.PopupClient(Loc.GetString(comp.PopupMessage), uid, args.User);
        }

        _audio.PlayPredicted(comp.ActivationSound, uid, args.User);
        _appearance.SetData(uid, FlashVisuals.Flashing, true); // goida
        Dirty(uid, comp);
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
                {
                    if (comp.Event != null)
                        RaiseLocalEvent(target, comp.Event);
                    else
                        RaiseLocalEvent(target, new HypnoflashedEvent(GetNetEntity(uid)));
                }

                if (activator != null && activator.Value.Valid)
                {
                    if (comp.EventOnUser != null)
                        RaiseLocalEvent(activator.Value, comp.EventOnUser);
                    else
                        RaiseLocalEvent(activator.Value, new HypnoflashActivatedEvent(GetNetEntity(uid)));
                }
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
