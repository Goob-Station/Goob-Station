using Content.Goobstation.Shared.Flashbang;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Hypnoflash;

public sealed class HypnotizedSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HypnotizedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HypnotizedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HypnotizedComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<HypnotizedConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<MindContainerComponent, HypnoflashedEvent>(OnHypnotized);
    }

    private void OnInit(Entity<HypnotizedComponent> ent, ref ComponentInit args)
    {
        EnsureComp<MutedComponent>(ent); // so you dont hypnotize yourself by mistake
        EnsureComp<ActiveListenerComponent>(ent);

        ent.Comp.EndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Timer);
        _stunSystem.TryKnockdown(ent.Owner, TimeSpan.FromSeconds(4));
    }
    public override void Update(float frameTime) // so you dont stay muted forever idk
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HypnotizedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.EndTime)
                RemCompDeferred<HypnotizedComponent>(uid);
        }
    }
    private void OnListen(Entity<HypnotizedComponent> ent, ref ListenEvent args)
    {
        var message = args.Message.Trim();

        if (string.IsNullOrWhiteSpace(message))
            return;

        if (!_mind.TryGetMind(ent, out var mindId, out var mind))
            return;

        var objectiveId = Spawn("HypnotizedObjective"); // goida
        _meta.SetEntityDescription(objectiveId, message);
        _mind.AddObjective(mindId, mind, objectiveId);

        RemCompDeferred<MutedComponent>(ent); // when the mimes talk...
        RemCompDeferred<HypnotizedComponent>(ent);
        RemCompDeferred<ActiveListenerComponent>(ent);
        _stunSystem.TryKnockdown(ent.Owner, TimeSpan.FromSeconds(4));
    }

    private void OnShutdown(Entity<HypnotizedComponent> ent, ref ComponentShutdown args)
    {
        RemCompDeferred<ActiveListenerComponent>(ent);
        RemCompDeferred<MutedComponent>(ent);
    }
    private void OnGetProgress(EntityUid uid, HypnotizedConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f; // "Objective X(xx/nxx) of john goida (xx/nxx) didnt set a progress value!" error my ass
    }
    private void OnHypnotized(Entity<MindContainerComponent> ent, ref HypnoflashedEvent args)
    {
        EnsureComp<HypnotizedComponent>(ent.Owner);
        if (_mind.TryGetMind(ent.Owner, out var mindId, out var mind))
            _role.MindAddRole(mindId, "MindRoleHypnotized"); // free agent status, but still must follow his objectives right? change to familiar if shitters be shitters
    }
}
