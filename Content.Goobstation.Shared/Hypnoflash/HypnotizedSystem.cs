using Content.Goobstation.Shared.Flashbang;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.Hypnoflash;

public sealed class HypnotizedSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HypnotizedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HypnotizedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HypnotizedComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<HypnotizedConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<MindContainerComponent, HypnoflashedEvent>(OnHypnotized);
    }

    private void OnInit(EntityUid uid, HypnotizedComponent comp, ref ComponentInit args)
    {
        EnsureComp<ActiveListenerComponent>(uid);
        EnsureComp<MutedComponent>(uid); // so you dont hypnotize yourself by mistake
        _stunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(4));
    }

    public override void Update(float frameTime) // so you dont stay muted forever idk
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HypnotizedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Timer -= frameTime;
            if (comp.Timer <= 0)
                RemComp<HypnotizedComponent>(uid);
        }
    }

    private void OnListen(EntityUid uid, HypnotizedComponent comp, ref ListenEvent args)
    {
        var message = args.Message.Trim();

        if (string.IsNullOrWhiteSpace(message))
            return;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        var objectiveId = Spawn("HypnotizedObjective"); // goida
        var meta = Comp<MetaDataComponent>(objectiveId);
        _meta.SetEntityDescription(objectiveId, message, meta);
        _mind.AddObjective(mindId, mind, objectiveId);

        RemComp<HypnotizedComponent>(uid);
        RemComp<ActiveListenerComponent>(uid);
        RemComp<MutedComponent>(uid);
        _stunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(4));
    }

    private void OnShutdown(EntityUid uid, HypnotizedComponent comp, ref ComponentShutdown args)
    {
        RemComp<ActiveListenerComponent>(uid);
        RemComp<MutedComponent>(uid);
    }
    private void OnGetProgress(EntityUid uid, HypnotizedConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f; // "Objective X(xx/nxx) of john goida (xx/nxx) didnt set a progress value!" error my ass
    }

    private void OnHypnotized(EntityUid uid, MindContainerComponent comp, ref HypnoflashedEvent args)
    {
        EnsureComp<HypnotizedComponent>(uid);
        if (_mind.TryGetMind(uid, out var mindId, out var mind))
            _role.MindAddRole(mindId, "MindRoleHypnotized"); // free agent status, but still must follow his objectives right? change to familiar if shitters be shitters
    }
}
