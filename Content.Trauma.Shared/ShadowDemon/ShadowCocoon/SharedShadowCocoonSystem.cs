using Content.Goobstation.Shared.Sound.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;


public abstract class SharedShadowCocoonSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ShadowCocoonMakerComponent> _shadowCocoonMakerQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanBeShadowCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<CanBeShadowCocoonComponent, ShadowCocoonDoAfterEvent>(OnCocoonDoAfter);

        SubscribeLocalEvent<ShadowCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltShadowCocoonVerbs);
        SubscribeLocalEvent<ShadowCocoonComponent, MapInitEvent>(OnMapInit);

        _shadowCocoonMakerQuery = GetEntityQuery<ShadowCocoonMakerComponent>();
    }

    #region Shadow Cocoon Maker
    private void OnGetAltVerbs(Entity<CanBeShadowCocoonComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!_shadowCocoonMakerQuery.TryComp(args.User, out var shadowCocoonMaker))
            return;

        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;
        var target = args.Target;
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("shadow-cocoon-verb"),
            Act = () =>
            {
                StartCocooning(user, target, shadowCocoonMaker.CocoonDelay);
            }
        });
    }

    private void OnCocoonDoAfter(Entity<CanBeShadowCocoonComponent> ent, ref ShadowCocoonDoAfterEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Target is not {} target || !_shadowCocoonMakerQuery.TryComp(args.User, out var shadowCocoonMaker))
            return;

        var spawnAt = Transform(target).Coordinates;
        var cocoon = SpawnAtPosition(shadowCocoonMaker.ShadowCocoon, spawnAt);

        _entityStorage.Insert(target, cocoon);

        _adminLog.Add(LogType.Verb, LogImpact.High,
            $"{args.User} spawned a shadow cocoon and put {target} inside");
    }

    private void StartCocooning(EntityUid user, EntityUid target, TimeSpan delay)
    {
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            user,
            delay,
            new ShadowCocoonDoAfterEvent(),
            target,
            target)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
    #endregion

    #region Shadow Cocoon Entity
    private void OnMapInit(Entity<ShadowCocoonComponent> ent, ref MapInitEvent args) =>
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Update;

    private void OnGetAltShadowCocoonVerbs(Entity<ShadowCocoonComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!_shadowCocoonMakerQuery.HasComp(args.User))
            return;

        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("shadow-cocoon-activate-sounds-verb"),
            Act = () =>
            {
                ent.Comp.Silent = !ent.Comp.Silent;
                Dirty(ent);

                if (!ent.Comp.Silent)
                {
                    var sounds = EnsureComp<RandomIntervalSoundComponent>(ent.Owner);
                    sounds.Sound = ent.Comp.RandomSounds;

                    _popup.PopupClient(Loc.GetString("shadow-cocoon-halluc-activated"), user, user);
                    return;
                }

                _popup.PopupClient(Loc.GetString("shadow-cocoon-halluc-deactivated"), user, user);
                RemCompDeferred<RandomIntervalSoundComponent>(ent.Owner);
            }
        });
    }
    #endregion
}
