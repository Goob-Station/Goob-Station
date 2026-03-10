using Content.Shared.Interaction.Events;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Content.Shared.Flash;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HypnoflashComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, HypnoflashComponent comp, UseInHandEvent args)
    {
        if (args.Handled || comp.EndTime != null)
            return;

        args.Handled = true;
        comp.Activator = args.User;

        var curTime = _timing.CurTime;
        comp.EndTime = curTime + comp.Duration;

        if (comp.ProtoOnFlash != null)
            comp.SpawnEndTime = curTime + comp.SpawnDelay;

        if (comp.Unremoveable)
        {
            EnsureComp<UnremoveableComponent>(uid);
            _popup.PopupClient(Loc.GetString(comp.PopupMessage), uid, args.User);
        }

        _audio.PlayEntity(comp.ActivationSound, uid, uid);
        _appearance.SetData(uid, FlashVisuals.Flashing, true); // goida
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

                if (comp.ProtoOnFlash != null)
                    Spawn(comp.ProtoOnFlash, xform.Coordinates);
            }

            if (comp.EndTime != null && currentTime >= comp.EndTime)
            {
                comp.EndTime = null;
                var activator = comp.Activator;
                comp.Activator = null;

                if (comp.Unremoveable)
                    RemComp<UnremoveableComponent>(uid);

                _appearance.SetData(uid, FlashVisuals.Flashing, false);

                var gamers = new HashSet<EntityUid>();
                _lookup.GetEntitiesInRange(xform.Coordinates, comp.Radius, gamers);

                var validTargets = new List<EntityUid>();

                foreach (var gamer in gamers)
                {
                    if (comp.Blacklist != null && _whitelist.IsValid(comp.Blacklist, gamer))
                        continue;

                    if (comp.Whitelist != null && !_whitelist.IsValid(comp.Whitelist, gamer))
                        continue;

                    validTargets.Add(gamer);
                }

                foreach (var target in validTargets)
                {
                    if (comp.Event != null)
                        RaiseLocalEvent(target, comp.Event);
                    else
                        RaiseLocalEvent(target, new HypnoflashedEvent(uid));
                }

                if (activator != null && activator.Value.Valid)
                {
                    if (comp.EventOnUser != null)
                        RaiseLocalEvent(activator.Value, comp.EventOnUser);
                    else
                        RaiseLocalEvent(activator.Value, new HypnoflashActivatedEvent(uid));
                }
            }
        }
    }
}
