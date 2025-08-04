using Content.Goobstation.Shared.Overlays;
using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Shared.Actions;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This handles the Heightened Senses ability
/// The ability grants you thermal vision for a specific amount of time.
/// </summary>
public sealed class WerewolfHeightenedSensesSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfHeightenedSensesComponent, HeightenedSensesEvent>(OnHeightenedSenses);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WerewolfHeightenedSensesComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.AccumulationTime)
                continue;

            if (!comp.Activated)
                continue;

            ActivateThermal(uid);
            comp.Activated = false;
        }
    }

    private void OnHeightenedSenses(Entity<WerewolfHeightenedSensesComponent> ent, ref HeightenedSensesEvent args)
    {
        ActivateThermal(ent.Owner);
        ent.Comp.AccumulationTime = _timing.CurTime + ent.Comp.Duration;
        ent.Comp.Activated = true;

        _actions.StartUseDelay(args.Action);
    }

    /// <summary>
    /// Activates thermal vision
    /// </summary>
    /// <param name="uid"></param>
    private void ActivateThermal(EntityUid uid)
    {
        var ev = new ToggleThermalVisionEvent();
        RaiseLocalEvent(uid, ev);
    }
}
