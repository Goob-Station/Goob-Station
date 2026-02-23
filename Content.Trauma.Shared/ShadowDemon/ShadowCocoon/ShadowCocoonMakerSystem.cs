using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Content.Trauma.Common.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon;


public sealed class ShadowCocoonMakerSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    // [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    private readonly ProtoId<TagPrototype> _shadowCocoonMaker = "ShadowCocoonMaker";
    private readonly EntProtoId _shadowCocoon = "ShadowCocoon";

    private TimeSpan _cocoonTime;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanBeShadowCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<CanBeShadowCocoonComponent, ShadowCocoonDoAfterEvent>(OnCocoonDoAfter);

        Subs.CVar(_cfg, TraumaCVars.ShadowCocoonDelay, x => _cocoonTime = TimeSpan.FromSeconds(x), true);
    }

    private void OnGetAltVerbs(Entity<CanBeShadowCocoonComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!_tag.HasTag(args.User, _shadowCocoonMaker))
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
                StartCocooning(user, target);
            }
        });
    }

    private void OnCocoonDoAfter(Entity<CanBeShadowCocoonComponent> ent, ref ShadowCocoonDoAfterEvent args)
    {
        if (args.Target is not {} target)
            return;

        var spawnAt = Transform(target).Coordinates;
        var cocoon = PredictedSpawnAtPosition(_shadowCocoon, spawnAt);

        _entityStorage.Insert(ent.Owner, cocoon);

        _adminLog.Add(LogType.Verb, LogImpact.High,
            $"{args.User} spawned a shadow cocoon and put {target} inside");
    }

    private void StartCocooning(EntityUid user, EntityUid target)
    {
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            user,
            _cocoonTime,
            new ShadowCocoonDoAfterEvent(),
            user,
            target)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
}
