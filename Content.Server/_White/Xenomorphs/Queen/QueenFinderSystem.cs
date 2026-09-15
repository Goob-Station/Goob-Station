// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Xenomorph;
using Content.Shared._White.Xenomorphs.Caste;
using Content.Shared._White.Xenomorphs.Ovipositor;
using Content.Shared._White.Xenomorphs.Queen;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Body.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.Queen;

/// <summary>
/// Drives xenomorph Queen Finder pinpointers with hive leadership priority and alert click cycling.
/// </summary>
public sealed class QueenFinderSystem : EntitySystem
{
    private static readonly ProtoId<XenomorphCastePrototype> EmpressCaste = "Empress";
    private static readonly ProtoId<XenomorphCastePrototype> QueenCaste = "Queen";
    private static readonly ProtoId<XenomorphCastePrototype> PraetorianCaste = "Praetorian";

    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPinpointerSystem _pinpointer = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<TransformComponent> _xformQuery;
    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
        SubscribeLocalEvent<QueenFinderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<QueenFinderComponent, CycleQueenFinderAlertEvent>(OnCycleAlert);
    }

    private void OnMapInit(Entity<QueenFinderComponent> ent, ref MapInitEvent args)
    {
        Refresh(ent);
    }

    private void OnCycleAlert(Entity<QueenFinderComponent> ent, ref CycleQueenFinderAlertEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!_xformQuery.TryGetComponent(ent.Owner, out var xform))
            return;

        var candidates = BuildCandidates(ent.Owner, xform.MapID);
        if (candidates.Count == 0)
        {
            ent.Comp.SelectedTarget = null;
            ApplyTarget(ent, null);
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-finder-none"), ent.Owner, ent.Owner);
            return;
        }

        var currentIndex = ent.Comp.SelectedTarget is { } selected
            ? candidates.IndexOf(selected)
            : -1;
        var nextIndex = (currentIndex + 1) % candidates.Count;
        var next = candidates[nextIndex];

        ent.Comp.SelectedTarget = next;
        Dirty(ent);
        ApplyTarget(ent, next);

        _popup.PopupEntity(
            Loc.GetString("xenomorphs-queen-finder-cycle", ("target", Identity.Entity(next, EntityManager))),
            ent.Owner,
            ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Pinpointer runs every frame; refresh leadership a bit slower.
        _accumulator += frameTime;
        if (_accumulator < 0.5f)
            return;

        _accumulator = 0f;

        var query = EntityQueryEnumerator<QueenFinderComponent, PinpointerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var finder, out var pin, out var xform))
        {
            if (!pin.IsActive)
                continue;

            Refresh((uid, finder), pin, xform);
        }
    }

    private void Refresh(Entity<QueenFinderComponent> ent, PinpointerComponent? pin = null, TransformComponent? xform = null)
    {
        if (!Resolve(ent.Owner, ref pin, ref xform, false))
            return;

        var candidates = BuildCandidates(ent.Owner, xform.MapID);
        if (candidates.Count == 0)
        {
            if (ent.Comp.SelectedTarget != null)
            {
                ent.Comp.SelectedTarget = null;
                Dirty(ent);
            }

            ApplyTarget(ent, null, pin);
            return;
        }

        if (ent.Comp.SelectedTarget is not { } selected || !candidates.Contains(selected))
        {
            // Highest priority first (already ordered).
            ent.Comp.SelectedTarget = candidates[0];
            Dirty(ent);
        }

        ApplyTarget(ent, ent.Comp.SelectedTarget, pin);
    }

    private void ApplyTarget(Entity<QueenFinderComponent> ent, EntityUid? target, PinpointerComponent? pin = null)
    {
        if (!Resolve(ent.Owner, ref pin, false))
            return;

        if (target == null || !Exists(target.Value))
        {
            _pinpointer.SetTargets(ent.Owner, new List<EntityUid>(), pin);
            return;
        }

        _pinpointer.SetTargets(ent.Owner, new List<EntityUid> { target.Value }, pin);
    }

    /// <summary>
    /// Priority: Empress → Queen → egg sack / ovipositor → Praetorian. Within a tier, nearer first.
    /// </summary>
    private List<EntityUid> BuildCandidates(EntityUid user, MapId mapId)
    {
        var tiers = new List<(EntityUid Uid, float DistSq)>[4];
        for (var i = 0; i < tiers.Length; i++)
            tiers[i] = new List<(EntityUid, float)>();

        var userPos = _xformQuery.TryGetComponent(user, out var userXform)
            ? _transform.GetWorldPosition(userXform)
            : System.Numerics.Vector2.Zero;

        var query = EntityQueryEnumerator<XenomorphComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var xeno, out var xform))
        {
            if (uid == user || xform.MapID != mapId || _mobState.IsDead(uid) || TerminatingOrDeleted(uid))
                continue;

            var tier = GetPriorityTier(uid, xeno);
            if (tier < 0)
                continue;

            var distSq = (_transform.GetWorldPosition(xform) - userPos).LengthSquared();
            tiers[tier].Add((uid, distSq));
        }

        var result = new List<EntityUid>();
        foreach (var tier in tiers)
        {
            foreach (var (uid, _) in tier.OrderBy(t => t.DistSq))
                result.Add(uid);
        }

        return result;
    }

    private int GetPriorityTier(EntityUid uid, XenomorphComponent xeno)
    {
        if (xeno.Caste == EmpressCaste)
            return 0;

        if (xeno.Caste == QueenCaste)
            return 1;

        if (HasComp<XenomorphOvipositorComponent>(uid) || HasEggSack(uid))
            return 2;

        if (xeno.Caste == PraetorianCaste)
            return 3;

        return -1;
    }

    private bool HasEggSack(EntityUid uid)
    {
        return _body.TryGetBodyOrganEntityComps<EggSackComponent>((uid, null), out _);
    }
}