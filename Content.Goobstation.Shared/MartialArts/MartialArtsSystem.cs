using System.Linq;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.Zones;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts;

public sealed partial class MartialArtsSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _modifier = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private ZoneSystem _zone = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var meleeAttackRateMultiplierQuery = EntityQueryEnumerator<MartialArtModifiersComponent>();
        while (meleeAttackRateMultiplierQuery.MoveNext(out var ent, out var multiplier))
        {
            if (_timing.CurTime < multiplier.NextUpdate)
                continue;

            double? nextUpdate = null;
            var refreshSpeed = false;
            for (var i = multiplier.Data.Count - 1; i >= 0; i--)
            {
                var data = multiplier.Data[i];

                if (_timing.CurTime < data.EndTime)
                {
                    nextUpdate = nextUpdate == null
                        ? data.EndTime.TotalSeconds
                        : Math.Min(nextUpdate.Value, data.EndTime.TotalSeconds);
                    continue;
                }

                if ((data.Type & MartialArtModifierType.MoveSpeed) != 0)
                    refreshSpeed = true;

                multiplier.Data.RemoveAt(i);
            }

            if (refreshSpeed)
                _modifier.RefreshMovementSpeedModifiers(ent);

            if (multiplier.Data.Count == 0)
                RemCompDeferred(ent, multiplier);
            else
            {
                if (nextUpdate != null)
                    multiplier.NextUpdate = TimeSpan.FromSeconds(nextUpdate.Value);
                Dirty(ent, multiplier);
            }
        }
    }

    public (float mult, float mod) GetMultiplierModifier(Entity<MartialArtModifiersComponent> ent,
        MartialArtModifierType type,
        bool? armed)
    {
        var mult = 1f;
        var mod = 0f;
        foreach (var data in ent.Comp.Data.Where(x => (x.Type & type) != 0))
        {
            if (armed is true)
            {
                if ((data.Type & MartialArtModifierType.Armed) == 0
                    && (data.Type & MartialArtModifierType.Unarmed) != 0)
                    continue;
            }
            else if (armed is false)
            {
                if ((data.Type & MartialArtModifierType.Unarmed) == 0
                    && (data.Type & MartialArtModifierType.Armed) != 0)
                    continue;
            }
            mult *= data.Multiplier;
            mod += data.Modifier;
        }

        foreach (var (_, limit) in ent.Comp.MinMaxModifiersMultipliers.Where(x => (x.Key & type) != 0))
        {
            mult = Math.Clamp(mult, limit.X, limit.Y);
            mod = Math.Clamp(mod, limit.Z, limit.W);
        }

        return (mult, mod);
    }

    public DamageModifierSet GetDamageModifierSet(DamageSpecifier specifier, float multiplier, float modifier)
    {
        return new()
        {
            Coefficients = specifier.DamageDict
                .ToDictionary(x => (ProtoId<DamageTypePrototype>) x.Key, _ => multiplier),
            FlatReduction = specifier.DamageDict
                .ToDictionary(x => (ProtoId<DamageTypePrototype>) x.Key, _ => -modifier), // Minus mod because it subtracts values from damage
        };
    }

    /// <summary>
    /// Tries to grant a martial art to a user. Use this method.
    /// </summary>
    [PublicAPI]
    public bool TryGrantMartialArt(
        Entity<MeleeWeaponComponent?> ent,
        MartialArtPrototype martialArt,
        List<ProtoId<ZonePrototype>> restrictedZones)
    {
        // currently cant really do shit without melee weapon
        if (!Resolve(ent, ref ent.Comp))
            return false;

        var beforeEv = new BeforeMartialArtKnowledgeAdded(martialArt);
        RaiseLocalEvent(ent, ref beforeEv);
        if (beforeEv.Cancelled)
            return false;

        if (HasMartialArt(ent.Owner, martialArt))
            return false;

        EnsureComp<MartialArtsKnowledgeComponent>(ent);
        var knowledge = GetKnowledge(ent.Owner);

        foreach (var entry in knowledge)
        {
            if (entry.MartialArtId != martialArt)
                continue;
            knowledge.Remove(entry); //dont want duplicates (slop) but lazy
        }

        var limitations = new MartialArtEntryLimitations(restrictedZones);
        var knowledgeEntry = new MartialArtKnowledgeEntry(martialArt, limitations);
        knowledge.Add(knowledgeEntry);

        var afterEv = new MartialArtKnowledgeAdded(martialArt);
        RaiseLocalEvent(ent, ref afterEv);

        return true;
    }

    [PublicAPI]
    private void RemoveMartialArt(
        Entity<MartialArtsKnowledgeComponent?> ent,
        MartialArtPrototype artToRemove
    )
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        var ev = new MartialArtKnowledgeRemoved(artToRemove);
        RaiseLocalEvent(ent, ref ev);

        ent.Comp.Knowledge.RemoveAll(x => x.MartialArtId == artToRemove);
    }

    /// <summary>
    /// Tries to remove the given martial arts from a user. Returns whether any were removed from the user.
    /// </summary>
    [PublicAPI]
    public bool TryRemoveMartialArts(
        Entity<MartialArtsKnowledgeComponent?> ent,
        List<MartialArtPrototype> martialArts)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        var didRemove = false;

        foreach (var artToRemove in martialArts)
        {
            if (!HasMartialArt(ent, artToRemove))
                continue;

            RemoveMartialArt(ent, artToRemove);
            didRemove = true;
        }

        return didRemove;
    }

    /// <summary>
    /// Tries to remove the given martial art from a user. Returns true if they had it and it was removed.
    /// </summary>
    [PublicAPI]
    public bool TryRemoveMartialArt(
        Entity<MartialArtsKnowledgeComponent?> ent,
        MartialArtPrototype martialArt)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!HasMartialArt(ent, martialArt))
            return false;

        RemoveMartialArt(ent, martialArt);

        return true;
    }

    /// <summary>
    /// Sorts the given martial art prototype list by their weights.
    /// </summary>
    [PublicAPI]
    public static List<MartialArtPrototype> SortByWeights(List<MartialArtPrototype> arts)
    {
        return [.. arts.OrderBy(x => x.Weight)];
    }

    /// <summary>
    /// Gets martial arts knowledge of an entity.
    /// </summary>
    [PublicAPI]
    public List<MartialArtKnowledgeEntry> GetKnowledge(Entity<MartialArtsKnowledgeComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return [];

        return ent.Comp.Knowledge;
    }

    /// <summary>
    /// Gets martial arts prototypes of an entity.
    /// </summary>
    [PublicAPI]
    public List<MartialArtPrototype> GetMartialArts(Entity<MartialArtsKnowledgeComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return [];

        return [.. ent.Comp.Knowledge.Select(x => _proto.Index(x.MartialArtId))];
    }

    /// <summary>
    /// Gets active martial arts knowledge of an entity.
    /// </summary>
    [PublicAPI]
    public List<MartialArtPrototype> GetActiveMartialArts(Entity<MartialArtsKnowledgeComponent?> ent)
    {
        List<MartialArtPrototype> available = [];
        if (!Resolve(ent, ref ent.Comp, false))
            return available;

        foreach (var knowledgeEntry in ent.Comp.Knowledge)
        {
            var art = _proto.Index(knowledgeEntry.MartialArtId);
            if (!HasMartialArtActive(ent, art))
            {
                Log.Error($"No has {art}");
                continue;
            }
            available.Add(art);
        }
        return available;
    }

    /// <summary>
    /// Returns whether an entity knows any martial art.
    /// </summary>
    [PublicAPI]
    public bool HasMartialArt(Entity<MartialArtsKnowledgeComponent?> ent)
    {
        return GetKnowledge(ent).Count > 0;
    }

    /// <summary>
    /// Returns whether an entity knows a specific martial art.
    /// </summary>
    [PublicAPI]
    public bool HasMartialArt(Entity<MartialArtsKnowledgeComponent?> ent, MartialArtPrototype proto)
    {
        return GetMartialArts(ent).Contains(proto);
    }

    /// <summary>
    /// Returns whether an entity knows a specific martial art, and it is active.
    /// Such as chef both knowing CQC and being inside a kitchen CQC zone
    /// </summary>
    [PublicAPI]
    public bool HasMartialArtActive(Entity<MartialArtsKnowledgeComponent?> ent, MartialArtPrototype proto)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        foreach (var art in GetKnowledge(ent))
        {
            if (art.MartialArtId != proto)
                continue;

            if (art.Limitations.RestrictedZones.Count > 0)
                return _zone.IsWithinAny(ent.Owner, art.Limitations.RestrictedZones);
            else
                return true;
        }

        return false;
    }

    public void ApplyMultiplier(EntityUid uid,
        float multiplier,
        float modifier,
        TimeSpan time,
        MartialArtModifierType type = MartialArtModifierType.AttackRate)
    {
        if (Math.Abs(multiplier - 1f) < 0.001f
            && Math.Abs(modifier) < 0.001f
            || time <= TimeSpan.Zero)
            return;

        var multComp = EnsureComp<MartialArtModifiersComponent>(uid);
        multComp.Data.Add(new MartialArtModifierData
        {
            Type = type,
            Multiplier = multiplier,
            Modifier = modifier,
            EndTime = _timing.CurTime + time,
        });

        Dirty(uid, multComp);
        _modifier.RefreshMovementSpeedModifiers(uid);
    }
}
