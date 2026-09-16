using Content.Goobstation.Server.Voidwalker.Kidnapping;
using Content.Goobstation.Shared.TrackedComponents;
using Content.Goobstation.Shared.Voidwalker.Abilities.Kidnap;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.Spaced;
using Content.Goobstation.Shared.Voidwalker.Voided;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.GameTicking;
using Content.Shared.Medical;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Voidwalker.Voided;

public sealed class VoidedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly VoidwalkerKidnappedSystem _voidKidnapped = null!;
    [Dependency] private readonly VoidwalkerKidnappingSystem _voidKidnapping = null!;
    [Dependency] private readonly VomitSystem _vomit = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly TrackedComponentsSystem _trackedComponents = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VoidedComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    /// <summary>
    ///  Everyone gets to play in the void!!!
    /// </summary>
    private void OnRoundEnd(RoundEndMessageEvent args)
    {
        var voidedQuery = EntityQueryEnumerator<VoidedComponent>();
        while (voidedQuery.MoveNext(out var uid, out _))
            _voidKidnapping.TrySendToShadowRealm(uid);

        var voidwalkerQuery = EntityQueryEnumerator<VoidwalkerComponent>();
        while (voidwalkerQuery.MoveNext(out var uid, out _))
            _voidKidnapping.TrySendToShadowRealm(uid);
    }

    private void OnStartup(Entity<VoidedComponent> entity, ref ComponentStartup args)
    {
        _trackedComponents.EnsureTrackedComp<VoidedVisualsComponent>(entity, entity.Comp.TrackedComponentsIdentifier);
        _trackedComponents.EnsureTrackedComp<VoidAccentComponent>(entity, entity.Comp.TrackedComponentsIdentifier); // This was muted in ss13, but I think this accent is cooler.
        _trackedComponents.EnsureTrackedComp<PacifiedComponent>(entity, entity.Comp.TrackedComponentsIdentifier);
        var spaced = _trackedComponents.EnsureTrackedComp<SpacedStatusComponent>(entity, entity.Comp.TrackedComponentsIdentifier);
        spaced.CheckOnInterval = false; // we can do it ourselves

        SetNextVomitTime(entity);
    }

    private void OnShutdown(Entity<VoidedComponent> entity, ref ComponentShutdown args)
    {
        _trackedComponents.RemoveTrackedComps(entity, entity.Comp.TrackedComponentsIdentifier);
    }

    private void SetNextVomitTime(Entity<VoidedComponent> voided) =>
        voided.Comp.NextVomitTime = _timing.CurTime + _random.Next(voided.Comp.MinVomitInterval, voided.Comp.MaxVomitInterval);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Check every X amount of seconds. Just in case.
        var query = EntityQueryEnumerator<VoidedComponent>();
        while (query.MoveNext(out var uid, out var voided))
        {
            if (_timing.CurTime >= voided.NextSpacedCheck)
            {
                var ev = new CheckSpacedStatusEvent();
                RaiseLocalEvent(uid, ref ev);

                if (ev.Spaced)
                {
                    var map = _map.GetMap(Transform(uid).MapID);
                    if (!_voidKidnapped.TryTeleportToRandomPartOfStation(uid, map))
                        return;

                    var popup = Loc.GetString("voided-spaced-teleport");
                    _popup.PopupEntity(popup, uid, uid, PopupType.MediumCaution);
                }

                voided.NextSpacedCheck = _timing.CurTime + voided.SpacedCheckInterval;
            }

            if (_timing.CurTime >= voided.NextVomitTime)
            {
                _vomit.Vomit(uid, voided.ThirstLost, voided.HungerLost, true, voided.NebulaVomitProto);
                SetNextVomitTime((uid, voided));
            }

        }
    }

}
