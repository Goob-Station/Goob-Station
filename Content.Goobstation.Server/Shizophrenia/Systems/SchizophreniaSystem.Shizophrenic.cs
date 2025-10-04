using Content.Goobstation.Shared.Shizophrenia;
using Content.Shared.Eye;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Shizophrenia;

public sealed partial class SchizophreniaSystem : EntitySystem
{
    private void InitializeShizophrenic()
    {
        SubscribeLocalEvent<SchizophreniaComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SchizophreniaComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<CanHallucinateComponent, AddMobHallucinationsEvent>(OnAddMobs);
        SubscribeLocalEvent<CanHallucinateComponent, AddAppearanceHallucinationsEvent>(OnAddAppearances);
        SubscribeLocalEvent<HallucinatingComponent, RemoveHallucinationsEvent>(OnRemove);
    }

    private void OnPlayerAttached(Entity<SchizophreniaComponent> ent, ref PlayerAttachedEvent args)
    {
        foreach (var item in ent.Comp.Hallucinations)
            _pvsOverride.AddForceSend(item, args.Player);
    }

    private void OnPlayerDetached(Entity<SchizophreniaComponent> ent, ref PlayerDetachedEvent args)
    {
        foreach (var item in ent.Comp.Hallucinations)
            _pvsOverride.RemoveForceSend(item, args.Player);
    }

    private void OnAddMobs(Entity<CanHallucinateComponent> ent, ref AddMobHallucinationsEvent args)
    {
        var comp = EnsureComp<HallucinatingComponent>(ent.Owner);

        if (comp.Hallucinations.ContainsKey(args.Id))
            comp.Hallucinations.Remove(args.Id);

        if (comp.Removes.ContainsKey(args.Id))
            comp.Removes.Remove(args.Id);

        var entry = new MobHallucinationsEntry()
        {
            Prototypes = args.Entities,
            Delay = args.Delay,
            Range = args.Range,
            SpawnCount = args.SpawnCount,
            NextPerform = _timing.CurTime + TimeSpan.FromSeconds(10f)
        };
        comp.Hallucinations.Add(args.Id, entry);

        if (args.Duration > 0)
            comp.Removes.Add(args.Id, _timing.CurTime + TimeSpan.FromSeconds(args.Duration));
    }

    private void OnAddAppearances(Entity<CanHallucinateComponent> ent, ref AddAppearanceHallucinationsEvent args)
    {
        var comp = EnsureComp<HallucinatingComponent>(ent.Owner);

        if (comp.Hallucinations.ContainsKey(args.Id))
            comp.Hallucinations.Remove(args.Id);

        if (comp.Removes.ContainsKey(args.Id))
            comp.Removes.Remove(args.Id);

        var entry = new AppearanceHallucinationsEntry()
        {
            Delay = args.Delay,
            NextPerform = _timing.CurTime + TimeSpan.FromSeconds(10f)
        };

        for (var i = 0; i < args.Appearances.Count; i++)
            entry.Appearances.Add(args.Appearances[i]);

        comp.Hallucinations.Add(args.Id, entry);

        if (args.Duration > 0)
            comp.Removes.Add(args.Id, _timing.CurTime + TimeSpan.FromSeconds(args.Duration));
    }

    private void OnRemove(Entity<HallucinatingComponent> ent, ref RemoveHallucinationsEvent args)
    {
        if (ent.Comp.Hallucinations.ContainsKey(args.Id))
            ent.Comp.Hallucinations.Remove(args.Id);

        if (ent.Comp.Removes.ContainsKey(args.Id))
            ent.Comp.Removes.Remove(args.Id);
    }

    #region Public
    public EntityUid AddHallucination(EntityUid uid, string protoId, SchizophreniaComponent? comp = null)
    {
        comp = EnsureComp<SchizophreniaComponent>(uid);
        var ent = Spawn(protoId, Transform(uid).Coordinates);

        _visibility.SetLayer(ent, (ushort) VisibilityFlags.Hallucination, true);
        if (_player.TryGetSessionByEntity(uid, out var session))
            _pvsOverride.AddForceSend(ent, session);

        comp.Hallucinations.Add(ent);

        var hallucination = new HallucinationComponent()
        {
            Ent = uid
        };
        AddComp(ent, hallucination);

        if (comp.Idx <= 0)
        {
            comp.Idx = _nextIdx;
            _nextIdx++;
        }

        hallucination.Idx = comp.Idx;
        hallucination.Ent = uid;

        Dirty(uid, comp);
        Dirty(ent, hallucination);
        return ent;
    }

    public void AddAsHallucination(EntityUid uid, EntityUid toAdd, bool dirty = true)
    {
        var comp = EnsureComp<SchizophreniaComponent>(uid);
        _visibility.SetLayer(toAdd, (ushort) VisibilityFlags.Hallucination, true);
        if (_player.TryGetSessionByEntity(uid, out var session))
            _pvsOverride.AddForceSend(toAdd, session);

        var hallucination = new HallucinationComponent()
        {
            Ent = uid
        };
        AddComp(toAdd, hallucination);

        comp.Hallucinations.Add(toAdd);

        if (comp.Idx <= 0)
        {
            comp.Idx = _nextIdx;
            _nextIdx++;
        }

        hallucination.Idx = comp.Idx;

        if (dirty)
        {
            Dirty(uid, comp);
            Dirty(toAdd, hallucination);
        }
    }
    #endregion
}
