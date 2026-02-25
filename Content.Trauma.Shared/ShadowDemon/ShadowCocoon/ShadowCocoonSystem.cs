using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Physics.Events;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;


public sealed class ShadowCocoonSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    // [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPoweredLightSystem _poweredLight = default!;

    private EntityQuery<ShadowCocoonMakerComponent> _shadowCocoonMakerQuery;

    private readonly HashSet<Entity<PoweredLightComponent>> _lights = new(); // TODO: more generic

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanBeShadowCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<CanBeShadowCocoonComponent, ShadowCocoonDoAfterEvent>(OnCocoonDoAfter);

        SubscribeLocalEvent<ShadowCocoonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowCocoonComponent, StartCollideEvent>(OnCollide);

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
        if (args.Target is not {} target || !_shadowCocoonMakerQuery.TryComp(args.User, out var shadowCocoonMaker))
            return;

        var spawnAt = Transform(target).Coordinates;
        var cocoon = PredictedSpawnAtPosition(shadowCocoonMaker.ShadowCocoon, spawnAt);

        _entityStorage.Insert(ent.Owner, cocoon);

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
            user,
            target)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
    #endregion

    #region Shadow Cocoon Entity

    private void OnMapInit(Entity<ShadowCocoonComponent> ent, ref MapInitEvent args)
    {
        // Automatically break lights when spawning
        // TODO: Store those lights somewhere so they get ignored or smth idk optimize it
        _lights.Clear();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.Radius, _lights);
        foreach (var light in _lights)
        {
            _poweredLight.TryDestroyBulb(light.Owner, light.Comp);
        }
    }

    private void OnCollide(Entity<ShadowCocoonComponent> ent, ref StartCollideEvent args)
    {
        // TODO: Implement this, needs good optimization
        // On Collision -> break light
    }
    #endregion
}
