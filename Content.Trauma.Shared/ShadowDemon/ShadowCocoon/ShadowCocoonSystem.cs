using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;


public sealed class ShadowCocoonSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    private EntityQuery<ShadowCocoonMakerComponent> _shadowCocoonMakerQuery;

    /// <summary>
    /// Attaches to the shadow cocoon to detect nearby lights
    /// </summary>
    private static EntProtoId _shadowCocoonArea = "ShadowCocoonArea";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanBeShadowCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<CanBeShadowCocoonComponent, ShadowCocoonDoAfterEvent>(OnCocoonDoAfter);

        _shadowCocoonMakerQuery = GetEntityQuery<ShadowCocoonMakerComponent>();
    }

    // TODO: Update loop

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

        // Spawn the area used to detect lights
        SpawnAttachedTo(_shadowCocoonArea, Transform(cocoon).Coordinates);

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
}
