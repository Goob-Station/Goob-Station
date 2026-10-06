using Content.Goobstation.Shared.Gangwars.Components;
using Content.Shared.Construction.Components;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.Gangwars.Systems;

/// <summary>
/// Handles the anchoring timer and unlock restrictions for the gang crate
/// </summary>
public sealed class GangCrateSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangCrateComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<GangCrateComponent, StorageOpenAttemptEvent>(OnStorageOpenAttempt);
        SubscribeLocalEvent<GangCrateComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
        SubscribeLocalEvent<GangCrateComponent, StorageCloseAttemptEvent>(OnStorageCloseAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<GangCrateComponent>();

        while (query.MoveNext(out var uid, out var crate))
        {
            if (crate.LightState == GangCrateLightState.Off)
                continue;

            if (curTime >= crate.AnchoredUntil)
            {
                StopBlinkSound(uid, crate);
                _audio.PlayPredicted(crate.ReadySound, uid, uid);
                SetLightState(uid, crate, GangCrateLightState.Off);
                _transform.Unanchor(uid);
                continue;
            }

            var nextState = curTime >= crate.BlinkThreshold ? GangCrateLightState.Blinking
                          : curTime >= crate.LowThreshold ? GangCrateLightState.Low
                          : curTime >= crate.HalfThreshold ? GangCrateLightState.Half
                          : GangCrateLightState.Full;

            if (nextState != crate.LightState)
            {
                SetLightState(uid, crate, nextState);
                if (nextState == GangCrateLightState.Blinking)
                    StartBlinkSound(uid, crate);
            }
        }
    }

    private void OnMapInit(Entity<GangCrateComponent> ent, ref MapInitEvent args)
    {
        var curTime = _timing.CurTime;
        var duration = ent.Comp.AnchorDuration;

        ent.Comp.AnchoredUntil = curTime + duration;
        ent.Comp.HalfThreshold = curTime + duration * 0.33;
        ent.Comp.LowThreshold = curTime + duration * 0.66;
        ent.Comp.BlinkThreshold = curTime + duration * 0.90;
        ent.Comp.LightState = GangCrateLightState.Full;

        _transform.AnchorEntity(ent.Owner);
        Dirty(ent);
    }

    private void OnStorageOpenAttempt(Entity<GangCrateComponent> ent, ref StorageOpenAttemptEvent args)
    {
        if (ent.Comp.RewardGiven)
            return;

        if (!IsNearGangLocker(ent.Owner, ent.Comp.LockerRange))
        {
            args.Cancelled = true;
            _popup.PopupClient(Loc.GetString("gang-crate-no-locker-nearby"), ent.Owner, args.User);
            return;
        }

        var coords = Transform(ent.Owner).Coordinates;
        var lockers = _lookup.GetEntitiesInRange<GangLockerComponent>(coords, ent.Comp.LockerRange);
        if (lockers.Count == 0)
            return;

        var gangColor = lockers.First().Comp.GangColor;

        ent.Comp.RewardGiven = true;
        Dirty(ent);

        var memberQuery = EntityQueryEnumerator<GangMemberComponent>();
        while (memberQuery.MoveNext(out var memberUid, out var member))
        {
            if (member.Gang != gangColor)
                continue;

            member.GangPoints += ent.Comp.OpenReward;
            Dirty(memberUid, member);

            if (_net.IsServer) // Can't use client methods or it fails for other gang members
            {
                _popup.PopupEntity(Loc.GetString("gang-crate-reward-popup", ("points", ent.Comp.OpenReward)), memberUid, memberUid, PopupType.Large);
                _audio.PlayGlobal(ent.Comp.RewardSound, memberUid);
            }
        }

        QueueDel(ent.Owner);
    }

    private void OnUnanchorAttempt(Entity<GangCrateComponent> ent, ref UnanchorAttemptEvent args)
    {
        if (ent.Comp.LightState != GangCrateLightState.Off)
            args.Cancel();
    }

    private void OnStorageCloseAttempt(Entity<GangCrateComponent> ent, ref StorageCloseAttemptEvent args) =>
        args.Cancelled = true;

    private void SetLightState(EntityUid uid, GangCrateComponent crate, GangCrateLightState state)
    {
        crate.LightState = state;
        Dirty(uid, crate);
    }

    private void StartBlinkSound(EntityUid uid, GangCrateComponent crate)
    {
        if (!_net.IsServer)
            return;

        StopBlinkSound(uid, crate);
        crate.BlinkSoundEntity = _audio.PlayPvs(crate.BlinkSound, uid)?.Entity;
    }

    private void StopBlinkSound(EntityUid uid, GangCrateComponent crate)
    {
        if (!_net.IsServer)
            return;

        crate.BlinkSoundEntity = _audio.Stop(crate.BlinkSoundEntity);
    }

    private bool IsNearGangLocker(EntityUid crateUid, float range)
    {
        var coords = Transform(crateUid).Coordinates;
        var lockers = _lookup.GetEntitiesInRange<GangLockerComponent>(coords, range);
        return lockers.Count > 0;
    }
}
