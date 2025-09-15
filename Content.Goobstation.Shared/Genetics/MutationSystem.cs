using Content.Shared.Forensics.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Text;

namespace Content.Goobstation.Shared.Genetics;

public sealed class MutationSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly char[] _acgt = new[] { 'A', 'C', 'G', 'T' };
    private EntityQuery<DnaComponent> _dnaQuery;

    public override void Initialize()
    {
        base.Initialize();

        _dnaQuery = GetEntityQuery<DnaComponent>();

        SubscribeLocalEvent<MutatableComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<MutatableComponent> ent, ref MapInitEvent args)
    {
        RemoveConflictingMutations(ent);

        foreach (var id in ent.Comp.Mutations)
        {
            var mutation = _proto.Index(id);
            MutationAdded(ent, mutation);
        }
    }

    private void MutationAdded(Entity<MutatableComponent> ent, MutationPrototype mutation)
    {
        if (IsForeign(ent, mutation.ID))
            AddInstability(ent, mutation.Instability);

        foreach (var removing in mutation.Removes)
        {
            RemoveMutation(ent, removing);
        }

        if (mutation.AddedComponents is {} add)
            EntityManager.AddComponents(ent, add);
        if (mutation.RemovedComponents is {} remove)
            EntityManager.RemoveComponents(ent, remove);
    }

    private void MutationRemoved(Entity<MutatableComponent> ent, MutationPrototype mutation)
    {
        // very important that dormant is checked before removing instability
        // otherwise livrah rat heart incident can happen but for instability instead of damage reduction
        if (IsForeign(ent, mutation.ID))
            AddInstability(ent, -mutation.Instability);

        if (mutation.AddedComponents is {} add)
            EntityManager.RemoveComponents(ent, add);
        if (mutation.RemovedComponents is {} remove)
            EntityManager.AddComponents(ent, remove);
    }

    #region Public API

    /// <summary>
    /// Returns true if a mutation is foreign to an entity, i.e. not present in Dormant.
    /// </summary>
    public bool IsForeign(MutatableComponent comp, ProtoId<MutationPrototype> id)
        => !comp.Dormant.Contains(id);

    /// <summary>
    /// Tries to add a mutation to an entity, returning true if it succeeded.
    /// Instability increases if the mutation <see cref="IsForeign"/>.
    /// </summary>
    public bool AddMutation(Entity<MutatableComponent> ent, ProtoId<MutationPrototype> id)
    {
        if (ent.Comp.Mutations.Contains(id))
            return false; // already have it chuddy

        var mutation = _proto.Index(id);
        foreach (var good in mutation.Required)
        {
            if (!ent.Comp.Mutations.Contains(good))
                return false; // required mutation missing
        }

        foreach (var bad in mutation.Conflicts)
        {
            if (ent.Comp.Mutations.Contains(bad))
                return false; // conflicting mutation found
        }

        Log.Debug($"Added mutation '{mutation.LocalizedName}' ({id}) to {ToPrettyString(ent)}");
        ent.Comp.Mutations.Add(id);
        Dirty(ent);
        MutationAdded(ent, mutation);
        MutateDna(ent);
        return true;
    }

    /// <summary>
    /// Tries to activate a dormant mutation, does nothing if the mutation is not present in Dormant.
    /// Won't add instability to the entity.
    /// </summary>
    public bool ActivateMutation(Entity<MutatableComponent> ent, ProtoId<MutationPrototype> id)
    {
        return ent.Comp.Dormant.Contains(id) && AddMutation(ent, id);
    }

    public bool RemoveMutation(Entity<MutatableComponent> ent, ProtoId<MutationPrototype> id)
    {
        if (!ent.Comp.Mutations.Contains(id))
            return false; // didn't have it anyways chuddy

        var mutation = _proto.Index(id);
        if (mutation.Permanent)
            return false; // lol no

        foreach (var existing in ent.Comp.Mutations)
        {
            if (_proto.Index(existing).Required.Contains(id))
                return false; // other mutations depend on it
        }

        Log.Debug($"Removed mutation '{mutation.LocalizedName}' ({id}) from {ToPrettyString(ent)}");
        ent.Comp.Mutations.Remove(id);
        Dirty(ent);
        MutationRemoved(ent, mutation);
        MutateDna(ent);
        return true;
    }

    /// <summary>
    /// Randomizes <c>rolls</c> letters of the entity's forensics DNA.
    /// </summary>
    public void MutateDna(EntityUid uid, int rolls = 4)
    {
        if (_net.IsClient || !_dnaQuery.TryComp(uid, out var comp) || comp.DNA is not {} dna)
            return;

        var builder = new StringBuilder(dna);
        var max = dna.Length;
        for (int i = 0; i < rolls; i++)
        {
            var n = _random.Next(0, max);
            builder[n] = _random.Pick(_acgt);
        }

        comp.DNA = builder.ToString();
        Dirty(uid, comp);
    }

    /// <summary>
    /// Removes any mutations that conflict with others on the entity.
    /// Required mutations are ignored though, so you can write some cool stuff in YML.
    /// </summary>
    public bool RemoveConflictingMutations(Entity<MutatableComponent> ent)
    {
        var dirty = false;
        ent.Comp.Mutations.RemoveWhere(id =>
        {
            var mutation = _proto.Index(id);
            foreach (var bad in mutation.Conflicts)
            {
                if (!ent.Comp.Mutations.Contains(bad))
                    continue;

                Log.Error($"{ToPrettyString(ent)} had conflicting mutations {id} and {bad}, removing the first one.");
                dirty = true;
                return true;
            }

            return false;
        });

        if (dirty)
            Dirty(ent);
        return dirty;
    }

    /// <summary>
    /// Adds instability to an entity.
    /// </summary>
    public void AddInstability(Entity<MutatableComponent> ent, int instability)
    {
        if (instability == 0)
            return;

        ent.Comp.TotalInstability += instability;
        Dirty(ent);
    }

    #endregion
}
