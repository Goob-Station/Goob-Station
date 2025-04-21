using System.Linq;
using Content.Goobstation.Common.DoAfter;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.DoAfter;

public sealed partial class CombatDoAfterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly ReactiveSystem _reactiveSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeToggledEvent>(OnToggle);
        SubscribeLocalEvent<CombatDoAfterComponent, HandSelectedEvent>(OnSelected);
        SubscribeLocalEvent<CombatDoAfterComponent, HandDeselectedEvent>(OnDeselected);
        SubscribeLocalEvent<CombatDoAfterComponent, DroppedEvent>(OnDropped);

        InitializeTriggers();
    }

    private void OnDropped(Entity<CombatDoAfterComponent> ent, ref DroppedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        TryCancelDoAfter(ent);
    }

    private void OnDeselected(Entity<CombatDoAfterComponent> ent, ref HandDeselectedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        TryCancelDoAfter(ent);
    }

    private void OnSelected(Entity<CombatDoAfterComponent> ent, ref HandSelectedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!_combat.IsInCombatMode(args.User))
            return;

        ResetDoAfter(ent, args.User, true);
    }

    private void OnToggle(ref CombatModeToggledEvent ev)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!TryComp(ev.User, out HandsComponent? hands))
            return;

        var item = _hands.GetActiveItem((ev.User, hands));

        ResetDoAfter(item, ev.User, ev.Activated);
    }

    private void ResetDoAfter(EntityUid? item,
        EntityUid user,
        bool combatModeActivated,
        CombatDoAfterComponent? combatDoAfter = null)
    {
        if (item == null || !Resolve(item.Value, ref combatDoAfter, false))
            return;

        var dirty = !TryCancelDoAfter((item.Value, combatDoAfter));

        if (!combatModeActivated)
            return;

        var rand = new Random((int) _timing.CurTick.Value);
        var delay = rand.NextFloat(combatDoAfter.Delay - combatDoAfter.DelayVariation,
            combatDoAfter.Delay + combatDoAfter.DelayVariation);

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            delay,
            new CombatDoAfterEvent(),
            item.Value,
            null,
            item.Value)
        {
            Hidden = combatDoAfter.Hidden,
            BreakOnMove = combatDoAfter.BreakOnMove,
            BreakOnWeightlessMove = combatDoAfter.BreakOnWeightlessMove,
            BreakOnDamage = combatDoAfter.BreakOnDamage,
            MultiplyDelay = combatDoAfter.MultiplyDelay,
            ColorOverride = combatDoAfter.ColorOverride,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out var id))
            return;

        combatDoAfter.DoAfterId = id.Value.Index;
        combatDoAfter.DoAfterUser = user;
        if (dirty)
            Dirty(item.Value, combatDoAfter);
    }

    private bool CheckDoAfter(Entity<CombatDoAfterComponent> item, EntityUid? user, IReadOnlyList<EntityUid>? targets)
    {
        if (item.Comp.DoAfterUser == null || user != null && item.Comp.DoAfterUser != user ||
            item.Comp.DoAfterId == null)
            return user != null && targets != null && item.Comp.AlwaysTriggerOnSelf && targets.Contains(user.Value);

        if (targets != null && item.Comp.AlwaysTriggerOnSelf && targets.Contains(item.Comp.DoAfterUser.Value))
            return true;

        if (!TryComp(item.Comp.DoAfterUser.Value, out DoAfterComponent? doAfterComp))
            return false;

        if (!_doAfter.TryGetDoAfter(doAfterComp, item.Comp.DoAfterId.Value, out var doAfter))
            return false;

        if (doAfter.Cancelled)
            return false;

        var difference = _timing.CurTime - doAfter.StartTime - doAfter.Args.Delay;

        return Math.Abs(difference.TotalSeconds) < item.Comp.ActivationTolerance;
    }

    private bool TryCancelDoAfter(Entity<CombatDoAfterComponent> ent)
    {
        if (ent.Comp is { DoAfterUser: not null, DoAfterId: not null })
        {
            CancelDoAfter(ent.Comp.DoAfterUser.Value, ent.Comp.DoAfterId.Value);
            ent.Comp.DoAfterUser = null;
            ent.Comp.DoAfterId = null;
            Dirty(ent);
            return true;
        }

        return false;
    }

    private void CancelDoAfter(EntityUid uid, ushort doAfterId)
    {
        if (!TryComp(uid, out DoAfterComponent? component))
            return;

        if (!_doAfter.TryGetDoAfter(component, doAfterId, out _))
            return;

        _doAfter.Cancel(uid, doAfterId, component);
    }
}
