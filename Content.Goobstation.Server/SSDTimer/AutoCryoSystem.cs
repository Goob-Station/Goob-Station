using Content.Goobstation.Common.CCVar;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.CCVar;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles.Jobs;
using Content.Shared.SSDIndicator;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.SSDTimer;

public sealed class AutoCryoSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private static readonly ProtoId<HTNCompoundPrototype> AutoCryoCompound = "AutoCryoCompound";
    private float _scanInterval = 5f;
    private bool _enabled;
    private float _autoCryoTime;

    private float _icSsdSleepTime;
    private TimeSpan _nextScan;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoCryoActiveComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<AutoCryoActiveComponent, ComponentShutdown>(OnShutdown);

        Subs.CVar(_cfg, GoobCVars.AutoCryoEnabled, v => _enabled = v, true);
        Subs.CVar(_cfg, GoobCVars.AutoCryoTime, v => _autoCryoTime = v, true);
        Subs.CVar(_cfg, CCVars.ICSSDSleepTime, v => _icSsdSleepTime = v, true);
    }

    private void OnPlayerAttached(Entity<AutoCryoActiveComponent> ent, ref PlayerAttachedEvent args)
    {
        StopAutoCryo(ent);
    }

    private void OnShutdown(Entity<AutoCryoActiveComponent> ent, ref ComponentShutdown args)
    {
        RemComp<HTNComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_enabled)
            return;

        var curTime = _timing.CurTime;
        if (curTime < _nextScan)
            return;

        _nextScan = curTime + TimeSpan.FromSeconds(_scanInterval);

        var threshold = TimeSpan.FromSeconds(_autoCryoTime);
        var query = EntityQueryEnumerator<SSDIndicatorComponent>();
        while (query.MoveNext(out var uid, out var ssd))
        {
            if (!ssd.IsSSD)
                continue;

            if (HasComp<CryostorageContainedComponent>(uid))
                continue;

            if (HasComp<AutoCryoActiveComponent>(uid))
                continue;

            if (ssd.FallAsleepTime == TimeSpan.Zero)
                continue;

            var ssdSince = curTime - (ssd.FallAsleepTime - TimeSpan.FromSeconds(_icSsdSleepTime));
            if (ssdSince < threshold)
                continue;

            if (!IsEligible(uid))
                continue;

            StartAutoCryo(uid);
        }
    }

    private bool IsEligible(EntityUid uid)
    {
        if (TerminatingOrDeleted(uid))
            return false;

        if (!HasComp<HumanoidAppearanceComponent>(uid))
            return false;

        if (!HasComp<CanEnterCryostorageComponent>(uid))
            return false;

        if (!_mobState.IsAlive(uid))
            return false;

        if (_mind.TryGetMind(uid, out var _, out var mind))
        {
            if (mind.VisitingEntity is { } visiting
                && TryComp<GhostComponent>(visiting, out var ghost)
                && ghost.CanReturnToBody)
            {
                return false;
            }
        }

        return true;
    }

    private void StartAutoCryo(EntityUid uid)
    {
        EnsureComp<AutoCryoActiveComponent>(uid);

        var htn = EnsureComp<HTNComponent>(uid);
        htn.RootTask = new HTNCompoundTask { Task = AutoCryoCompound };
        htn.Blackboard.SetValue(NPCBlackboard.NavInteract, true);
        htn.Blackboard.SetValue(NPCBlackboard.Owner, uid);
        htn.Blackboard.SetValue("CryoSearchRange", 500f);

        _npc.WakeNPC(uid, htn);
    }

    private void StopAutoCryo(EntityUid uid)
    {
        RemComp<HTNComponent>(uid);
        RemComp<AutoCryoActiveComponent>(uid);
    }
}
