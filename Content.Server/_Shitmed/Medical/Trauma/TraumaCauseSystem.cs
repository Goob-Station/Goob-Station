using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Explosion;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Shitmed.Medical.Trauma;

public sealed class TraumaCauseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    private static readonly FixedPoint2 CausedTraumaSeverity = 12;

    private readonly Dictionary<Type, List<(TraumaTypePrototype Proto, TraumaCause Cause)>> _causesByType = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, BeforeExplodeEvent>(OnBeforeExplode);

        BuildIndex();
        _prototype.PrototypesReloaded += OnPrototypesReloaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _prototype.PrototypesReloaded -= OnPrototypesReloaded;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<TraumaTypePrototype>())
            BuildIndex();
    }

    private void BuildIndex()
    {
        _causesByType.Clear();
        foreach (var proto in _prototype.EnumeratePrototypes<TraumaTypePrototype>())
        {
            if (proto.Causes == null)
                continue;

            foreach (var cause in proto.Causes)
            {
                if (!_causesByType.TryGetValue(cause.GetType(), out var list))
                {
                    list = new List<(TraumaTypePrototype, TraumaCause)>();
                    _causesByType[cause.GetType()] = list;
                }

                list.Add((proto, cause));
            }
        }
    }

    private void OnBeforeExplode(Entity<BodyComponent> ent, ref BeforeExplodeEvent args)
    {
        if (!_causesByType.TryGetValue(typeof(ExplosionCause), out var causes))
            return;

        var total = args.Damage.GetTotal();

        foreach (var (proto, causeBase) in causes)
        {
            var cause = (ExplosionCause) causeBase;
            if (total < cause.MinDamage)
                continue;

            var chance = cause.Chance;
            if (cause.ScaleWithDamage && cause.MinDamage > 0)
                chance *= (float) (total / cause.MinDamage);

            if (!_random.Prob(Math.Clamp(chance, 0f, 1f)))
                continue;

            if (TryPickPart(ent, cause, out var part))
                _trauma.TryInflictTrauma(part, proto.ID, CausedTraumaSeverity);
        }
    }

    /// <summary>
    /// Picks a random attached, non-severed woundable part the cause is allowed to hit.
    /// </summary>
    private bool TryPickPart(EntityUid body, TraumaCause cause, out Entity<WoundableComponent> part)
    {
        part = default;
        var candidates = new List<Entity<WoundableComponent>>();

        foreach (var (id, partComp) in _body.GetBodyChildren(body))
        {
            if (!TryComp<WoundableComponent>(id, out var woundable)
                || !cause.PartAllowed(partComp.PartType))
                continue;

            candidates.Add((id, woundable));
        }

        if (candidates.Count == 0)
            return false;

        part = _random.Pick(candidates);
        return true;
    }
}
