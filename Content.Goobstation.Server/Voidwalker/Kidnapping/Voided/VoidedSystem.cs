using Content.Server.Gibbing.Systems;
using Content.Shared.Medical;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Voidwalker.Kidnapping.Voided;

public sealed class VoidedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly VoidwalkerSystem _voidwalker = null!;
    [Dependency] private readonly VoidwalkerKidnappedSystem _voidKidnapped = null!;
    [Dependency] private readonly VomitSystem _vomit = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;

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
    /// <param name="args"></param>
    private void OnRoundEnd(RoundEndMessageEvent args)
    {
        var voidedQuery = EntityQueryEnumerator<VoidedComponent>();
        while (voidedQuery.MoveNext(out var uid, out _))
            _voidwalker.TrySendToShadowRealm(uid);

        var voidwalkerQuery = EntityQueryEnumerator<VoidwalkerComponent>();
        while (voidwalkerQuery.MoveNext(out var uid, out _))
            _voidwalker.TrySendToShadowRealm(uid);
    }

    private void OnStartup(Entity<VoidedComponent> entity, ref ComponentStartup args)
    {
        EnsureComp<VoidedVisualsComponent>(entity);
        EnsureComp<VoidAccentComponent>(entity); // This was muted in ss13, but I think this accent is cooler.
        EnsureComp<PacifiedComponent>(entity);

        SetNextVomitTime(entity);
    }

    private void OnShutdown(Entity<VoidedComponent> entity, ref ComponentShutdown args)
    {
        RemComp<VoidedVisualsComponent>(entity);
        RemComp<VoidAccentComponent>(entity);
        RemComp<PacifiedComponent>(entity);
    }

    private void SetNextVomitTime(Entity<VoidedComponent> voided) =>
        voided.Comp.NextVomitTime = _timing.CurTime + _random.Next(voided.Comp.MinVomitInterval, voided.Comp.MaxVomitInterval);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Check every X amount of seconds. Just in case.
        var query = EntityQueryEnumerator<VoidedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.NextSpacedCheck)
            {
                if (_voidwalker.CheckInSpace(uid))
                {
                    var map = _map.GetMap(Transform(uid).MapID);
                    if (!_voidKidnapped.TryTeleportToRandomPartOfStation(uid, map))
                        return;

                    var popup = Loc.GetString("voided-spaced-teleport");
                    _popup.PopupEntity(popup, uid, uid, PopupType.MediumCaution);
                }

                comp.NextSpacedCheck = _timing.CurTime + comp.SpacedCheckInterval;
            }

            if (_timing.CurTime >= comp.NextVomitTime)
            {
                _vomit.Vomit(uid, comp.ThirstLost, comp.HungerLost, true, comp.NebulaVomitProto);
                SetNextVomitTime((uid, comp));
            }

        }
    }
}
