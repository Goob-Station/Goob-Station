using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Conversion;
using Content.Goobstation.Common.Heretic;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedHereticSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private EntityQuery<HereticComponent> _hereticQuery;
    private EntityQuery<GhoulComponent> _ghoulQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCheckEvent>(OnCheck);
        SubscribeLocalEvent<BeforeConversionEvent>(OnBeforeConversion);
        SubscribeLocalEvent<HereticComponent, EventHereticAddKnowledge>(OnAddKnowledge);

        _hereticQuery = GetEntityQuery<HereticComponent>();
        _ghoulQuery = GetEntityQuery<GhoulComponent>();
    }

    private void OnAddKnowledge(Entity<HereticComponent> ent, ref EventHereticAddKnowledge args)
    {
        foreach (var knowledge in args.Knowledge)
        {
            TryAddKnowledge(ent.AsNullable(), knowledge);
        }
    }

    private void OnBeforeConversion(ref BeforeConversionEvent ev)
    {
        if (TryGetHereticComponent(ev.Uid, out _, out _))
            ev.Blocked = true;
    }

    private void OnCheck(ref HereticCheckEvent ev)
    {
        ev.Result = TryGetHereticComponent(ev.Uid, out _, out _);
    }

    public bool TryGetHereticComponent(
        EntityUid uid,
        [NotNullWhen(true)] out HereticComponent? heretic,
        out EntityUid mind)
    {
        heretic = null;
        return _mind.TryGetMind(uid, out mind, out _) && _hereticQuery.TryComp(mind, out heretic);
    }

    public bool IsHereticOrGhoul(EntityUid uid)
    {
        return _ghoulQuery.HasComp(uid) || TryGetHereticComponent(uid, out _, out _);
    }

    public virtual bool TryAddKnowledge(Entity<HereticComponent?> ent,
        ProtoId<HereticKnowledgePrototype> id,
        EntityUid? body = null)
    {
        return false;
    }
}
