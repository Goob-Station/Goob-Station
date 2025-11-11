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

        SubscribeLocalEvent<CanHallucinateComponent, AddHallucinationsEvent>(OnAddMobs);
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

    private void OnAddMobs(Entity<CanHallucinateComponent> ent, ref AddHallucinationsEvent args)
    {
        var comp = EnsureComp<HallucinatingComponent>(ent.Owner);

        // Ensure that we don't have active hallucinations with this key
        if (comp.Hallucinations.ContainsKey(args.Id))
        {
            if (!args.OverwriteTimer)
                return;

            if (comp.Removes.ContainsKey(args.Id) && args.Duration < 0)
                comp.Removes.Remove(args.Id);
            else
                comp.Removes[args.Id] = _timing.CurTime + TimeSpan.FromSeconds(args.Duration);

            return;
        }

        // Get and add entry
        var entry = _proto.Index(args.Id).Data.GetEntry();
        comp.Hallucinations.Add(args.Id, entry);

        // If not infinite, add timer
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
    /// <summary>
    /// Spawns and makes entity a hallucination for another
    /// </summary>
    /// <param name="uid">Hallucinating entity</param>
    /// <param name="protoId">Entity to spawn</param>
    /// <returns></returns>
    public EntityUid AddHallucination(EntityUid uid, string protoId)
    {
        var comp = EnsureComp<SchizophreniaComponent>(uid);
        var ent = Spawn(protoId, Transform(uid).Coordinates);

        // Set invisible (kinda) layer
        _visibility.SetLayer(ent, (ushort) VisibilityFlags.Hallucination, true);

        // Add pvs override if can
        if (_player.TryGetSessionByEntity(uid, out var session))
            _pvsOverride.AddForceSend(ent, session);

        comp.Hallucinations.Add(ent);

        // Just needed, else game crashes
        var hallucination = new HallucinationComponent()
        {
            Ent = uid
        };
        AddComp(ent, hallucination);

        // We dont need to change index if entity is already hallucinating
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

    /// <summary>
    /// Makes entity a hallucination for another one
    /// </summary>
    /// <param name="uid">Hallucinating entity</param>
    /// <param name="toAdd">Hallucination</param>
    /// <param name="dirty">Whether dirty comps or not. Used for sounds and pointers that does not have to be networked</param>
    public void AddAsHallucination(EntityUid uid, EntityUid toAdd, bool dirty = true)
    {
        var comp = EnsureComp<SchizophreniaComponent>(uid);

        // Set invisible (kinda) layer
        _visibility.SetLayer(toAdd, (ushort) VisibilityFlags.Hallucination, true);

        // Add pvs override if can
        if (_player.TryGetSessionByEntity(uid, out var session))
            _pvsOverride.AddForceSend(toAdd, session);

        comp.Hallucinations.Add(toAdd);

        // Just needed, else game crashes
        var hallucination = new HallucinationComponent()
        {
            Ent = uid
        };
        AddComp(toAdd, hallucination);

        // We dont need to change index if entity is already hallucinating
        if (comp.Idx <= 0)
        {
            comp.Idx = _nextIdx;
            _nextIdx++;
        }

        hallucination.Idx = comp.Idx;

        // Dirty if needed
        if (dirty)
        {
            Dirty(uid, comp);
            Dirty(toAdd, hallucination);
        }
    }
    #endregion
}
