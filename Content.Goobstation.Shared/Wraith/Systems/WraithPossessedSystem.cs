using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Revenant;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.Magic.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;

/// <summary>
/// This handles getting possessed by the Wraith.
/// This system is hardcoded for Wraith, so don't re-use this.
/// Use the devil system instead. im sorry and sybau im not unhardocoding ts
/// </summary>
public sealed class WraithPossessedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly WraithRevenantSystem _wraithRevenant = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithPossessedComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<WraithPossessedComponent, DestructionAttemptEvent>(OnDestructionAttempt);
        SubscribeLocalEvent<WraithPossessedComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<WraithPossessedComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (comp.CancelEarly)
                ReturnBack((uid, comp));

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            if (HasComp<AnimateableComponent>(uid))
                ReturnBack((uid, comp));
        }
    }

    private void OnMapInit(Entity<WraithPossessedComponent> ent, ref MapInitEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        ent.Comp.OriginalMind = mindId;
        Dirty(ent);
    }

    private void OnDestructionAttempt(Entity<WraithPossessedComponent> ent, ref DestructionAttemptEvent args)
    {
        // early return on destruction
        if (ent.Comp.PossessorMind == null
            || ent.Comp.Possessor == null)
            return;

        ent.Comp.CancelEarly = true;
        Dirty(ent);

        _mind.TransferTo(ent.Comp.PossessorMind.Value, ent.Comp.Possessor);

        var ev = new PossessionEndedEvent();
        RaiseLocalEvent(ent.Comp.Possessor.Value, ref ev);
    }

    private void OnMobStateChanged(Entity<WraithPossessedComponent> ent, ref MobStateChangedEvent args)
    {
        // early return on death/crit
        if (args.NewMobState == MobState.Alive
            || ent.Comp.PossessorMind == null
            || ent.Comp.Possessor == null)
            return;

        ent.Comp.CancelEarly = true;
        Dirty(ent);

        _mind.TransferTo(ent.Comp.PossessorMind.Value, ent.Comp.Possessor);

        var ev = new PossessionEndedEvent();
        RaiseLocalEvent(ent.Comp.Possessor.Value, ref ev);
    }

    #region Helpers
    /// <summary>
    /// Starts the possession.
    /// Note: Do not use this if you are not a wraith entity lmao
    /// </summary>
    /// <param name="ent"></param> The entity that is being possessed
    /// <param name="possessor"></param> The possessor
    /// <param name="possessorMind"></param> The possessor's mind
    /// <param name="makeRev"></param> Whether to make the user into a Revenant
    public void StartPossession(Entity<WraithPossessedComponent> ent,
        EntityUid possessor,
        EntityUid possessorMind,
        bool makeRev = false)
    {
        SetPossessorAndMind(ent, possessor, possessorMind);

        var ev = new PossessionStartedEvent();
        RaiseLocalEvent(possessor, ref ev);

        if (makeRev)
        {
            _mind.TransferTo(possessorMind, ent.Owner);
            var rev = EnsureComp<WraithRevenantComponent>(ent.Owner);
            // HELP HELP HELP HELP HELP HELP HELPH ELP HELP HELPHELHPLELHEPL PHLELPHE HELHLEHPELHPELHPELHPELHPELHLEPHLE

            var alive = new List<MobState>();
            alive.Add(MobState.Alive);

            _wraithRevenant.SetPassiveDamageValues((ent.Owner, rev), ent.Comp.RevenantDamageOvertime, alive);

            _admin.Add(LogType.Mind, LogImpact.High, $"{ToPrettyString(possessor)} made a revenant (possessed) out of ${ToPrettyString(ent.Owner)}");
            return;
        }

        // its animateable, no reason to check for anything else
        if (HasComp<AnimateableComponent>(ent.Owner))
        {
            _mind.TransferTo(possessorMind, ent.Owner);

            ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.PossessionDuration;
            Dirty(ent);

            _admin.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(possessor)} possessed the object ${ToPrettyString(ent.Owner)}");
        }
    }

    //TO DO: Revenant should get some sort of aura, cosmetic only. Leave for part 2.
    public void SetPossessorAndMind(
        Entity<WraithPossessedComponent> ent,
        EntityUid possessor,
        EntityUid possessorMind)
    {
        ent.Comp.Possessor = possessor;
        ent.Comp.PossessorMind = possessorMind;
        Dirty(ent);
    }

    public void SetPossessionDuration(Entity<WraithPossessedComponent> ent, TimeSpan duration)
    {
        ent.Comp.PossessionDuration = duration;
        Dirty(ent);
    }

    private void ReturnBack(Entity<WraithPossessedComponent> ent)
    {
        if (ent.Comp.Possessor == null || ent.Comp.PossessorMind == null)
            return;

        _mind.TransferTo(ent.Comp.PossessorMind.Value, ent.Comp.Possessor);

        var ev = new PossessionEndedEvent();
        RaiseLocalEvent(ent.Comp.Possessor.Value, ref ev);

        if (ent.Comp.OriginalMind.HasValue
            && TryComp<MindComponent>(ent.Comp.OriginalMind.Value, out var mindComp)
            && _player.TryGetSessionById(mindComp.UserId, out _))
        {
            _mind.TransferTo(ent.Comp.OriginalMind.Value, ent.Owner);
        }

        RemCompDeferred<WraithPossessedComponent>(ent.Owner);
    }
    #endregion
}
